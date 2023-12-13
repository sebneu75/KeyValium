using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench.Helpers
{
    internal class KVEntry
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public KVEntry()
        {
            //Children = new Dictionary<long, KVEntry>();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        internal void ClearValue()
        {
            Value = null;
            ValueLength = 0;
            ValueSeed = 0;
        }

        public int ValueLength
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            set;
        }

        public int ValueSeed
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            set;
        }

        public int KeyLength
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            set;
        }

        public byte[] Key
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            set;
        }

        public byte[] Value
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            set;
        }

        //public Dictionary<long, KVEntry> Children
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveOptimization|MethodImplOptions.AggressiveInlining)]
        //    get;
        //    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        //    private set;
        //}

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public KVEntry Copy()
        {
            var ret = new KVEntry();

            ret.KeyLength = KeyLength;
            ret.ValueLength = ValueLength;
            ret.ValueSeed = ValueSeed;
            ret.Key = Copy(Key);
            ret.Value = Copy(Value);

            //foreach (var entry in Children)
            //{
            //    ret.Children.Add(entry.Key, entry.Value.Copy());
            //}

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private byte[] Copy(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            var ret = new byte[bytes.Length];
            bytes.CopyTo(ret, 0);

            return ret;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", ValueLength, ValueSeed);
        }
    }
}
