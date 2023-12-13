using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Collections
{
    internal sealed class ArrayAllocator<T> where T : struct,  IDisposable
    {
        internal const int DefaultInitialSize = 32;

        internal T[] _items = new T[DefaultInitialSize];

        public ArrayAllocator()
        {
            Perf.CallCount();
        }

        internal int Last = -1;

        [SkipLocalsInit]
        public void DisposeItems()
        {
            // clear cutoff items because of reference counting
            for (int i = 0; i <= Last; i++)
            {
                _items[i].Dispose();
            }

            Last = -1;
        }

        public ref T GetRef(int index)
        {
            Perf.CallCount();

            return ref _items[index];
        }

        /// <summary>
        /// inserts a new node at current the current  position
        /// CurrentNode points to the inserted item after this call
        /// if CurrentNode is a placeholder it is overwritten otherwise inserted
        /// </summary>
        internal void InsertAt(int index, T item)
        {
            Perf.CallCount();

            if (index < 0 || index > Last)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            // check reallocate
            if (Last == _items.Length - 1)
            {
                ReAllocate();
            }

            // move contents to the right
            MoveValues(index, +1);

            // update last
            Last++;

            // set new values
            ref var slot = ref GetRef(index);
            slot = item;
        }

        /// <summary>
        /// Appends an item at the end of the array
        /// CurrentNode points to the appended Item after this call
        /// </summary>
        /// <param name="item">the Item to append</param>
        internal void Append(T item)
        {
            Perf.CallCount();

            // check reallocate
            if (Last == _items.Length - 1)
            {
                ReAllocate();
            }

            // update last
            Last++;

            // set new values
            ref var slot = ref GetRef(Last);
            slot = item;
        }

        /// <summary>
        /// cuts off all nodes following the current node
        /// </summary>
        public void Cutoff(int index)
        {
            Perf.CallCount();

            for (int i = index + 1; i <= Last; i++)
            {
                // clear cutoff items because of reference counting
                _items[i].Dispose();
            }

            Last = index;
        }

        /// <summary>
        /// removes the current node from the nodelist 
        /// Current points to the following node or the last node after this call
        /// </summary>
        /// <returns>the pagenumber of the removed node</returns>
        public void Remove(int index)
        {
            Perf.CallCount();

            // TODO check index

            // move contents to the left
            MoveValues(index, -1);

            // update last
            Last--;

            // TODO check if needed empty
            //if (Last < 0)
            //{
            //    Initialize();
            //}
        }

        private void MoveValues(int index, int direction)
        {
            Perf.CallCount();

            if (direction > 0)
            {
                // move array contents to the right starting with current
                Array.Copy(_items, index, _items, index + 1, Last - index + 1);

                // set KeyPointer to default to avoid calling ReleaseRef on the moved element
                _items[index] = default;
            }
            else if (direction < 0)
            {
                // clear current item because it is reference counted
                _items[index].Dispose();

                // move array contents to the left starting with current +1
                Array.Copy(_items, index + 1, _items, index, Last - index);

                // clear former last avoiding release ref
                _items[Last] = default;
            }
        }

        private void ReAllocate()
        {
            Perf.CallCount();

            var newsize = _items.Length * 2;

            var newarray = new T[newsize];

            Array.Copy(_items, newarray, _items.Length);

            _items = newarray;
        }
    }
}