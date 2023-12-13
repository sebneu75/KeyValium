
namespace KeyValium.Collections
{
    /// <summary>
    /// Maintains a sorted list of non-overlapping PageRanges
    /// Ranges are automatically merged or splitted
    /// Implemented with a red black tree
    /// </summary>
    internal sealed class PageRangeList
    {
        public PageRangeList()
        {
            Perf.CallCount();

            _ranges = new RedBlackTree<PageRange>();
        }

        private PageRangeList(RedBlackTree<PageRange> ranges)
        {
            Perf.CallCount();

            _ranges = ranges;
        }

        #region Variables

        readonly RedBlackTree<PageRange> _ranges;

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get
            {
                Perf.CallCount();

                return _ranges.Count == 0;
            }
        }

        public int RangeCount
        {
            get
            {
                Perf.CallCount();

                return _ranges.Count;
            }
        }

        public ulong PageCount
        {
            get
            {
                Perf.CallCount();

                // TODO maintain pagecount variable
                ulong sum = 0;

                if (_ranges.TryGetMin(out var index))
                {
                    do
                    {
                        ref var range = ref _ranges.GetItem(index);
                        sum += range.PageCount;
                    }
                    while (_ranges.TryGetNext(index, out index));
                }

                return sum;
            }
        }

        #endregion

        #region Public API

        internal void Clear()
        {
            Perf.CallCount();

            _ranges.Clear();
        }

