using System;

namespace KeyValium.TestBench
{
    internal class FastRandom
    {
        public FastRandom() : this(DateTime.Now.Ticks)
        {

        }

        public FastRandom(long seed)
        {
            _seed = seed;
        }

        private long _seed;

        internal void NextBytes(byte[] ret)
        {
            var val = _seed;

            for (int i = 0; i < ret.Length; i++)
            {
                val = (val << 2) + 65537;
                ret[i] = (byte)val;
            }
        }
    }
}
