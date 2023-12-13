using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Memory
{
    internal struct KeyFromPool
    {
        internal KeyFromPool(byte[] bytes, int keylength)
        {
            Bytes = bytes;
            Length = keylength;
        }

        internal readonly byte[] Bytes;

        internal readonly int Length;

        internal ReadOnlySpan<byte> Span
        {
            get
            {
                return new ReadOnlySpan<byte>(Bytes, 0, Length);
            }
        }
    }
}