        public bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return ContainsRange(pageno, pageno);
        }

        internal bool ContainsRange(KvPagenumber first, KvPagenumber last)
        {
            Perf.CallCount();

            return ContainsRange(new PageRange(first, last));
        }

        internal bool ContainsRange(PageRange range)
        {
            Perf.CallCount();

            var index = _ranges.FindMaxLeq(range);
            if (index >= 0)
            {
                ref var range1 = ref _ranges.GetItem(index);
                return range1.Contains(range);
            }

            return false;
        }

        internal void AddRange(KvPagenumber first, KvPagenumber last)
        {
            Perf.CallCount();

            AddRange(new PageRange(first, last));
        }

        internal void AddRange(PageRange range)
        {
            Perf.CallCount();

            // TODO optimize
            var index1 = _ranges.FindMaxLeqOrMin(range);
            if (index1 >= 0)
            {
                ref var range1 = ref _ranges.GetItem(index1);
                if (range1.Overlaps(ref range))
                {
                    throw new ArgumentException("Range already exists partially.");
                }

                if (range1.Last == range.First - 1)
                {
                    range1.Last = range.Last;
                    MergeWithNext(index1);

                    return;
                }
                else if (range1.First == range.Last + 1)
                {
                    range1.First = range.First;
                    // not needed because already checked in previous case
                    // MergeWithPrev(index1);

                    return;
                }
                else
                {
                    if (_ranges.TryGetNext(index1, out var index2))
                    {
                        ref var range2 = ref _ranges.GetItem(index2);
                        if (range2.First == range.Last + 1)
                        {
                            range2.First = range.First;

                            // not needed because already checked in previous case
                            //MergeWithPrev(index2);

                            return;
                        }
                    }
                }
            }

            _ranges.Insert(range);

            ValidateRanges();
        }

        internal void AddRanges(PageRangeList rangelist)
        {
            Perf.CallCount();

            if (rangelist._ranges.TryGetMin(out var index))
            {
                do
                {
                    ref var range = ref rangelist._ranges.GetItem(index);
                    AddRange(range);
                }
                while (rangelist._ranges.TryGetNext(index, out index));
            }
        }

        internal void AddPage(KvPagenumber pageno)
        {
            Perf.CallCount();

            AddRange(pageno, pageno);
        }

        internal void RemovePage(KvPagenumber pageno)
        {
            Perf.CallCount();

            RemoveRange(new PageRange(pageno, pageno));
        }

        internal void RemoveRange(KvPagenumber first, KvPagenumber last)
        {
            Perf.CallCount();

            RemoveRange(new PageRange(first, last));
        }

        internal void RemoveRange(PageRange range)
        {
            Perf.CallCount();

            // TODO optimize
            var index1 = _ranges.FindMaxLeq(range);
            if (index1 < 0)
            {
                throw new ArgumentException("Range not found.");
            }

            ref var range1 = ref _ranges.GetItem(index1);
            if (range1.Contains(range))
            {
                if (range1.First == range.First && range1.Last == range.Last)
                {
                    _ranges.Remove(range1);
                }
                else if (range1.First == range.First)
                {
                    range1.First = range.Last + 1;
                }
                else if (range1.Last == range.Last)
                {
                    range1.Last = range.First - 1;
                }
                else
                {
                    // split range
                    var temp = range1.Last;
                    range1.Last = range.First - 1;

                    _ranges.Insert(new PageRange(range.Last + 1, temp));
                }
            }
            else
            {
                throw new ArgumentException("Range not found.");
            }

            ValidateRanges();
        }

        public void RemovePageIfExists(KvPagenumber pageno)
        {
            Perf.CallCount();

            RemoveAllInRange(new PageRange(pageno, pageno));
        }

        public void RemoveAllInRange(KvPagenumber first, KvPagenumber last)
        {
            Perf.CallCount();

            RemoveAllInRange(new PageRange(first, last));
        }

        internal void RemoveAllInRange(PageRange range)
        {
            Perf.CallCount();

            var removees = new List<PageRange>();

            var index1 = _ranges.FindMaxLeqOrMin(range);
            while (index1 >= 0)
            {
                ref var range1 = ref _ranges.GetItem(index1);
                if (range1.Contains(range))
                {
                    if (range1.First == range.First && range1.Last == range.Last)
                    {
                        _ranges.Remove(range1);
                    }
                    else if (range1.First == range.First)
                    {
                        range1.First = range.Last + 1;
                    }
                    else if (range1.Last == range.Last)
                    {
                        range1.Last = range.First - 1;
                    }
                    else
                    {
                        // split range
                        var temp = range1.Last;
                        range1.Last = range.First - 1;

                        _ranges.Insert(new PageRange(range.Last + 1, temp));
                    }

                    break;
                }
                else
                {
                    if (range.Contains(range1))
                    {
                        removees.Add(range1);
                    }
                    else if (range1.Contains(range.First))
                    {
                        range1.Last = range.First - 1;
                    }
                    else if (range1.Contains(range.Last))
                    {
                        range1.First = range.Last + 1;
                        break;
                    }

                    _ranges.TryGetNext(index1, out index1);
                }
            }

            foreach (var range2 in removees)
            {
                _ranges.Remove(range2);
            }

            ValidateRanges();
        }

        internal PageRangeList Copy()
        {
            Perf.CallCount();

            return new PageRangeList(_ranges.Copy());
        }

        internal List<PageRange> ToList()
        {
            Perf.CallCount();

            // TODO move to tree
            var ret = new List<PageRange>(_ranges.Count);

            if (_ranges.TryGetMin(out var index))
            {
                do
                {
                    ref var range = ref _ranges.GetItem(index);
                    ret.Add(range);
                }
                while (_ranges.TryGetNext(index, out index));
            }

            return ret;
        }

        /// <summary>
        /// returns a single page and removes it from the list
        /// </summary>
        /// <param name="pageno">the pagenumber</param>    
        /// <returns>true on success, false if no page is available</returns>
        /// <exception cref="NotImplementedException"></exception>
        internal bool TryTakePage(out KvPagenumber pageno)
        {
            Perf.CallCount();

            if (_ranges.TryGetMin(out int index))
            {
                ref var range = ref _ranges.GetItem(index);
                pageno = range.First;
                if (range.PageCount > 1)
                {
                    range.First++;
                }
                else
                {
                    _ranges.Remove(range);
                }

                return true;
            }

            pageno = default;

            return false;
        }

        /// <summary>
        /// returns a single page and removes it from the list
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal KvPagenumber? TakePage()
        {
            Perf.CallCount();

            if (TryTakePage(out var pageno))
            {
                return pageno;
            }

            return null;
        }

        internal bool TryGetLast(out PageRange range)
        {
            Perf.CallCount();

            if (_ranges.TryGetMax(out var index))
            {
                ref var r = ref _ranges.GetItem(index);
                range = r;
                return true;
            }

            range = default;

            return false;
        }
        /// <summary>
        /// returns a range of pages and removes it from the list
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal bool TryTakeRange(KvPagenumber startpageno, ulong count, out PageRange rangeout)
        {
            Perf.CallCount();

            if (TryGetRangeOfLength(startpageno, count, out var index))
            {
                ref var range = ref _ranges.GetItem(index);
                if (range.PageCount == count)
                {
                    // make copy
                    rangeout = range;
                    _ranges.Remove(range);

                    return true;
                }
                else
                {
                    rangeout = new PageRange(range.First, range.First + count - 1);
                    range.First += count;
                }

                ValidateRanges();

                return true;
            }

            rangeout = PageRange.Empty;

            return false;
        }

        #endregion

        #region Private

        private void MergeWithNext(int index)
        {
            Perf.CallCount();

            if (_ranges.TryGetNext(index, out var next))
            {
                ref var range = ref _ranges.GetItem(index);
                ref var right = ref _ranges.GetItem(next);

                if (right.First == range.Last + 1)
                {
                    range.Last = right.Last;
                    _ranges.Remove(right);
                }

                ValidateRanges();
            }
        }

        private void MergeWithPrev(int index)
        {
            Perf.CallCount();

            if (_ranges.TryGetPrev(index, out var prev))
            {
                ref var range = ref _ranges.GetItem(index);
                ref var left = ref _ranges.GetItem(prev);

                if (left.Last == range.First - 1)
                {
                    left.Last = range.Last;
                    _ranges.Remove(range);
                }

                ValidateRanges();
            }
        }

        /// <summary>
        /// Gets the first range that contains at least count pages
        /// </summary>
        /// <param name="pageno"></param>
        /// <returns></returns>
        private bool TryGetRangeOfLength(KvPagenumber startpageno, ulong count, out int index)
        {
            Perf.CallCount();

            index = -1;

            var startindex = _ranges.FindMinGeq(new PageRange(startpageno, startpageno));
            if (startindex >= 0)
            {
                do
                {
                    ref var range = ref _ranges.GetItem(startindex);
                    if (range.PageCount >= count)
                    {
                        index = startindex;
                        return true;
                    }
                }
                while (_ranges.TryGetNext(startindex, out startindex));
            }

            return false;
        }

        #endregion

        #region Debug

        [Conditional("DEBUG")]
        private void ValidateRanges()
        {
            if (_ranges.TryGetMin(out var index))
            {
                do
                {
                    ref var range = ref _ranges.GetItem(index);
                    if (range.First > range.Last)
                    {
                        throw new ArgumentException("Range is negative.");
                    }

                    if (_ranges.TryGetPrev(index, out var previndex))
                    {
                        ref var prevrange = ref _ranges.GetItem(previndex);

                        if (prevrange.First >= range.First)
                        {
                            throw new ArgumentException("Sortorder is wrong.");
                        }

                        if (prevrange.Last >= range.First)
                        {
                            throw new ArgumentException("Overlapping ranges.");
                        }
                    }
                }
                while (_ranges.TryGetNext(index, out index));
            }
        }

        #endregion
    }
}
