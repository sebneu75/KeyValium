
namespace KeyValium.Collections
{
    /// <summary>
    /// Dictionary for tuples of KvPageNumber and values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class KvDictionary<T> where T : struct
    {
        public const int DefaultSize = HashKeyValueAllocator<T>.DefaultInitialSize;

        public KvDictionary(): this (DefaultSize)
        {
            Perf.CallCount();
        }

        public KvDictionary(int itemcount)
        {
            Perf.CallCount();

            _allocator = new HashKeyValueAllocator<T>(itemcount);
        }

        private HashKeyValueAllocator<T> _allocator;

        internal PageRangeList ToRangeList()
        {
            Perf.CallCount();

            var list = new PageRangeList();

            _allocator.ForEach(pageno => list.AddPage(pageno));

            return list;
        }

        internal SortedDictionary<int, int> GetBucketStats()
        {
            Perf.CallCount();

            return _allocator.GetBucketStats();
        }

        #region public API

        internal void ForEach(HashKeyValueAllocator<T>.KeyIterator action)
        {
            Perf.CallCount();

            _allocator.ForEach(action);
        }

        internal void ForEach(HashKeyValueAllocator<T>.KeyValueIterator action)
        {
            Perf.CallCount();

            _allocator.ForEach(action);
        }

        public void Clear()
        {
            Perf.CallCount();

            _allocator.Clear();
        }

        public int Count
        {
            get
            {
                Perf.CallCount();

                return _allocator.Count;
            }
        }

        public void Add(KvPagenumber pageno, ref T val)
        {
            Perf.CallCount();

            Add(pageno, ref val, true);
        }

        public void Upsert(KvPagenumber pageno, ref T val)
        {
            Perf.CallCount();

            Add(pageno, ref val, false);
        }

        private void Add(KvPagenumber pageno, ref T val, bool throwifexists)
        {
            Perf.CallCount();

            ref var item = ref _allocator.TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                if (throwifexists)
                {
                    throw new NotSupportedException("Pagenumber already exists!");
                }

                item = val;
            }
            else
            {
                // TODO use ref parameter
                _allocator.Add(pageno, ref val);
            }
        }

        public bool Remove(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _allocator.Remove(pageno);
        }

        public bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _allocator.Contains(pageno);
        }

        public ref T TryGetValueRef(KvPagenumber pageno, out bool isvalid)
        {
            Perf.CallCount();

            return ref _allocator.TryGetValueRef(pageno, out isvalid);
        }

        private List<KeyValuePair<KvPagenumber, T>> _list;

        internal List<KeyValuePair<KvPagenumber, T>> ToList()
        {
            Perf.CallCount();

            _list = new List<KeyValuePair<KvPagenumber, T>>();

            _allocator.ForEach(AddToList);

            var ret = _list;
            _list = null;
            return ret;
        }

        internal void AddToList(KvPagenumber pageno, ref T item)
        {
            Perf.CallCount();

            _list.Add(new KeyValuePair<KvPagenumber, T>(pageno, item));
        }


        #endregion
    }
}
