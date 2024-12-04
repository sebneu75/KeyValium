/*
 
    This RedBlackTree implementation is based on System.Collections.Generic.SortedSet

    https://github.com/dotnet/runtime/blob/main/src/libraries/System.Collections/src/System/Collections/Generic/SortedSet.cs
        Licensed to the .NET Foundation under one or more agreements.
        The .NET Foundation licenses this file to you under the MIT license.
    
    Copyright (c) .NET Foundation and Contributors
*/

using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyValium.Collections
{
    internal sealed class RedBlackTree<T>
    {
        internal enum TreeRotation
        {
            Left,
            Right,
            RightLeft,
            LeftRight,
        }

        #region Constructors

        public RedBlackTree() : this(null)
        {
            Perf.CallCount();
        }

        public RedBlackTree(IComparer<T> comparer)
        {
            Perf.CallCount();

            _allocator = new ArrayListAllocator<TreeNode>();
            _comparer = comparer ?? Comparer<T>.Default;
        }

        private RedBlackTree(IComparer<T> comparer, ArrayListAllocator<TreeNode> allocator, int rootindex, int count)
        {
            Perf.CallCount();

            _allocator = allocator;
            _comparer = comparer;
            _rootindex = rootindex;
            _count = count;
        }

        #endregion

        #region Variables

        int _rootindex = -1;

        int _count = 0;

        readonly ArrayListAllocator<TreeNode> _allocator;

        readonly IComparer<T> _comparer;

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                Perf.CallCount();

                return _count;
            }
        }

        internal IComparer<T> Comparer
        {
            get
            {
                Perf.CallCount();

                return _comparer;
            }
        }

        #endregion

        #region Node functions

        int nodeAllocate(ref T item, bool isred)
        {
            Perf.CallCount();

            ref var node = ref _allocator.Allocate(out var index);
            node = new TreeNode(item, isred);

            return index;
        }

        void nodeRelease(int index)
        {
            Perf.CallCount();

            _allocator.Release(index);
        }

        private ref TreeNode nodeGet(int index)
        {
            Perf.CallCount();

            Debug.Assert(index >= 0, "FAIL");

            // TODO change to GetRef?
            return ref _allocator.GetRef(index);
        }

        private T nodeGetItem(int index)
        {
            Perf.CallCount();

            //if (index < 0)
            //{
            //    return -1;
            //}

            ref var node = ref nodeGet(index);
            return node.Item;
        }

        private void nodeColorBlack(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            node.IsRed = false;
        }

        private void nodeColorRed(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            node.IsRed = true;
        }

        private void nodeSameColor(int source, int target)
        {
            Perf.CallCount();

            Debug.Assert(source != target, "FAIL");

            ref var snode = ref _allocator.GetRef(source);
            ref var tnode = ref _allocator.GetRef(target);

            tnode.IsRed = snode.IsRed;
        }

        private bool nodeIs4Node(int index)
        {
            Perf.CallCount();
            
            if (nodeIsNonNullRed(nodeGetLeft(index)))
            {
                return nodeIsNonNullRed(nodeGetRight(index));
            }

            return false;
        }

        private bool nodeIsNonNullRed(int index)
        {
            Perf.CallCount();

            if (index < 0)
            {
                return false;
            }

            ref var node = ref nodeGet(index);
            return node.IsRed;
        }

        private int nodeGetLeft(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            return node.Left;
        }

        private void nodeSetLeft(int index, int left)
        {
            Perf.CallCount();
            Debug.Assert(index != left, "FAIL");

            ref var node = ref _allocator.GetRef(index);
            node.Left = left;

            if (left >= 0)
            {
                ref var nodeleft = ref _allocator.GetRef(left);
                nodeleft.Parent = index;
            }
        }

        private int nodeGetRight(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            return node.Right;
        }

        private void nodeSetRight(int index, int right)
        {
            Perf.CallCount();

            Debug.Assert(index != right, "FAIL");

            ref var node = ref _allocator.GetRef(index);
            node.Right = right;

            if (right >= 0)
            {
                ref var noderight = ref _allocator.GetRef(right);
                noderight.Parent = index;
            }
        }

        private int nodeGetParent(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            return node.Parent;
        }

        private int nodeGetParentAndSide(int index, out bool isleftchild)
        {
            Perf.CallCount();

            isleftchild = false;
            ref var node = ref _allocator.GetRef(index);

            if (node.Parent >= 0)
            {
                ref var parent = ref _allocator.GetRef(node.Parent);
                isleftchild = parent.Left == index;
            }

            return node.Parent;
        }

        private bool nodeIsBlack(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            return !node.IsRed;
        }

        private bool nodeIsRed(int index)
        {
            Perf.CallCount();

            ref var node = ref _allocator.GetRef(index);
            return node.IsRed;
        }

        private void nodeSplit4Node(int index)
        {
            Perf.CallCount();

            nodeColorRed(index);
            nodeColorBlack(nodeGetLeft(index));
            nodeColorBlack(nodeGetRight(index));
        }

        private int nodeRotateLeft(int index)
        {
            Perf.CallCount();

            var right = nodeGetRight(index);
            nodeSetRight(index, nodeGetLeft(right));
            nodeSetLeft(right, index);

            return right;
        }

        private int nodeRotateLeftRight(int index)
        {
            Perf.CallCount();

            var left = nodeGetLeft(index);
            var right = nodeGetRight(left);
            nodeSetLeft(index, nodeGetRight(right));
            nodeSetRight(right, index);
            nodeSetRight(left, nodeGetLeft(right));
            nodeSetLeft(right, left);

            return right;
        }

        private int nodeRotateRight(int index)
        {
            Perf.CallCount();

            var left = nodeGetLeft(index);
            nodeSetLeft(index, nodeGetRight(left));
            nodeSetRight(left, index);

            return left;
        }

        private int nodeRotateRightLeft(int index)
        {
            Perf.CallCount();

            var right = nodeGetRight(index);
            var left = nodeGetLeft(right);
            nodeSetRight(index, nodeGetLeft(left));
            nodeSetLeft(left, index);
            nodeSetLeft(right, nodeGetRight(left));
            nodeSetRight(left, right);

            return left;
        }

        private void nodeReplaceChild(int index, int child, int newChild)
        {
            Perf.CallCount();

            if (nodeGetLeft(index) == child)
            {
                nodeSetLeft(index, newChild);
            }
            else
            {
                nodeSetRight(index, newChild);
            }
        }

        private bool nodeIs2Node(int index)
        {
            Perf.CallCount();

            if (nodeIsBlack(index) && nodeIsNullOrBlack(nodeGetLeft(index)))
            {
                return nodeIsNullOrBlack(nodeGetRight(index));
            }
            return false;
        }

        private bool nodeIsNullOrBlack(int index)
        {
            Perf.CallCount();

            if (index < 0)
            {
                return true;
            }

            ref var node = ref nodeGet(index);
            return !node.IsRed;
        }

        private int nodeGetSibling(int index, int child)
        {
            Perf.CallCount();

            if (child != nodeGetLeft(index))
            {
                return nodeGetLeft(index);
            }
            return nodeGetRight(index);
        }

        private void nodeMerge2Nodes(int index)
        {
            Perf.CallCount();

            nodeColorBlack(index);
            nodeColorRed(nodeGetLeft(index));
            nodeColorRed(nodeGetRight(index));
        }

        private TreeRotation nodeGetRotation(int index, int current, int sibling)
        {
            Perf.CallCount();

            bool flag = nodeGetLeft(index) == current;

            if (!nodeIsNonNullRed(nodeGetLeft(sibling)))
            {
                if (!flag)
                {
                    return TreeRotation.LeftRight;
                }
                return TreeRotation.Left;
            }

            if (!flag)
            {
                return TreeRotation.Right;
            }

            return TreeRotation.RightLeft;
        }

        private int nodeRotate(int index, TreeRotation rotation)
        {
            Perf.CallCount();

            switch (rotation)
            {
                case TreeRotation.Right:
                    var left = nodeGetLeft(nodeGetLeft(index));
                    nodeColorBlack(left);
                    return nodeRotateRight(index);

                case TreeRotation.Left:
                    var right = nodeGetRight(nodeGetRight(index));
                    nodeColorBlack(right);
                    return nodeRotateLeft(index);

                case TreeRotation.RightLeft:
                    return nodeRotateRightLeft(index);

                case TreeRotation.LeftRight:
                    return nodeRotateLeftRight(index);

                default:
                    return -1;
            }
        }

        #endregion

        #region Public API

        public RedBlackTree<T> Copy()
        {
            Perf.CallCount();

            return new RedBlackTree<T>(_comparer, _allocator.Copy(), _rootindex, _count);
        }

        public void Clear()
        {
            Perf.CallCount();

            _allocator.Clear();
            _rootindex = -1;
            _count = 0;
        }

        //public string AsString()
        //{
        //    var sb = new StringBuilder();

        //    AppendString(sb, _rootindex, 0);

        //    return sb.ToString();
        //}

        //private void AppendString(StringBuilder sb, int index, int level)
        //{
        //    if (index >= 0)
        //    {
        //        ref var node = ref nodeGet(index);

        //        sb.AppendFormat("{3}{0:00} Item:{1:0000} C:{2}\n", level, node.Item, node.IsRed ? "R" : "B", new string(' ', 4 * level));

        //        AppendString(sb, nodeGetLeft(index), level + 1);
        //        AppendString(sb, nodeGetRight(index), level + 1);
        //    }
        //}

        public bool Insert(T item)
        {
            Perf.CallCount();

            if (_rootindex < 0)
            {
                _rootindex = nodeAllocate(ref item, false);
                _count = 1;
                return true;
            }

            var cindex = _rootindex;
            var pindex = -1;
            var gpindex = -1;
            var ggpindex = -1;
            var order = 0;
            while (cindex >= 0)
            {
                order = _comparer.Compare(item, nodeGetItem(cindex));
                if (order == 0)
                {
                    nodeColorBlack(_rootindex);
                    return false;
                }
                if (nodeIs4Node(cindex))
                {
                    nodeSplit4Node(cindex);
                    if (nodeIsNonNullRed(pindex))
                    {
                        InsertionBalance(cindex, ref pindex, gpindex, ggpindex);
                    }
                }
                ggpindex = gpindex;
                gpindex = pindex;
                pindex = cindex;
                cindex = ((order < 0) ? nodeGetLeft(cindex) : nodeGetRight(cindex));
            }

            var newchild = nodeAllocate(ref item, true);
            if (order > 0)
            {
                nodeSetRight(pindex, newchild);
            }
            else
            {
                nodeSetLeft(pindex, newchild);
            }

            if (nodeIsRed(pindex))
            {
                InsertionBalance(newchild, ref pindex, gpindex, ggpindex);
            }

            nodeColorBlack(_rootindex);
            _count++;

            return true;
        }

        public bool Remove(T item)
        {
            Perf.CallCount();

            if (_rootindex < 0)
            {
                return false;
            }

            int cindex = _rootindex;
            int pindex = -1;
            int gpindex = -1;
            int match = -1;
            int pmatch = -1;
            bool matchfound = false;

            while (cindex >= 0)
            {
                if (nodeIs2Node(cindex))
                {
                    if (pindex < 0)
                    {
                        nodeColorRed(cindex);
                    }
                    else
                    {
                        var sibling = nodeGetSibling(pindex, cindex);
                        if (nodeIsRed(sibling))
                        {
                            if (nodeGetRight(pindex) == sibling)
                            {
                                nodeRotateLeft(pindex);
                            }
                            else
                            {
                                nodeRotateRight(pindex);
                            }
                            nodeColorRed(pindex);
                            nodeColorBlack(sibling);
                            ReplaceChildOrRoot(gpindex, pindex, sibling);
                            gpindex = sibling;
                            
                            if (pindex == match)
                            {
                                pmatch = sibling;
                            }

                            sibling = nodeGetSibling(pindex, cindex);
                        }
                        if (nodeIs2Node(sibling))
                        {
                            nodeMerge2Nodes(pindex);
                        }
                        else
                        {
                            var newgpindex = nodeRotate(pindex, nodeGetRotation(pindex, cindex, sibling));

                            nodeSameColor(pindex, newgpindex);
                            nodeColorBlack(pindex);
                            nodeColorRed(cindex);
                            ReplaceChildOrRoot(gpindex, pindex, newgpindex);

                            if (pindex == match)
                            {
                                pmatch = newgpindex;
                            }
                        }
                    }
                }

                var order = matchfound ? (-1) : _comparer.Compare(item, nodeGetItem(cindex));
                if (order == 0)
                {
                    matchfound = true;
                    match = cindex;
                    pmatch = pindex;
                }

                gpindex = pindex;
                pindex = cindex;
                cindex = (order < 0) ? nodeGetLeft(cindex) : nodeGetRight(cindex);
            }
            if (match >= 0)
            {
                ReplaceNode(match, pmatch, pindex, gpindex);
                _count--;
                nodeRelease(match);
            }

            if (_rootindex >= 0)
            {
                nodeColorBlack(_rootindex);
            }

            return matchfound;
        }

        public bool TryGetMin(out int min)
        {
            Perf.CallCount();

            return TryGetMin(_rootindex, out min);
        }

        private bool TryGetMin(int index, out int min)
        {
            Perf.CallCount();

            min = -1;

            if (index < 0)
            {
                return false;
            }

            var left = index;
            var leftleft = nodeGetLeft(left);

            while (leftleft >= 0)
            {
                left = leftleft;
                leftleft = nodeGetLeft(left);
            }

            min = left;
            return true;
        }

        public bool TryGetMax(out int max)
        {
            Perf.CallCount();

            return TryGetMax(_rootindex, out max);
        }

        private bool TryGetMax(int index, out int max)
        {
            Perf.CallCount();

            max = -1;

            if (index < 0)
            {
                return false;
            }

            var right = index;
            var rightright = nodeGetRight(right);
            while (rightright >= 0)
            {
                right = rightright;
                rightright = nodeGetRight(right);
            }

            max = right;
            return true;
        }

        public bool TryGetPrev(int index, out int prev)
        {
            Perf.CallCount();

            prev = -1;

            if (index < 0)
            {
                return false;
            }

            {
                var left = nodeGetLeft(index);
                if (left >= 0)
                {
                    // return max from left subtree
                    return TryGetMax(left, out prev);
                }
            }

            var parent = index;
            while (parent >= 0)
            {
                parent = nodeGetParentAndSide(parent, out var isleft);
                if (parent >= 0 && !isleft)
                {
                    prev = parent;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetNext(int index, out int next)
        {
            Perf.CallCount();

            next = -1;

            if (index < 0)
            {
                return false;
            }

            {
                var right = nodeGetRight(index);
                if (right >= 0)
                {
                    // return min from right subtree
                    return TryGetMin(right, out next);
                }
            }

            var parent = index;
            while (parent >= 0)
            {
                parent = nodeGetParentAndSide(parent, out var isleft);
                if (parent >= 0 && isleft)
                {
                    next = parent;
                    return true;
                }
            }

            return false;
        }

        public void InsertAll(IEnumerable<T> collection)
        {
            Perf.CallCount();

            foreach (T item in collection)
            {
                Insert(item);
            }
        }

        private void RemoveAll(IEnumerable<T> collection)
        {
            Perf.CallCount();

            foreach (T item in collection)
            {
                Remove(item);
            }
        }

        public bool Contains(T item)
        {
            Perf.CallCount();

            return FindNode(item) >= 0;
        }

        public ref T GetItem(int index)
        {
            Perf.CallCount();

            ref var node = ref nodeGet(index);
            return ref node.Item;
        }

        internal TreeNode GetNode(int index)
        {
            Perf.CallCount();

            ref var node = ref nodeGet(index);
            return node;
        }

        /// <summary>
        /// returns the maximum item that is less than or equal to item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal int FindMaxLeq(T item)
        {
            Perf.CallCount();

            var index = _rootindex;
            var lastright = -1;

            while (index >= 0)
            {
                var result = _comparer.Compare(GetItem(index), item);
                if (result == 0)
                {
                    return index;
                }
                else if (result > 0)
                {
                    index = nodeGetLeft(index);
                }
                else
                {
                    lastright = index;
                    index = nodeGetRight(index);
                }
            }

            return lastright;
        }

        /// <summary>
        /// returns the maximum item that is less than or equal to item or the minimum
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal int FindMaxLeqOrMin(T item)
        {
            Perf.CallCount();

            var index = _rootindex;
            var lastright = -1;
            var lastleft = -1;

            while (index >= 0)
            {
                var result = _comparer.Compare(GetItem(index), item);
                if (result == 0)
                {
                    return index;
                }
                else if (result > 0)
                {
                    lastleft = index;
                    index = nodeGetLeft(index);
                }
                else
                {
                    lastright = index;
                    index = nodeGetRight(index);
                }
            }

            return lastright < 0 ? lastleft : lastright;
        }


        /// <summary>
        /// returns the minimum item that is greater than or equal to item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal int FindMinGeq(T item)
        {
            Perf.CallCount();

            var index = _rootindex;
            var lastleft = -1;

            while (index >= 0)
            {
                var result = _comparer.Compare(GetItem(index), item);
                if (result == 0)
                {
                    return index;
                }
                else if (result > 0)
                {
                    lastleft = index;
                    index = nodeGetLeft(index);
                }
                else
                {
                    index = nodeGetRight(index);
                }
            }

            return lastleft;
        }

        internal int FindNode(T item)
        {
            Perf.CallCount();

            var node = _rootindex;
            while (node >= 0)
            {
                int num = _comparer.Compare(item, nodeGetItem(node));
                if (num == 0)
                {
                    return node;
                }
                node = ((num < 0) ? nodeGetLeft(node) : nodeGetRight(node));
            }

            return -1;
        }

        internal int InternalIndexOf(T item)
        {
            Perf.CallCount();

            var node = _rootindex;
            int num = 0;
            while (node >= 0)
            {
                int num2 = _comparer.Compare(item, nodeGetItem(node));
                if (num2 == 0)
                {
                    return num;
                }
                node = ((num2 < 0) ? nodeGetLeft(node) : nodeGetRight(node));
                num = ((num2 < 0) ? (2 * num + 1) : (2 * num + 2));
            }

            return -1;
        }

        public void CheckParents()
        {
            Perf.CallCount();

            InOrderTreeWalk((index) =>
            {
                ref var node = ref nodeGet(index);

                if (node.Left >= 0)
                {
                    ref var left = ref nodeGet(node.Left);
                    if (left.Parent != index)
                    {
                        throw new Exception("Parent mismatch!");
                    }
                }

                if (node.Right >= 0)
                {
                    ref var right = ref nodeGet(node.Right);

                    if (right.Parent != index)
                    {
                        throw new Exception("Parent mismatch!");
                    }
                }

                return true;
            });
        }

        internal delegate bool TreeWalkPredicate(int index);

        internal bool InOrderTreeWalk(TreeWalkPredicate action)
        {
            Perf.CallCount();

            if (_rootindex < 0)
            {
                return true;
            }

            var stack = new Stack<int>(2 * BitOperations.Log2((uint)Count + 1));

            for (int left = _rootindex; left >= 0; left = nodeGetLeft(left))
            {
                stack.Push(left);
            }

            while (stack.Count != 0)
            {
                var item = stack.Pop();
                if (!action(item))
                {
                    return false;
                }
                for (int node = nodeGetRight(item); node >= 0; node = nodeGetLeft(node))
                {
                    stack.Push(node);
                }
            }

            return true;
        }

        internal bool BreadthFirstTreeWalk(TreeWalkPredicate action)
        {
            Perf.CallCount();

            if (_rootindex < 0)
            {
                return true;
            }

            var queue = new Queue<int>();
            queue.Enqueue((_rootindex));
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                if (!action(item))
                {
                    return false;
                }

                var left = nodeGetLeft(item);
                if (left >= 0)
                {
                    queue.Enqueue(left);
                }

                var right = nodeGetRight(item);
                if (right >= 0)
                {
                    queue.Enqueue(right);
                }
            }

            return true;
        }


        #endregion

        #region Helpers

        private void InsertionBalance(int current, ref int parent, int grandParent, int greatGrandParent)
        {
            Perf.CallCount();

            bool flag = nodeGetRight(grandParent) == parent;
            bool flag2 = nodeGetRight(parent) == current;

            int node;

            if (flag == flag2)
            {
                node = flag2 ? nodeRotateLeft(grandParent) : nodeRotateRight(grandParent);
            }
            else
            {
                node = flag2 ? nodeRotateLeftRight(grandParent) : nodeRotateRightLeft(grandParent);
                parent = greatGrandParent;
            }
            nodeColorRed(grandParent);
            nodeColorBlack(node);

            ReplaceChildOrRoot(greatGrandParent, grandParent, node);
        }

        private void ReplaceChildOrRoot(int parent, int child, int newChild)
        {
            Perf.CallCount();

            if (parent >= 0)
            {
                nodeReplaceChild(parent, child, newChild);
            }
            else
            {
                _rootindex = newChild;
                if (_rootindex >= 0)
                {
                    ref var node = ref nodeGet(_rootindex);
                    node.Parent = -1;
                }
            }
        }

        private void ReplaceNode(int match, int parentOfMatch, int successor, int parentOfSuccessor)
        {
            Perf.CallCount();

            if (successor == match)
            {
                successor = nodeGetLeft(match);
            }
            else
            {
                var tempright = nodeGetRight(successor);
                if (tempright >= 0)
                {
                    nodeColorBlack(tempright);
                }

                if (parentOfSuccessor != match)
                {
                    nodeSetLeft(parentOfSuccessor, tempright);
                    nodeSetRight(successor, nodeGetRight(match));
                }
                nodeSetLeft(successor, nodeGetLeft(match));
            }

            if (successor >= 0)
            {
                nodeSameColor(match, successor);
            }

            ReplaceChildOrRoot(parentOfMatch, match, successor);
        }

        #endregion

        #region TreeNode

        [StructLayout(LayoutKind.Auto)]
        internal struct TreeNode
        {
            public T Item;

            public int Left;

            public int Right;

            public int Parent;

            public bool IsRed;

            public TreeNode(T item, bool isred)
            {
                Perf.CallCount();

                Item = item;
                IsRed = isred;
                Left = -1;
                Right = -1;
                Parent = -1;
            }

            //public Node DeepClone(int count)
            //{
            //    Node node = ShallowClone();
            //    Stack<(Node, Node)> stack = new Stack<(Node, Node)>(2 * SortedSet<T>.Log2(count) + 2);
            //    stack.Push((this, node));
            //    (Node, Node) result;
            //    while (stack.TryPop(out result))
            //    {
            //        Node left = result.Item1.Left;
            //        if (left != null)
            //        {
            //            Node node2 = left.ShallowClone();
            //            result.Item2.Left = node2;
            //            stack.Push((left, node2));
            //        }
            //        Node right = result.Item1.Right;
            //        if (right != null)
            //        {
            //            Node node2 = right.ShallowClone();
            //            result.Item2.Right = node2;
            //            stack.Push((right, node2));
            //        }
            //    }
            //    return node;
            //}

            //public Node ShallowClone()
            //{
            //    return new Node(Item, Color);
            //}
        }

        #endregion
    }
}

