
using KeyValium.Inspector;

namespace KeyValium.Collections
{
    /// <summary>
    /// Manages an array of items, a freelist and an array of buckets
    /// elements can be allocated and released
    /// free items form a single linked list via the Next property
    /// array is resized (grown only) automatically
    /// </summary>
    internal sealed class HashKeyAllocator
    {
        //const int ArrayMaxLength = 2000000000;

        internal struct Slot
        {
            // the key
            internal KvPagenumber PageNumber;
            internal bool HasValue;
            internal int Next;
        }

        public const int DefaultInitialSize = 32;

        public HashKeyAllocator() : this(DefaultInitialSize)
        {
            Perf.CallCount();
        }

        public HashKeyAllocator(int initialsize)
        {
            Perf.CallCount();

            Initialize(initialsize);
        }

        //private HashKeyAllocator(int[] buckets, Slot[] items, int nextitem, int freelist, int count)
        //{
        //    Perf.CallCount();

        //    _buckets = buckets;
        //    _items = items;
        //    _nextitem = nextitem;
        //    _freelist = freelist;
        //    _count = count;
        //}

        #region Variables

        internal int[] _buckets;

        internal int _nextitem;

        // index of first item of freelist
        internal int _freelist;

        internal Slot[] _items;

        internal int _count;

        internal ulong _mod;

        #endregion

        internal void Initialize(int itemcount)
        {
            Perf.CallCount();

            var size = GetPower2(itemcount); // KvUtil.GetPrime(itemcount);

            _buckets = new int[size];
            _items = new Slot[size];

            // first array element will not be used
            // otherwise buckets would need to be initialized with -1
            // and we do not have the time for that ;-)
            _nextitem = 1;
            _freelist = 0;
            _count = 0;
            _mod = (ulong)(size - 1);
        }

        private int GetPower2(int x)
        {
            for (int i = 0; i < 31; i++)
            {
                var power = 1 << i;
                if (power >= x)
                {
                    return power;
                }
            }

            throw new InvalidOperationException("Count is too large!");
        }

        private int NextPower2(int x)
        {
            return x << 1;
        }

        internal SortedDictionary<int, int> GetBucketStats()
        {
            Perf.CallCount();

            var counts = new SortedDictionary<int, int>();

            for (int bucket = 0; bucket < _buckets.Length; bucket++)
            {
                var count = 0;

                for (int index = _buckets[bucket]; index > 0; index = _items[index].Next)
                {
                    count++;
                }

                if (!counts.ContainsKey(count))
                {
                    counts.Add(count, 0);
                }

                counts[count] += 1;
            }

            return counts;
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

                return _count;
            }
        }

        public bool Add(KvPagenumber pageno, out int slotindex)
        {
            Perf.CallCount();

            // TODO do not call twice
            var bucket = (int)(pageno & _mod);

            for (int index = _buckets[bucket]; index > 0; index = _items[index].Next)
            {
                if (_items[index].PageNumber == pageno)
                {
                    slotindex = index;
                    return false;
                }
            }

            slotindex = GetFreeSlot();

            // refresh bucket number because of possible resize
            // TODO do not call twice
            bucket = (int)(pageno & _mod);

            var first = _buckets[bucket];
            _buckets[bucket] = slotindex;

            ref var slot = ref _items[slotindex];

            slot.Next = first;
            slot.PageNumber = pageno;
            slot.HasValue = true;

            _count++;

            return true;
        }

        public bool Remove(KvPagenumber pageno, out int slotindex)
        {
            Perf.CallCount();

            var bucket = (int)(pageno & _mod);

            var prev = _buckets[bucket];

            for (slotindex = _buckets[bucket]; slotindex > 0; slotindex = _items[slotindex].Next)
            {
                ref var item = ref _items[slotindex];

                if (item.PageNumber == pageno)
                {
                    if (slotindex == prev)
                    {
                        // remove first item of list
                        _buckets[bucket] = item.Next;
                    }
                    else
                    {
                        _items[prev].Next = item.Next;
                    }

                    AddFreeSlot(slotindex);

                    _count--;

                    return true;
                }

                prev = slotindex;
            }

            slotindex = 0;
            return false;
        }

        internal delegate void KeyIterator(KvPagenumber pageno);

        internal void ForEach(KeyIterator action)
        {
            Perf.CallCount();

            for (int i = 0; i < _nextitem; i++)
            {
                ref var item = ref _items[i];

                // TODO check if Pagenumber 0 should be excluded
                if (!item.HasValue)
                {
                    continue;
                }

                action(item.PageNumber);
            }
        }

        public bool Contains(KvPagenumber pageno, out int slotindex)
        {
            Perf.CallCount();

            var bucket = (int)(pageno & _mod);

            // TODO try to use ref _items[index]
            for (slotindex = _buckets[bucket]; slotindex > 0; slotindex = _items[slotindex].Next)
            {
                if (_items[slotindex].PageNumber == pageno)
                {
                    return true;
                }
            }

            slotindex = 0;

            return false;
        }

        private int GetFreeSlot()
        {
            Perf.CallCount();

            if (_freelist > 0)
            {
                // return first item
                var ret = _freelist;
                _freelist = _items[_freelist].Next;

                return ret;
            }

            if (_nextitem == _items.Length)
            {
                GrowItems();
            }

            return _nextitem++;
        }

        private void AddFreeSlot(int index)
        {
            Perf.CallCount();

            ref var item = ref _items[index];

            item.PageNumber = 0;
            item.HasValue = false;
            item.Next = _freelist;

            _freelist = index;
        }


        private void GrowItems()
        {
            Perf.CallCount();

            var newsize = NextPower2(_items.Length);
            var newmod = (ulong) (newsize - 1);

            if (newsize <= _items.Length)
            {
                throw new OutOfMemoryException("HashAllocator has reached maximum array size!");
            }

            var newitems = new Slot[newsize];
            Array.Copy(_items, newitems, _nextitem);

            var newbuckets = new int[newsize];

            for (int i = 0; i < _nextitem; i++)
            {
                ref var currentitem = ref newitems[i];
                if (currentitem.HasValue)
                {
                    int bucket = (int)(currentitem.PageNumber & newmod);

                    ref var currentbucket = ref newbuckets[bucket];

                    currentitem.Next = currentbucket;
                    currentbucket = i;
                }
            }

            _items = newitems;
            _buckets = newbuckets;
            _mod = newmod;
        }


        //internal HashKeyAllocator Copy()
        //{
        //    Perf.CallCount();

        //    var buckets = new int[_items.Length];
        //    Array.Copy(_buckets, buckets, _nextitem);

        //    var items = new Slot[_items.Length];
        //    Array.Copy(_items, items, _nextitem);


        //    return new HashKeyAllocator(buckets, items, _nextitem, _freelist, _count);
        //}
    }
}
