using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class ShadowTree
    {
        public ShadowTree()
        {
            _list.Add(new TreeNode(0, null));
        }

        #region Cursors

        private Dictionary<string, CursorEntry> _cursors = new Dictionary<string, CursorEntry>();

        public Dictionary<string, CursorEntry> Cursors
        {
            get
            {
                return _cursors;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void ClearCursors()
        {
            foreach(var pair in Cursors) 
            {
                pair.Value.Cursor.TreeRef?.Dispose();
                pair.Value.Cursor.Dispose();
            }

            _cursors.Clear();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void BeginTransaction()
        {
            _list.Add(Current.Copy(null));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Commit()
        {
            if (_list.Count > 1)
            {
                var data = Current;
                _list.RemoveAt(_list.Count - 1);
                _list[_list.Count - 1] = data;
            }
            else
            {
                throw new Exception("Tx Mismatch");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Rollback()
        {
            if (_list.Count > 1)
            {
                _list.RemoveAt(_list.Count - 1);
            }
            else
            {
                throw new Exception("Tx Mismatch");
            }
        }

        private List<TreeNode> _list = new List<TreeNode>();


        public TreeNode Current
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get
            {
                return _list.Last();
            }
        }

        public int Level
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get
            {
                return _list.Count - 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal void Clear()
        {
            if (_list.Count == 1)
            {
                Current.Clear();
            }
            else
            {
                throw new Exception("Tx Mismatch");
            }
        }

        internal void RemoveCursor(KeyValuePair<string, CursorEntry> entry)
        {
            entry.Value.Cursor.TreeRef?.Dispose();
            entry.Value.Cursor.Dispose();
            Cursors.Remove(entry.Key);
        }
    }
}
