using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KeyValium.TestBench
{
    public class KeyComparer : IComparer<byte[]>, IEqualityComparer<byte[]>
    {
        public int Compare(byte[] key1, byte[] key2)
        {
            var l1 = key1 == null ? 0 : key1.Length;
            var l2 = key2 == null ? 0 : key2.Length;

            var len = Math.Min(l1, l2);

            for (int i = 0; i < len; i++)
            {
                if (key1[i] == key2[i])
                    continue;

                return key1[i] < key2[i] ? -1 : +1;
            }

            if (l1 == l2)
                return 0;

            return l1 < l2 ? -1 : +1;
        }

        public bool Equals(byte[] key1, byte[] key2)
        {
            if (key1.Length == key2.Length)
            {
                var i = 0;
                for (i = 0; i < key2.Length && key1[i] == key2[i]; i++) ;

                return i == key1.Length;
            }

            return false;
        }

        public int GetHashCode([DisallowNull] byte[] bytes)
        {
            if (bytes == null)
            {
                return 0;
            }

            int hash = 17;

            for (int i = 0; i < bytes.Length; i++)
            {
                var h = bytes[i].GetHashCode();
                hash = hash * 31 + h;
            }

            return hash;
        }
    }
}
