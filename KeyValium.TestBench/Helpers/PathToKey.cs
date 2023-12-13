using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class PathToKey
    {
        public PathToKey(List<long> vals)
        {
            Path = vals;
            Last = vals.Last();
        }

        public readonly long Last;

        public List<long> Path
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public override string ToString()
        {
            return string.Join(',', Path);
        }

        private PathToKey _parent = null;

        public PathToKey Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (_parent == null)
                {
                    _parent = new PathToKey(Path.Take(Path.Count - 1).ToList());
                }

                return _parent;
            }
        }
    }
}
