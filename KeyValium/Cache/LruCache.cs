using KeyValium.Collections;
using KeyValium.Inspector;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace KeyValium.Cache
{
    /// <summary>
    /// A cache for pages. Maintains a last recently used list of pages.
    /// Pagenumber 0 is never put in the cache. 0 is used as a sentinel value for instance in GetPage
    /// </summary>
    internal sealed class LruCache : IDisposable
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="maxitems">Maximum number of cached pages</param>
        public LruCache(int maxitems)
        {
            Perf.CallCount();

            MaxItems = maxitems;

            _pages = new KvDictionary<PageRef>(MaxItems + 4);
            _list = new KvList<KvPagenumber>(MaxItems + 4);
        }

        /// <summary>
        /// Maximum number of cached pages
        /// </summary>
        internal readonly int MaxItems;

        /// <summary>
        /// a dictionary that contains the pages
        /// </summary>
        internal readonly KvDictionary<PageRef> _pages;

        /// <summary>
        /// the last recently used list
        /// </summary>
        internal readonly KvList<KvPagenumber> _list;

        #region Stats

        /// <summary>
        /// returns some statistics of the LruCache
        /// </summary>
        /// <returns>Statistics</returns>
        internal CacheStats GetStats()
        {
            Perf.CallCount();

            return new CacheStats(_pages.Count, MaxItems, _hits, _misses, _pages.GetBucketStats(), _pages.ToRangeList());
        }

        /// <summary>
        /// resets the hits and misses count to zero
        /// </summary>
        internal void ClearStats()
        {
            Perf.CallCount();

            _hits = 0;
            _misses = 0;
        }

        private ulong _hits = 0;

        private ulong _misses = 0;

        #endregion

        /// <summary>
        /// copies the content of this cache to another
        /// </summary>
        /// <param name="other"></param>
        internal void CopyTo(LruCache other)
        {
            Perf.CallCount();

            void Copy(KvPagenumber pageno, ref PageRef item)
            {
                item.Page?.AddRef();
                other.UpsertPage(ref item);
            }

            _pages.ForEach(Copy);
        }

        /// <summary>
        /// rermoves the content of this cache from another
        /// </summary>
        /// <param name="other"></param>
        internal void RemoveFrom(LruCache other)
        {
            Perf.CallCount();

            void Remove(KvPagenumber pageno)
            {
                other.RemovePage(pageno);
            }

            _pages.ForEach(Remove);
        }

        /// <summary>
        /// returns the page with the given pagenumber
        /// </summary>
        /// <param name="pageno">the pagenumber of the requested page</param>
        /// <returns>an instance of PageRef2. if PageNumber is zero the page has not been found</returns>
        public ref PageRef GetPage(KvPagenumber pageno, out bool isvalid)
        {
            Perf.CallCount();

            ref var val = ref _pages.TryGetValueRef(pageno, out isvalid);
            if (isvalid)
            {
                _list.MoveToFirst(val.Slot);
                _hits++;
            }
            else
            {
                _misses++;
            }

            return ref val;
        }

        /// <summary>
        /// updates or inserts a page in the cache
        /// </summary>
        /// <param name="pageref">The PageRef containing the Page and the Pagenumber</param>
        public void UpsertPage(ref PageRef pageref)
        {
            Perf.CallCount();

            //KvDebug.Assert(pageref.Page.State == PageStates.Clean || pageref.Page.State == PageStates.Spilled || pageref.Page.State == PageStates.Dirty, "Only clean, dirty or spilled pages can be put in the cache!");
            ref var val = ref _pages.TryGetValueRef(pageref.PageNumber, out var isvalid);
            if (isvalid)
            {
                _list.MoveToFirst(val.Slot);

                // save slot
                pageref.Slot = val.Slot;

                // clear Page because of refcounting
                val.Page = null;

                val = pageref;
            }
            else
            {
                // update slot
                pageref.Slot = _list.InsertFirst(pageref.PageNumber);

                _pages.Add(pageref.PageNumber, ref pageref);

                EnsureMaxItemCount();
            }
        }

        /// <summary>
        /// removes the given page number from the cache
        /// </summary>
        /// <param name="pageno">page number to be removed</param>
        public void RemovePage(KvPagenumber pageno)
        {
            Perf.CallCount();

            ref var node = ref _pages.TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                node.Page = null;
                _list.Remove(node.Slot);
                _pages.Remove(pageno);
            }
        }

        internal void RemoveLastPage()
        {
            Perf.CallCount();

            if (_list.RemoveLast(out var pageno))
            {
                ref var node = ref _pages.TryGetValueRef(pageno, out var isvalid);
                if (isvalid)
                {
                    node.Page = null;
                    _pages.Remove(pageno);
                }
                else
                {
                    throw new InvalidOperationException("Could not find page to remove.");
                }
            }
        }

        /// <summary>
        /// clears the cache
        /// </summary>
        public void Clear()
        {
            Perf.CallCount();

            void Clear(KvPagenumber pageno, ref PageRef item)
            {
                item.Page = null;
            }

            _pages.ForEach(Clear);

            _pages.Clear();
            _list.Clear();
        }

        /// <summary>
        /// ensures the maximum item count by removing elements from the cache
        /// </summary>
        private void EnsureMaxItemCount()
        {
            Perf.CallCount();

            if (MaxItems < 0)
            {
                return;
            }

            while (_pages.Count > MaxItems)
            {
                RemoveLastPage();
            }
        }

        #region IDisposable

        private bool disposedValue;

        /// <summary>
        /// clears the cache and disposes it
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!disposedValue)
            {
                if (disposing)
                {
                    Clear();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the cache
        /// </summary>
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

