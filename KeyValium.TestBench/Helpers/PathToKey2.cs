using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class PathToKey2
    {
        public PathToKey2(long[] path)
        {
            FullPath = path;

            if (path.Length > 1)
            {
                var p = new long[FullPath.Length - 1];
                Array.Copy(FullPath, 0, p, 0, FullPath.Length - 1);
                Parent = new PathToKey2(p);
            }

            Child = FullPath[FullPath.Length - 1];

            FullName = string.Join(',', FullPath);
            ParentName = string.Join(',', Parent);
            ChildName= string.Join(",", Child); 
        }

        /// <summary>
        /// Fullname 
        /// </summary>
        public readonly string FullName;

        /// <summary>
        /// Path without the last element
        /// </summary>
        public readonly string ParentName;

        /// <summary>
        /// the last element of the array
        /// </summary>
        public readonly string ChildName;

        public long[] FullPath
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public PathToKey2 Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public long Child
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            private set;
        }

        //public override string ToString()
        //{
        //    return string.Join(',', Path);
        //}

        //public PathToKey Parent
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        //    get
        //    {
        //        return new PathToKey() { Path = Path.Take(Path.Count - 1).ToList() };
        //    }
        //}
    }
}
