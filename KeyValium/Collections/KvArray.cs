using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Collections
{
    internal class KvArray<T> where T : struct,  IDisposable
    {
        #region Constructors

        public const int DefaultSize = ArrayAllocator<T>.DefaultInitialSize;

        public KvArray()
        {
            Perf.CallCount();

            _allocator = new ArrayAllocator<T>();

            Current = -1;
        }

        #endregion

        #region Variables

        internal ArrayAllocator<T> _allocator;

        internal int Current;

        #endregion

        [SkipLocalsInit]
        internal void DisposeItems()
        {
            Perf.CallCount();

            _allocator.DisposeItems();

            Current = -1;
        }

        public bool HasCurrent
        {
            get
            {
                Perf.CallCount();

                return Current >= 0;
            }
        }

        /// <summary>
        /// current item
        /// </summary>
        public ref T CurrentItem
        {
            get
            {
                Perf.CallCount();

                return ref _allocator.GetRef(Current);
            }
        }

        public bool HasPrevItem
        {
            get
            {
                Perf.CallCount();

                return Current > 0;
            }
        }

        public ref T PrevItem
        {
            get
            {
                Perf.CallCount();

                return ref _allocator.GetRef(Current - 1);
            }
        }

        public bool HasNextItem
        {
            get
            {
                Perf.CallCount();

                return Current >= 0 && Current < _allocator.Last;
            }
        }

        public ref T NextItem
        {
            get
            {
                Perf.CallCount();

                return ref _allocator.GetRef(Current + 1);
            }
        }

        internal void Insert(T item)
        {
            Perf.CallCount();

            if (Current < 0)
            {
                Append(item);
            }
            else
            {
                _allocator.InsertAt(Current, item);
            }
        }

        /// <summary>
        /// Appends a node at the end of the nodelist
        /// CurrentNode points to the appended Item after this call
        /// </summary>
        /// <param name="page"></param>
        /// <param name="keyindex"></param>
        internal void Append(T item)
        {
            Perf.CallCount();

            _allocator.Append(item);

            Current = _allocator.Last;
        }

        /// <summary>
        /// cuts off all nodes following the current node
        /// </summary>
        public void Cutoff()
        {
            Perf.CallCount();

            _allocator.Cutoff(Current);
        }

        /// <summary>
        /// removes the current node from the nodelist 
        /// Current points to the following node or the last node after this call
        /// </summary>
        /// <returns>the pagenumber of the removed node</returns>
        public void Remove()
        {
            Perf.CallCount();

            _allocator.Remove(Current);

            // check empty
            if (Current > _allocator.Last)
            {
                Current = _allocator.Last;
            }
        }

        /// <summary>
        /// moves Current to the next NodePointer.
        /// If the function returns false, Current is not changed
        /// </summary>
        /// <returns>true on success, false if there is no next NodePointer</returns>
        public bool MoveNext()
        {
            Perf.CallCount();

            if (Current >= 0 && Current < _allocator.Last)
            {
                Current++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// moves Current to the previous NodePointer.
        /// If the function returns false, Current is not changed
        /// </summary>
        /// <returns>true on success, false if there is no previous nodepointer</returns>
        public bool MovePrevious()
        {
            Perf.CallCount();

            if (Current > 0 )
            {
                Current--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// moves Current to the first NodePointer.
        /// If the function returns false, Current is not changed
        /// </summary>
        /// <returns>true on success, false if there is no first nodepointer (should not happen)</returns>

        public bool MoveFirst()
        {
            Perf.CallCount();

            if (Current >= 0)
            {
                Current = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// moves Current to the last NodePointer.
        /// If the function returns false, Current is not changed
        /// </summary>
        /// <returns>true on success, false if there is no last nodepointer (should not happen)</returns>
        public bool MoveLast()
        {
            Perf.CallCount();

            if (Current >= 0)
            {
                Current = _allocator.Last;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            Perf.CallCount();

            var sb = new StringBuilder();

            if (!HasCurrent)
            {
                sb.AppendLine("<Empty>");
            }
            else
            {
                for (int i = 0; i <= _allocator.Last; i++)
                {
                    ref var node = ref _allocator.GetRef(i);

                    sb.AppendFormat("[{0:00}] {1}{2}\n", i, node.ToString(), i == Current ? "*" : "");
                }
            }

            return sb.ToString();
        }
    }
}