/*
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable, ISerializable, IDeserializationCallback
        {
            private readonly SortedSet<T> _tree;

            private readonly int _version;

            private readonly Stack<Node> _stack;

            private Node _current;

            private readonly bool _reverse;

            public T Current
            {
                get
                {
                    if (_current != null)
                    {
                        return _current.Item;
                    }
                    return default(T);
                }
            }

            object? IEnumerator.Current
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }
                    return _current.Item;
                }
            }

            internal bool NotStartedOrEnded => _current == null;

            internal Enumerator(SortedSet<T> set)
                : this(set, reverse: false)
            {
            }

            internal Enumerator(SortedSet<T> set, bool reverse)
            {
                _tree = set;
                set.VersionCheck();
                _version = set.version;
                _stack = new Stack<Node>(2 * SortedSet<T>.Log2(set.TotalCount() + 1));
                _current = null;
                _reverse = reverse;
                Initialize();
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new PlatformNotSupportedException();
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                throw new PlatformNotSupportedException();
            }

            private void Initialize()
            {
                _current = null;
                Node node = _tree.root;
                while (node != null)
                {
                    Node node2 = (_reverse ? node.Right : node.Left);
                    Node node3 = (_reverse ? node.Left : node.Right);
                    if (_tree.IsWithinRange(node.Item))
                    {
                        _stack.Push(node);
                        node = node2;
                    }
                    else
                    {
                        node = ((node2 != null && _tree.IsWithinRange(node2.Item)) ? node2 : node3);
                    }
                }
            }

            public bool MoveNext()
            {
                _tree.VersionCheck();
                if (_version != _tree.version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }
                if (_stack.Count == 0)
                {
                    _current = null;
                    return false;
                }
                _current = _stack.Pop();
                Node node = (_reverse ? _current.Left : _current.Right);
                while (node != null)
                {
                    Node node2 = (_reverse ? node.Right : node.Left);
                    Node node3 = (_reverse ? node.Left : node.Right);
                    if (_tree.IsWithinRange(node.Item))
                    {
                        _stack.Push(node);
                        node = node2;
                    }
                    else
                    {
                        node = ((node3 != null && _tree.IsWithinRange(node3.Item)) ? node3 : node2);
                    }
                }
                return true;
            }

            public void Dispose()
            {
            }

            internal void Reset()
            {
                if (_version != _tree.version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }
                _stack.Clear();
                Initialize();
            }

            void IEnumerator.Reset()
            {
                Reset();
            }
        }

        internal struct ElementCount
        {
            internal int UniqueCount;

            internal int UnfoundCount;
        }

        internal sealed class TreeSubSet : SortedSet<T>, ISerializable, IDeserializationCallback
        {
            private readonly SortedSet<T> _underlying;

            private readonly T _min;

            private readonly T _max;

            private int _countVersion;

            private readonly bool _lBoundActive;

            private readonly bool _uBoundActive;

            internal override T MinInternal
            {
                get
                {
                    VersionCheck();
                    Node node = root;
                    T result = default(T);
                    while (node != null)
                    {
                        int num = (_lBoundActive ? base.Comparer.Compare(_min, node.Item) : (-1));
                        if (num > 0)
                        {
                            node = node.Right;
                            continue;
                        }
                        result = node.Item;
                        if (num == 0)
                        {
                            break;
                        }
                        node = node.Left;
                    }
                    return result;
                }
            }

            internal override T MaxInternal
            {
                get
                {
                    VersionCheck();
                    Node node = root;
                    T result = default(T);
                    while (node != null)
                    {
                        int num = ((!_uBoundActive) ? 1 : base.Comparer.Compare(_max, node.Item));
                        if (num < 0)
                        {
                            node = node.Left;
                            continue;
                        }
                        result = node.Item;
                        if (num == 0)
                        {
                            break;
                        }
                        node = node.Right;
                    }
                    return result;
                }
            }

            public TreeSubSet(SortedSet<T> Underlying, T Min, T Max, bool lowerBoundActive, bool upperBoundActive)
                : base(Underlying.Comparer)
            {
                _underlying = Underlying;
                _min = Min;
                _max = Max;
                _lBoundActive = lowerBoundActive;
                _uBoundActive = upperBoundActive;
                root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive);
                count = 0;
                version = -1;
                _countVersion = -1;
            }

            internal override bool AddIfNotPresent(T item)
            {
                if (!IsWithinRange(item))
                {
                    throw new ArgumentOutOfRangeException("item");
                }
                bool result = _underlying.AddIfNotPresent(item);
                VersionCheck();
                return result;
            }

            public override bool Contains(T item)
            {
                VersionCheck();
                return base.Contains(item);
            }

            internal override bool DoRemove(T item)
            {
                if (!IsWithinRange(item))
                {
                    return false;
                }
                bool result = _underlying.Remove(item);
                VersionCheck();
                return result;
            }

            public override void Clear()
            {
                if (base.Count != 0)
                {
                    List<T> toRemove = new List<T>();
                    BreadthFirstTreeWalk(delegate (Node n)
                    {
                        toRemove.Add(n.Item);
                        return true;
                    });
                    while (toRemove.Count != 0)
                    {
                        SortedSet<T> underlying = _underlying;
                        List<T> list = toRemove;
                        underlying.Remove(list[list.Count - 1]);
                        toRemove.RemoveAt(toRemove.Count - 1);
                    }
                    root = null;
                    count = 0;
                    version = _underlying.version;
                }
            }

            internal override bool IsWithinRange(T item)
            {
                int num = (_lBoundActive ? base.Comparer.Compare(_min, item) : (-1));
                if (num > 0)
                {
                    return false;
                }
                num = ((!_uBoundActive) ? 1 : base.Comparer.Compare(_max, item));
                return num >= 0;
            }

            internal override bool InOrderTreeWalk(TreeWalkPredicate<T> action)
            {
                VersionCheck();
                if (root == null)
                {
                    return true;
                }
                Stack<Node> stack = new Stack<Node>(2 * Log2(count + 1));
                Node node = root;
                while (node != null)
                {
                    if (IsWithinRange(node.Item))
                    {
                        stack.Push(node);
                        node = node.Left;
                    }
                    else
                    {
                        node = ((!_lBoundActive || base.Comparer.Compare(_min, node.Item) <= 0) ? node.Left : node.Right);
                    }
                }
                while (stack.Count != 0)
                {
                    node = stack.Pop();
                    if (!action(node))
                    {
                        return false;
                    }
                    Node node2 = node.Right;
                    while (node2 != null)
                    {
                        if (IsWithinRange(node2.Item))
                        {
                            stack.Push(node2);
                            node2 = node2.Left;
                        }
                        else
                        {
                            node2 = ((!_lBoundActive || base.Comparer.Compare(_min, node2.Item) <= 0) ? node2.Left : node2.Right);
                        }
                    }
                }
                return true;
            }

            internal override bool BreadthFirstTreeWalk(TreeWalkPredicate<T> action)
            {
                VersionCheck();
                if (root == null)
                {
                    return true;
                }
                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(root);
                while (queue.Count != 0)
                {
                    Node node = queue.Dequeue();
                    if (IsWithinRange(node.Item) && !action(node))
                    {
                        return false;
                    }
                    if (node.Left != null && (!_lBoundActive || base.Comparer.Compare(_min, node.Item) < 0))
                    {
                        queue.Enqueue(node.Left);
                    }
                    if (node.Right != null && (!_uBoundActive || base.Comparer.Compare(_max, node.Item) > 0))
                    {
                        queue.Enqueue(node.Right);
                    }
                }
                return true;
            }

            internal override Node FindNode(T item)
            {
                if (!IsWithinRange(item))
                {
                    return null;
                }
                VersionCheck();
                return base.FindNode(item);
            }

            internal override int InternalIndexOf(T item)
            {
                int num = -1;
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        num++;
                        if (base.Comparer.Compare(item, current) == 0)
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }

            internal override void VersionCheck(bool updateCount = false)
            {
                VersionCheckImpl(updateCount);
            }

            private void VersionCheckImpl(bool updateCount)
            {
                if (version != _underlying.version)
                {
                    root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive);
                    version = _underlying.version;
                }
                if (updateCount && _countVersion != _underlying.version)
                {
                    count = 0;
                    InOrderTreeWalk(delegate
                    {
                        count++;
                        return true;
                    });
                    _countVersion = _underlying.version;
                }
            }

            internal override int TotalCount()
            {
                return _underlying.Count;
            }

            public override SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
            {
                if (_lBoundActive && base.Comparer.Compare(_min, lowerValue) > 0)
                {
                    throw new ArgumentOutOfRangeException("lowerValue");
                }
                if (_uBoundActive && base.Comparer.Compare(_max, upperValue) < 0)
                {
                    throw new ArgumentOutOfRangeException("upperValue");
                }
                return (TreeSubSet)_underlying.GetViewBetween(lowerValue, upperValue);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                GetObjectData(info, context);
            }

            protected override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new PlatformNotSupportedException();
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                throw new PlatformNotSupportedException();
            }

            protected override void OnDeserialization(object sender)
            {
                throw new PlatformNotSupportedException();
            }
        }


        private bool ContainsAllElements(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }
            return true;
        }


        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, Count);
        }

        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, Count);
        }

        public void CopyTo(T[] array, int index, int count)
        {
            T[] array2 = array;
            ArgumentNullException.ThrowIfNull(array2, "array");
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count > array2.Length - index)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }
            count += index;
            InOrderTreeWalk(delegate (Node node)
            {
                if (index >= count)
                {
                    return false;
                }
                array2[index++] = node.Item;
                return true;
            });
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array, "array");
            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.Arg_RankMultiDimNotSupported, "array");
            }
            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(SR.Arg_NonZeroLowerBound, "array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (array.Length - index < Count)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }
            T[] array2 = array as T[];
            if (array2 != null)
            {
                CopyTo(array2, index);
                return;
            }
            object[] objects = array as object[];
            if (objects == null)
            {
                throw new ArgumentException(SR.Argument_InvalidArrayType, "array");
            }
            try
            {
                InOrderTreeWalk(delegate (Node node)
                {
                    objects[index++] = node.Item;
                    return true;
                });
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(SR.Argument_InvalidArrayType, "array");
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }





        internal Node FindRange(T from, T to, bool lowerBoundActive, bool upperBoundActive)
        {
            Node node = root;
            while (node != null)
            {
                if (lowerBoundActive && comparer.Compare(from, node.Item) > 0)
                {
                    node = node.Right;
                    continue;
                }
                if (upperBoundActive && comparer.Compare(to, node.Item) < 0)
                {
                    node = node.Left;
                    continue;
                }
                return node;
            }
            return null;
        }






        public int RemoveWhere(Predicate<T> match)
        {
            Predicate<T> match2 = match;
            ArgumentNullException.ThrowIfNull(match2, "match");
            List<T> matches = new List<T>(Count);
            BreadthFirstTreeWalk(delegate (Node n)
            {
                if (match2(n.Item))
                {
                    matches.Add(n.Item);
                }
                return true;
            });
            int num = 0;
            for (int num2 = matches.Count - 1; num2 >= 0; num2--)
            {
                if (Remove(matches[num2]))
                {
                    num++;
                }
            }
            return num;
        }

        public IEnumerable<T> Reverse()
        {
            Enumerator e = new Enumerator(this, reverse: true);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }


        public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
        {
            Node node = FindNode(equalValue);
            if (node != null)
            {
                actualValue = node.Item;
                return true;
            }
            actualValue = default(T);
            return false;
        }

    }
}

*/