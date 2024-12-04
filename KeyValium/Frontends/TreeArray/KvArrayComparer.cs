using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    internal class KvArrayComparer : IEqualityComparer<KvArrayKey[]>
    {
        public bool Equals(KvArrayKey[] keys1, KvArrayKey[] keys2)
        {
            if (keys1 == keys2)
            {
                return true;
            }

            if (keys1 == null || keys2 == null)
            {
                return false;
            }

            if (keys1.Length != keys2.Length)
            {
                return false;
            }

            for (int i = 0; i < keys1.Length; i++)
            {
                if (!keys1[i].Equals(keys2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] KvArrayKey[] keys)
        {
            var ret = keys[0].GetHashCode();

            for (int i = 1; i < keys.Length; i++)
            {
                Rol(ref ret);
                ret ^= keys[i].GetHashCode();
            }

            return ret;
        }
        /// <summary>
        /// rotates i left 1 bit 
        /// </summary>
        /// <param name="i"></param>
        private void Rol(ref int i)
        {
            i = (int)(((uint)i << 1) | ((uint)i >> 31));
        }
    }
}
