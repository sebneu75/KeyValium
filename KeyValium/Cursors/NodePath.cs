using KeyValium.Collections;
using System.Text;

namespace KeyValium.Cursors
{
    internal sealed class NodePath : KvArray<Node>
    {
        public NodePath()
        {
            Perf.CallCount();

            Initialize(true);
        }

        internal bool IsValid;

        public bool HasCurrentOfType(ushort pagetype)
        {
            Perf.CallCount();

            if (Current >= 0)
            {
                ref var current = ref CurrentItem;
                return current.Page.PageType == pagetype;
            }

            return false;
        }

        internal int First
        {
            get
            {
                return 0;
            }
        }

        internal int Last
        {
            get
            {
                return _allocator.Last;
            }
        }

        public bool HasPrevOfType(ushort pagetype)
        {
            Perf.CallCount();

            if (HasPrevItem)
            {
                ref var prev = ref PrevItem;
                return prev.Page.PageType == pagetype;
            }

            return false;
        }

        public bool HasNextOfType(ushort pagetype)
        {
            Perf.CallCount();

            if (HasNextItem)
            {
                ref var next = ref NextItem;
                return next.Page.PageType == pagetype;
            }

            return false;
        }

        public ref Node GetNode(int index)
        {
            Perf.CallCount();

            return ref _allocator.GetRef(index);
        }

        /// <summary>
        /// creates a placeholder node if the nodelist is empty 
        /// </summary>
        [SkipLocalsInit]
        public void Initialize(bool isvalid)
        {
            Perf.CallCount();

            DisposeItems();

            IsValid = isvalid;
        }

        internal void Invalidate()
        {
            Perf.CallCount();

            Initialize(false);
        }

        internal void Append(AnyPage page, int keyindex)
        {
            Perf.CallCount();

            Append(new Node(page, keyindex));
        }

        /// <summary>
        /// inserts a new node at current the current  position
        /// CurrentNode points to the inserted item after this call
        /// if CurrentNode is a placeholder it is overwritten otherwise inserted
        /// </summary>
        /// <param name="page"></param>
        /// <param name="keyindex"></param>
        internal void Insert(AnyPage page, ushort keyindex)
        {
            Perf.CallCount();

            Insert(new Node(page, keyindex));
        }

        internal ulong Remove2()
        {
            Perf.CallCount();

            var pageno = CurrentItem.Page.PageNumber;
            Remove();

            return pageno;
        }

        internal NodePath Copy()
        {
            Perf.CallCount();

            var ret = new NodePath();

            for (var i = 0; i <= _allocator.Last; i++)
            {
                ref var node = ref _allocator.GetRef(i);
                ret.Append(new Node(node.Page, node.KeyIndex));
            }

            ret.IsValid = IsValid;

            return ret;
        }

        internal void Validate()
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating KeyPath.");

            if (!IsValid)
            {
                throw new KeyValiumException(ErrorCodes.InvalidCursor, "The KeyPath is invalid.");
            }
        }
    }
}
