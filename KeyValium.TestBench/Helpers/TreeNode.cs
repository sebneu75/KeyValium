using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class TreeNode
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public TreeNode(long number, TreeNode parent)
        {
            Number = number;
            Parent = parent;
            _children = new Dictionary<long, TreeNode>();
        }

        private Dictionary<long, TreeNode> _children;

        public KVEntry Entry;

        public long Number;

        public TreeNode Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public IReadOnlyCollection<TreeNode> Children
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get
            {
                return _children.Values;
            }
        }

        public int TotalCount
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                var ret = Children.Count;
                foreach (var child in Children)
                {
                    ret += child.TotalCount;
                }

                return ret;
            }
        }

        public PathToKey Path
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                var path = new List<long>();
                var node = this;

                do
                {
                    path.Insert(0, node.Number);
                    node = node.Parent;
                }
                while (node != null && node.Parent != null);

                return new PathToKey(path);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _children.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void Add(long number, TreeNode item)
        {
            item.Parent = this;
            _children.Add(number, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public TreeNode Copy(TreeNode parent)
        {
            var ret = new TreeNode(Number, parent);
            ret.Entry = Entry?.Copy();

            foreach (var item in _children)
            {
                ret.Add(item.Key, item.Value.Copy(ret));
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal KVEntry GetEntry(PathToKey key)
        {
            var node = GetNode(key);
            return node?.Entry;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public TreeNode GetNode(PathToKey key)
        {
            var parent = GetParentNode(key);

            if (parent != null)
            {
                if (parent._children.TryGetValue(key.Path[key.Path.Count - 1], out var item))
                {
                    return item;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal byte[][] GetParentKeys(PathToKey key)
        {
            var node = GetParentNode(key);

            var ret = new List<byte[]>();

            while (node != null && node.Parent != null)
            {
                ret.Insert(0, node.Entry.Key);
                node = node.Parent;
            }

            return ret.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal TreeNode GetParentNode(PathToKey key)
        {
            var current = this;

            for (int i = 0; i < key.Path.Count - 1; i++)
            {
                if (current._children.TryGetValue(key.Path[i], out var item))
                {
                    current = item;
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]

        public void InsertEntry(PathToKey key, KVEntry entry)
        {
            var parent = GetParentNode(key);
            if (parent != null)
            {
                if (parent._children.ContainsKey(key.Last))
                {
                    throw new Exception("Key already exists!");
                }
                else
                {
                    parent._children.Add(key.Last, new TreeNode(key.Last, parent) { Entry = entry });
                }
            }
            else
            {
                throw new Exception("KeyPath not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UpsertEntry(PathToKey key, KVEntry entry)
        {
            var parent = GetParentNode(key);
            if (parent != null)
            {
                if (parent._children.ContainsKey(key.Last))
                {
                    parent._children[key.Last].Entry = entry;
                }
                else
                {
                    parent._children.Add(key.Last, new TreeNode(key.Last, parent) { Entry = entry });
                }
            }
            else
            {
                throw new Exception("KeyPath not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UpdateEntry(PathToKey key, KVEntry entry)
        {
            var parent = GetParentNode(key);
            if (parent != null)
            {
                if (parent._children.ContainsKey(key.Last))
                {
                    parent._children[key.Last].Entry = entry;
                }
                else
                {
                    throw new Exception("Key not found!");
                }
            }
            else
            {
                throw new Exception("KeyPath not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void Remove(PathToKey key)
        {
            var parent = GetParentNode(key);

            if (parent != null)
            {
                if (parent._children.ContainsKey(key.Last))
                {
                    var child = parent._children[key.Last];
                    if (child._children.Count == 0)
                    {
                        parent._children.Remove(key.Last);
                    }
                    else
                    {
                        child.Entry.ClearValue();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal bool ExistsParent(PathToKey key)
        {
            if (key.Path.Count <= 1)
            {
                return true;
            }

            return GetParentNode(key) != null;
        }
    }
}
