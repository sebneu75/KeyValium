using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Helpers
{
    internal class Shadow
    {
        public Shadow()
        {
            _list.Add(new ShadowDict());
            Current = _list[0];
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
            foreach (var pair in Cursors)
            {
                pair.Value.Cursor.TreeRef?.Dispose();
                pair.Value.Cursor.Dispose();
            }

            _cursors.Clear();
        }

        internal void RemoveCursor(KeyValuePair<string, CursorEntry> entry)
        {
            entry.Value.Cursor.TreeRef?.Dispose();
            entry.Value.Cursor.Dispose();
            Cursors.Remove(entry.Key);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void BeginTransaction()
        {
            _list.Add(Current.Copy());
            Current=_list.Last();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Commit()
        {
            if (_list.Count > 1)
            {
                var data = Current;
                _list.RemoveAt(_list.Count - 1);
                _list[_list.Count - 1] = data;
                Current = _list.Last();
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
                Current = _list.Last();
            }
            else
            {
                throw new Exception("Tx Mismatch");
            }
        }

        private List<ShadowDict> _list = new ();

        public ShadowDict Current
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            private set;
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
    }
}
