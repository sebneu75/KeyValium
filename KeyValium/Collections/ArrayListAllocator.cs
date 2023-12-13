using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Collections
{
    /// <summary>
    /// Manages an array and a freelist
    /// elements can be allocated and released
    /// released items form a single linked list via the NextFree property
    /// array is resized (grown only) automatically
    /// </summary>
    internal sealed class ArrayListAllocator<T> where T : struct
    {
        //const int ArrayMaxLength = 2000000000;

        internal struct Slot
        {
            /// <summary>
            /// next free slot
            /// </summary>
            internal int NextFree;
            public T Item;
        }

        public const int DefaultInitialSize = 32;

        public ArrayListAllocator() : this(DefaultInitialSize)
        {
            Perf.CallCount();
        }

        public ArrayListAllocator(int initialsize)
        {
            Perf.CallCount();

            Initialize(initialsize);
        }

        private ArrayListAllocator(Slot[] items, int nextitem, int freelist)
        {
            Perf.CallCount();

            _items = items;
            _nextitem = nextitem;
            _freelist = freelist;
        }

        internal int _nextitem;

        // index of first item of freelist
        internal int _freelist;

        internal Slot[] _items;

        private void Initialize(int itemcount)
        {
            Perf.CallCount();

            _items = new Slot[itemcount];
            _nextitem = 0;
            _freelist = -1;
        }

        public void Clear()
        {
            Perf.CallCount();

            _nextitem = 0;
            _freelist = -1;
        }

        internal ref T GetRef(int index)
        {
            Perf.CallCount();

            //if (index < 0 || index >= _nextitem)
            //{
            //    throw new IndexOutOfRangeException("Index out of range!");
            //}

            return ref _items[index].Item;
        }

        internal ref T Allocate(out int index)
        {
            Perf.CallCount();

            if (_freelist >= 0)
            {
                // return first item
                index = _freelist;
                _freelist = _items[_freelist].NextFree;

                return ref _items[index].Item;
            }

            if (_nextitem == _items.Length)
            {
                Grow();
            }

            index = _nextitem;
            return ref _items[_nextitem++].Item;
        }

        internal void Release(int index)
        {
            Perf.CallCount();

            ref var item = ref _items[index];
            item = default;
            //item.Value = default;
            //item.HasValue = false;
            item.NextFree = _freelist;
            // _items[index].Prev = -1; // not used for freelist

            _freelist = index;
        }

        private void Grow()
        {
            Perf.CallCount();

            var newsize = _items.Length * 2;

            if (newsize < 0 || newsize > Array.MaxLength) // ArrayMaxLength)
            {
                newsize = Array.MaxLength; // ArrayMaxLength;
            }

            if (newsize <= _items.Length)
            {
                throw new OutOfMemoryException("ArrayAllocator has reached maximum array size!");
            }

            var newitems = new Slot[newsize];
            Array.Copy(_items, newitems, _nextitem);

            _items = newitems;
        }

        internal ArrayListAllocator<T> Copy()
        {
            Perf.CallCount();

            var copy = new Slot[_items.Length];
            Array.Copy(_items, copy, _nextitem);

            return new ArrayListAllocator<T>(copy, _nextitem, _freelist);
        }
    }
}
