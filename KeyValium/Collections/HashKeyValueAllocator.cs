
namespace KeyValium.Collections
{
    /// <summary>
    /// Manages an array of items, a freelist and an array of buckets
    /// elements can be allocated and released
    /// free items form a single linked list via the Next property
    /// array is resized (grown only) automatically
    /// </summary>
    internal sealed class HashKeyValueAllocator<T> where T : struct
    {
        public const int DefaultInitialSize = 32;

        public HashKeyValueAllocator() : this(DefaultInitialSize)
        {
            Perf.CallCount();
        }

        public HashKeyValueAllocator(int initialsize)
        {
            Perf.CallCount();

            Initialize(initialsize);
        }

        #region Variables

        HashKeyAllocator _keys;

        internal T[] _values;

        internal T Sentinel = default;

        #endregion

        internal SortedDictionary<int, int> GetBucketStats()
        {
            return _keys.GetBucketStats();
        }

        internal void Initialize(int itemcount)
        {
            Perf.CallCount();

            _keys = new HashKeyAllocator(itemcount);

            _values = new T[_keys._items.Length];
        }

        public void Clear()
        {
            Perf.CallCount();

            Initialize(DefaultInitialSize);
        }

        public int Count
        {
            get
            {
                Perf.CallCount();

                return _keys.Count;
            }
        }

        public bool Add(KvPagenumber pageno, ref T item)
        {
            Perf.CallCount();

            var ret = _keys.Add(pageno, out var slotindex);

            EnsureSameSize();

            _values[slotindex] = item;

            return ret;
        }

        private void EnsureSameSize()
        {
            Perf.CallCount();

            if (_values.Length == _keys._items.Length)
            {
                return;
            }

            var newvalues = new T[_keys._items.Length];
            Array.Copy(_values, newvalues, _values.Length);

            _values = newvalues;
        }

        public bool Remove(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _keys.Remove(pageno, out var slotindex);

            // do something with slotindex or not
        }

        internal delegate void KeyIterator(KvPagenumber pageno);

        internal void ForEach(KeyIterator action)
        {
            Perf.CallCount();

            for (int i = 0; i < _keys._nextitem; i++)
            {
                ref var item = ref _keys._items[i];

                // TODO check if Pagenumber 0 should be excluded
                if (!item.HasValue)
                {
                    continue;
                }

                action(item.PageNumber);
            }
        }

        internal delegate void KeyValueIterator(KvPagenumber pageno, ref T item);

        internal void ForEach(KeyValueIterator action)
        {
            Perf.CallCount();

            for (int i = 0; i < _keys._nextitem; i++)
            {
                ref var item = ref _keys._items[i];

                if (!item.HasValue)
                {
                    continue;
                }

                action(item.PageNumber, ref _values[i]);
            }
        }

        public bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return _keys.Contains(pageno, out _);
        }

        public ref T TryGetValueRef(KvPagenumber pageno, out bool isvalid)
        {
            Perf.CallCount();

            isvalid = _keys.Contains(pageno, out var slotindex);

            if (isvalid)
            {
                return ref _values[slotindex];
            }

            return ref Sentinel;
        }



        //internal HashKeyValueAllocator2<T> Copy()
        //{
        //    Perf.CallCount();

        //    var buckets = new int[_itemsize];
        //    Array.Copy(_buckets, buckets, _nextitem);

        //    var items = new Slot[_itemsize];
        //    Array.Copy(_items, items, _nextitem);


        //    return new HashKeyValueAllocator2<T>(buckets, items, _itemsize, _nextitem, _freelist, _count);
        //}
    }
}
