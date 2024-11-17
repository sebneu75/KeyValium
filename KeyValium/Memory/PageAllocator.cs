namespace KeyValium.Memory
{
    internal unsafe sealed class PageAllocator : IDisposable
    {
        /// <summary>
        /// minimum number of pages to allocate in one call
        /// </summary>
        internal const int MinPageCount = 16;

        /// <summary>
        /// maximum number of bytes to allocate in one call
        /// </summary>
        internal const int MaxChunkSize = 4 * 1024 * 1024;

        internal PageAllocator(uint pagesize, bool zeropages)
        {
            Perf.CallCount();

            PageSize = pagesize;

            //switch (pagesize)
            //{
            //    case 4096:
            //        ZeroPage = MemUtils.ZeroPage4K;
            //        CopyPage = MemUtils.CopyPage4K;
            //        break;

            //    default:
            //        ZeroPage = MemUtils.ZeroPage256;
            //        CopyPage = MemUtils.CopyPage256;
            //        break;
            //}

            // TODO fix option
            _zeromemory = zeropages;

            _maxpagestoallocate = MaxChunkSize / (int)PageSize;

            _maxqueuecount = _maxpagestoallocate * 8;
        }

        //internal delegate void ZeroPage_D(byte* pointer, int size);

        //internal readonly ZeroPage_D ZeroPage;

        //internal delegate void CopyPage_D(byte* target, byte* source, int size);

        //internal readonly CopyPage_D CopyPage;

        internal readonly uint PageSize;

        private readonly object _lock = new();

        private readonly bool _zeromemory;

        private readonly int _maxpagestoallocate;

        private readonly int _maxqueuecount;

        private int _pagecount = MinPageCount;

        private Queue<AnyPage> _queue = new Queue<AnyPage>(64);

        private HashSet<AnyPage> _usedpages = new HashSet<AnyPage>(64);

        // number of allocated pages
        private ulong _allocated;

        // total number of page usages
        private ulong _used;

        // total number of page recycles
        private ulong _recycled;

        // number of deallocated pages
        private ulong _deallocated;

        // number of copied pages with same pagenumber
        private ulong _copied;

        // number of copied pages with new pagenumber
        private ulong _copiednew;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageno"></param>
        /// <param name="createheader"></param>
        /// <param name="initpagetype"></param>
        /// <param name="tid"></param>
        /// <returns>the page with incremented refcount</returns>
        /// <exception cref="NotSupportedException"></exception>
        internal AnyPage GetPage(KvPagenumber pageno, bool createheader, ushort? initpagetype, KvTid tid)
        {
            Perf.CallCount();

            AnyPage page = null;
            KvPagenumber oldpageno = 0;

            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    AllocatePages();
                }

                if (_queue.Count > 0)
                {
                    page = _queue.Dequeue();

                    if (page.RefCount != 0)
                    {
                        throw new NotSupportedException("RefCount of AnyPage is not zero!");
                    }

                    if (!_usedpages.Add(page))
                    {
                        throw new NotSupportedException("AnyPage is already in use!");
                    }

                    _used++;

                    oldpageno = page.PageNumber;
                    page.IsInUse = true;
                    page.AddRef();
                }
                else
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "No AnyPage available!");
                }
            }

            // do initialization outside of lock
            page.Initialize(pageno, createheader, initpagetype, tid);

            KvDebug.Assert(!createheader || page.PageType == page.Header.PageType, "FAIL");

            Logger.LogInfo(LogTopics.Allocation, tid, "Reused AnyPage old:{0} new:{1}", oldpageno, page.PageNumber);

            return page;
        }

        internal void AllocatePages()
        {
            Perf.CallCount();

            // allocate _pagecount pages
            for (int i = 0; i < _pagecount; i++)
            {
                var page = new AnyPage(this, PageSize);
                _queue.Enqueue(page);
                _allocated++;
            }

            if (_pagecount < _maxpagestoallocate)
            {
                // double for next cycle
                _pagecount <<= 1;
            }
        }

        internal void Recycle(AnyPage page)
        {
            Perf.CallCount();

            lock (_lock)
            {
                if (page.RefCount != 0)
                {
                    throw new NotSupportedException("RefCount of recycled AnyPage is not zero!");
                }

                page.IsInUse = false;

                if (!_usedpages.Remove(page))
                {
                    throw new NotSupportedException("Recycled AnyPage was not in use!");
                }

                if (_zeromemory)
                {
                    page.Bytes.Span.Clear();
                }

                if (_queue.Count < _maxqueuecount)
                {
                    // recycle
                    _queue.Enqueue(page);
                    _recycled++;
                }
                else
                {
                    // deallocate
                    page.Deallocate();
                    _deallocated++;
                }

                Logger.LogInfo(LogTopics.Allocation, "Allocator.Recycle(): recycled page {0}", page.PageNumber);
            }
        }

        /// <summary>
        /// makes a copy of the page with a new pagenumber 
        /// </summary>
        /// <param name="source">Page to copy</param>
        /// <param name="newpageno">new pagenumber</param>
        /// <param name="tid">Tid of the transaction</param>
        /// <returns></returns>
        internal unsafe AnyPage GetCopy(AnyPage source, KvPagenumber newpageno, KvTid tid)
        {
            Perf.CallCount();

            var target = GetPage(newpageno, false, null, 0);

            //CopyPage(target.Pointer, source.Pointer, (int)PageSize);
            MemUtils.MemoryCopy(target.Pointer, source.Pointer, (int)PageSize);
            //source.Bytes.ReadOnlySpan.CopyTo(target.Bytes.Span);

            if (source.PageType != PageTypes.Raw)
            {
                target.CreateHeaderAndContent(source.PageType, tid);
            }

            _copiednew++;

            return target;
        }

        /// <summary>
        /// makes a copy of the page
        /// used in child transactions
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal unsafe AnyPage GetCopy(AnyPage source)
        {
            Perf.CallCount();

            var target = GetPage(source.PageNumber, false, null, 0);

            //CopyPage(target.Pointer, source.Pointer, (int)PageSize);
            MemUtils.MemoryCopy(target.Pointer, source.Pointer, (int)PageSize);
            //source.Bytes.ReadOnlySpan.CopyTo(target.Bytes.Span);

            if (source.PageType != PageTypes.Raw)
            {
                target.CreateHeaderAndContent(null, 0);
            }

            _copied++;

            return target;
        }

        internal PageAllocatorStats GetStats()
        {
            Perf.CallCount();

            lock (_lock)
            {
                return new PageAllocatorStats(_usedpages, _queue.Count, _usedpages.Count, _allocated, _used, _recycled, _deallocated, _copied, _copiednew);
            }
        }

        internal void ClearStats()
        {
            Perf.CallCount();

            lock (_lock)
            {
                _copied = 0;
                _copiednew = 0;
            }
        }

        #region IDisposable

        private bool disposedValue;
        private bool disposedValue1;

        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            lock (_lock)
            {
                if (!disposedValue)
                {
                    //if (disposing)
                    //{

                    if (_usedpages.Count > 0)
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, "One or more pages are still in use while disposing the Allocator.");
                    }

                    // deallocate all queued pages because they are pinned
                    while (_queue.Count > 0)
                    {
                        var page = _queue.Dequeue();

                        if (page.RefCount != 0)
                        {
                            var msg = string.Format("RefCount is not zero while disposing the Allocator. (Page {0}: RefCount {1})", page.PageNumber, page.RefCount);
                            throw new KeyValiumException(ErrorCodes.InternalError, msg);
                        }

                        page.Deallocate();
                        _deallocated++;
                    }

                    if (_allocated != _deallocated)
                    {
                        var msg = string.Format("The number of allocated pages does not match the number of deallocated ones. (Allocated: {0} Deallocated: {1})", _allocated, _deallocated);
                        throw new KeyValiumException(ErrorCodes.InternalError, msg);
                    }

                    //}
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
