using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KeyValium.TestBench
{
    public class KeyValueGenerator
    {
        public KeyValueGenerator()
        {

        }

        static Random _rnd = new Random();

        public static List<KeyValuePair<byte[], byte[]>> Generate(uint pagesize, long position, long count, KeyGenStrategy st, int minkeysize, int maxkeysize, int minvalsize, int maxvalsize)
        {
            var ret = new List<KeyValuePair<byte[], byte[]>>((int)count);
            var hash = new HashSet<byte[]>((int)count, new KeyComparer());

            var pos = position;
            while (hash.Count < count)
            {
                var key = GetBytes(st, pos,
                    minkeysize < 0 ? Limits.GetMaxKeyLength(pagesize) : minkeysize,
                    maxkeysize < 0 ? Limits.GetMaxKeyLength(pagesize) : maxkeysize);
                hash.Add(key);
                pos++;
            }

            //var minkeysize = this.MinKeySize < 0 ? Limits.GetMaxKeyLength(this.DbPageSize) : this.MinKeySize;
            //var maxkeysize = this.MaxKeySize < 0 ? Limits.GetMaxKeyLength(this.DbPageSize) : this.MaxKeySize;
            //var minvalsize = this.MinValueSize < 0 ? Limits.GetMaxInlineValueSize(this.)
            //var maxvalsize = this.MaxValueSize;

            var keys = hash.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var val = GetBytes(minvalsize < 0 ? Limits.GetMaxInlineValueSize(pagesize, (ushort)key.Length) : minvalsize,
                                   maxvalsize < 0 ? Limits.GetMaxInlineValueSize(pagesize, (ushort)key.Length) : maxvalsize);

                ret.Add(new KeyValuePair<byte[], byte[]>(key, val));
            }

            Debug.Assert(ret.Count == count, "FAIL!");

            return ret;
        }

        public static byte[] GetBytes(KeyGenStrategy st, long pos, int min, int max)
        {
            var counter = BitConverter.GetBytes(pos).Reverse().ToArray();

            var len = min + _rnd.Next(max - min + 1);

            if (len < counter.Length)
            {
                var ret2 = new byte[len];
                Array.Copy(counter, counter.Length - len, ret2, 0, len);
                return ret2;
            }

            var ret = new byte[len];

            switch (st)
            {
                case KeyGenStrategy.Random:
                    _rnd.NextBytes(ret);
                    break;
                case KeyGenStrategy.Sequential:
                    Array.Fill<byte>(ret, 0);
                    break;
            }

            counter.CopyTo(ret, ret.Length - counter.Length);

            Debug.Assert(ret.Length <= max && ret.Length >= min, "FAIL!");

            return ret;
        }

        public static byte[] GetBytes(int min, int max)
        {
            var len = min + _rnd.Next(max - min + 1);

            var ret = new byte[len];

            _rnd.NextBytes(ret);

            Debug.Assert(ret.Length <= max && ret.Length >= min, "FAIL!");

            return ret;
        }

        public unsafe static byte[] GetSeededBytes(int seed, int length)
        {
            var ret = new byte[length];

            fixed (byte* ptr = ret)
            {
                var intptr = (int*)ptr;
                var seedptr = (byte*) &seed;

                for (int i = 0; i < length / 4; i++)
                {
                    *intptr = seed;
                    intptr++;
                    seed += 0x01010101;
                }

                var remainder = length & 3;

                var byteptr = (byte*)intptr;

                while (remainder > 0) 
                {
                    *byteptr = *seedptr;
                    byteptr++;
                    seedptr++;
                    remainder--;
                }
            }

            //var rnd = new FastRandom(seed);
            //rnd.NextBytes(ret);

            return ret;
        }

        public static int GetRandomLength(int min, int max)
        {
            var ret = min + _rnd.Next(max - min + 1);

            Debug.Assert(ret <= max && ret >= min, "FAIL!");

            return ret;
        }

        public static int GetRandomSeed()
        {
            return _rnd.Next();
        }

        public static List<KeyValuePair<byte[], byte[]>> Order(List<KeyValuePair<byte[], byte[]>> list, KeyOrder order)
        {
            var comp = new KeyComparer();

            List<KeyValuePair<byte[], byte[]>> ret;

            switch (order)
            {
                case KeyOrder.Ascending:
                    ret = list.OrderBy(x => x.Key, comp).ToList();
                    VaildateSortOrder(ret, -1);
                    return ret;
                case KeyOrder.Descending:
                    ret = list.OrderByDescending(x => x.Key, comp).ToList();
                    VaildateSortOrder(ret, +1);
                    return ret;
                case KeyOrder.Random:
                    return Shuffle(_rnd, list);
            }

            return list.ToList();
        }

        private static void VaildateSortOrder(List<KeyValuePair<byte[], byte[]>> list, int expected)
        {
            var comp = new KeyComparer();

            for (int i = 1; i < list.Count; i++)
            {
                var result = comp.Compare(list[i - 1].Key, list[i].Key);
                if (result != expected)
                {
                    Console.WriteLine("Fail!");
                }
            }
        }

        public static List<T> Shuffle<T>(Random rnd, List<T> list)
        {
            var temp = list.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = temp[k];
                temp[k] = temp[n];
                temp[n] = value;
            }

            return temp;
        }
    }
}
