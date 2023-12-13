using KeyValium.Inspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class ShadowDict
    {
        public ShadowDict()
        {

        }

        private Dictionary<string, Dictionary<string, KVEntry>> _db = new();

        public int TotalCount
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                var ret = 0;
                foreach (var dict in _db)
                {
                    ret += dict.Value.Count;
                }

                return ret;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            foreach (var dict in _db)
            {
                dict.Value.Clear();
            }
            _db.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Add(PathToKey2 path, KVEntry entry)
        {
            var dict = EnsureNode(path.ParentName);
            dict.Add(path.ChildName, entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Dictionary<string, KVEntry> EnsureNode(string name)
        {
            if (_db.TryGetValue(name, out var dict))
            {
                return dict;
            }

            dict = new Dictionary<string, KVEntry>();
            _db.Add(name, dict);

            return dict;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public ShadowDict Copy()
        {
            var copy = new ShadowDict();

            foreach (var item in _db)
            {
                var newdict = new Dictionary<string, KVEntry>();

                foreach (var entry in item.Value)
                {
                    newdict.Add(entry.Key, entry.Value.Copy());
                }

                copy._db.Add(item.Key, newdict);
            }

            return copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal KVEntry GetEntry(PathToKey2 path)
        {
            if (_db.TryGetValue(path.ParentName, out var dict))
            {
                if (dict.TryGetValue(path.ChildName, out var entry))
                {
                    return entry;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void InsertEntry(PathToKey2 path, KVEntry entry)
        {
            if (_db.TryGetValue(path.ParentName, out var dict))
            {
                if (!dict.TryGetValue(path.ChildName, out var exentry))
                {
                    dict.Add(path.ChildName, entry);
                }
                else
                {
                    throw new Exception("ChildName already exists!");
                }
            }
            else
            {
                throw new Exception("ParentName not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UpsertEntry(PathToKey2 path, KVEntry entry)
        {
            if (_db.TryGetValue(path.ParentName, out var dict))
            {
                if (dict.TryGetValue(path.ChildName, out var exentry))
                {
                    dict[path.ChildName] = entry;
                }
                else
                {
                    dict.Add(path.ChildName, entry);
                }
            }
            else
            {
                throw new Exception("ParentName not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void UpdateEntry(PathToKey2 path, KVEntry entry)
        {
            if (_db.TryGetValue(path.ParentName, out var dict))
            {
                if (dict.TryGetValue(path.ChildName, out var exentry))
                {
                    dict[path.ChildName] = entry;
                }
                else
                {
                    throw new Exception("ChildName already exists!");
                }
            }
            else
            {
                throw new Exception("ParentName not found!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal void Remove(PathToKey2 path)
        {
            if (_db.TryGetValue(path.ParentName, out var dict))
            {
                dict.Remove(path.ChildName);
            }
        }
    }
}
