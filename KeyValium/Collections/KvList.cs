
using System.Runtime.InteropServices;

namespace KeyValium.Collections
{
    internal sealed class KvList<T> where T : struct
    {
        #region Constructors

        public const int DefaultSize = ArrayListAllocator<T>.DefaultInitialSize;

        public KvList() : this(DefaultSize)
        {
            Perf.CallCount();
        }

        public KvList(int itemcount)
        {
            Perf.CallCount();

            Initialize(itemcount);
        }

        #endregion

        #region Variables

        //private int _itemcount;

        internal ArrayListAllocator<KvListSlot> _allocator;

        internal int _first;

        internal int _last;

        #endregion

        private void Initialize(int itemcount)
        {
            Perf.CallCount();

            _allocator = new ArrayListAllocator<KvListSlot>(itemcount);

            _first = -1;
            _last = -1;
        }

        #region public API

        public void Clear()
        {
            Perf.CallCount();

            _allocator.Clear();

            _first = -1;
            _last = -1;
        }

        /// <summary>
        /// Inserts an item at the start of the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int InsertFirst(T item)
        {
            Perf.CallCount();

            ref var slot = ref _allocator.Allocate(out var index);

            slot.Item = item;
            slot.Next = _first;
            slot.Prev = -1;

            if (_first >= 0)
            {
                ref var firstslot = ref _allocator.GetRef(_first);
                firstslot.Prev = index;
            }
            else
            {
                _last = index;
            }

            _first = index;

            return index;
        }

        internal void Remove(int pos)
        {
            Perf.CallCount();

            if (pos >= 0)
            {
                ref var slot = ref _allocator.GetRef(pos);

                var prev = slot.Prev;
                var next = slot.Next;

                if (prev >= 0)
                {
                    ref var previtem = ref _allocator.GetRef(prev);
                    previtem.Next = next;
                }
                else
                {
                    _first = next;
                }

                if (next >= 0)
                {
                    ref var nextitem = ref _allocator.GetRef(next);
                    nextitem.Prev = prev;
                }
                else
                {
                    _last = prev;
                }

                _allocator.Release(pos);
            }
        }

        public bool RemoveLast(out T item)
        {
            Perf.CallCount();

            if (_last >= 0)
            {
                ref var lastitem = ref _allocator.GetRef(_last);

                item = lastitem.Item;

                var prev = lastitem.Prev;
                if (prev >= 0)
                {
                    ref var previtem = ref _allocator.GetRef(prev);
                    previtem.Next = -1;
                    lastitem.Prev = -1;

                    _allocator.Release(_last);
                    _last = prev;
                }
                else
                {
                    _allocator.Release(_last);

                    _last = -1;
                    _first = -1;
                }

                return true;
            }

            item = default;
            return false;
        }

        internal void MoveToFirst(int pos)
        {
            Perf.CallCount();

            if (pos < 0 || pos == _first)
            {
                // item is already the first
                return;
            }

            ref var slot = ref _allocator.GetRef(pos);

            //
            // close the hole
            //
            var prev = slot.Prev;
            var next = slot.Next;

            if (prev >= 0)
            {
                ref var previtem = ref _allocator.GetRef(prev);
                previtem.Next = next;
            }
            else
            {
                // should not happen it would mean pos is already the first
                //_first = next;
                throw new InvalidOperationException("MoveToFirst failed (prev).");
            }

            if (next >= 0)
            {
                ref var nextitem = ref _allocator.GetRef(next);
                nextitem.Prev = prev;
            }
            else
            {
                _last = prev;
            }

            //
            // Insert at beginning of list
            //
            slot.Next = _first;
            slot.Prev = -1;

            if (_first >= 0)
            {
                ref var firstslot = ref _allocator.GetRef(_first);
                firstslot.Prev = pos;
            }
            else
            {
                // should not happen
                //_last = index;
                throw new InvalidOperationException("MoveToFirst failed. (first)");
            }

            _first = pos;
        }

        internal void Validate(int expectedcount, string where = "")
        {
            var vforward = new HashSet<int>();
            var vbackward = new HashSet<int>();

            var countforward = 0;
            var countbackward = 0;

            //
            // scan forward
            //
            var current = _first;
            while (current >= 0)
            {
                if (!vforward.Add(current))
                {
                    throw new InvalidOperationException("Fail: Node visited twice forward! " + where);
                }

                countforward++;
                ref var slot = ref _allocator.GetRef(current);
                if (countforward > 1 && slot.Prev < 0)
                {
                    throw new InvalidOperationException("Fail: Prev is < 0! " + where);
                }

                if (slot.Next == current)
                {
                    throw new InvalidOperationException("Fail: Next == Current! " + where);
                }

                if (slot.Prev == current)
                {
                    throw new InvalidOperationException("Fail: Prev == Current! " + where);
                }

                if (slot.Next >= 0)
                {
                    ref var next = ref _allocator.GetRef(slot.Next);
                    if (next.Prev != current)
                    {
                        throw new InvalidOperationException("Fail: Next.Prev != Current! " + where);
                    }
                }

                if (slot.Prev >= 0)
                {
                    ref var prev = ref _allocator.GetRef(slot.Prev);
                    if (prev.Next != current)
                    {
                        throw new InvalidOperationException("Fail: Prev.Next != Current! " + where);
                    }
                }

                if (slot.Prev >= 0 && slot.Prev == slot.Next)
                {
                    throw new InvalidOperationException("Fail: Node has same Prev and Next! " + where);
                }

                current = slot.Next;
            }

            //
            // scan backward
            //
            current = _last;
            while (current >= 0)
            {
                if (!vbackward.Add(current))
                {
                    throw new InvalidOperationException("Fail: Node visited twice backward! " + where);
                }

                countbackward++;
                ref var slot = ref _allocator.GetRef(current);
                if (countbackward > 1 && slot.Next < 0)
                {
                    throw new InvalidOperationException("Fail: Next is < 0! " + where);
                }
                current = slot.Prev;
            }

            if (countforward != countbackward)
            {
                throw new InvalidOperationException("Fail: Count Forward-Backward mismatch " + where);
            }

            if (!vforward.SetEquals(vbackward))
            {
                throw new InvalidOperationException("Fail: Sets not equal! " + where);
            }

            if (expectedcount > 0 && countforward != expectedcount)
            {
                throw new InvalidOperationException("Fail: Count mismatch " + where);
            }
        }

        #endregion

        [StructLayout(LayoutKind.Auto)]
        internal struct KvListSlot
        {
            internal int Next;
            internal int Prev;
            internal T Item;
        }
    }
}

