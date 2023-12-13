
namespace KeyValium.Collections
{
    /// <summary>
    /// HashSet of KvPageNumber
    /// </summary>
    internal class KvHashSet
    {
        public KvHashSet()
        {
            Perf.CallCount();

            _allocator = new HashKeyAllocator();
        }

        private HashKeyAllocator _allocator;

        public void Clear()
        {
            Perf.CallCount();

            _allocator.Clear();
        }

        #region public API

        public int Count
        {
            get
            {
                Perf.CallCount();

                return _allocator.Count;
            }
        }

        public bool Add(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _allocator.Add(pageno, out _);
        }

        public bool Remove(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _allocator.Remove(pageno, out _);
        }

        public bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _allocator.Contains(pageno, out _);
        }

        #endregion

        internal void AddRange(KvHashSet other)
        {
            Perf.CallCount();

            other._allocator.ForEach(pageno => Add(pageno));
        }

        internal void AddRange(IEnumerable<KvPagenumber> items)
        {
            Perf.CallCount();

            foreach (var item in items)
            {
                Add(item);
            }
        }

        internal void RemoveRange(KvHashSet other)
        {
            Perf.CallCount();

            other._allocator.ForEach(pageno => Remove(pageno));
        }

        internal PageRangeList ToRangeList()
        {
            Perf.CallCount();

            var list = new PageRangeList();

            _allocator.ForEach(pageno => list.AddPage(pageno));

            return list;
        }

        internal List<KvPagenumber> ToList()
        {
            Perf.CallCount();

            var list = new List<KvPagenumber>(Count);

            _allocator.ForEach(pageno => list.Add(pageno));

            return list;
        }

        internal void ForEach(HashKeyAllocator.KeyIterator action)
        {
            Perf.CallCount();

            _allocator.ForEach(action);
        }
    }
}
