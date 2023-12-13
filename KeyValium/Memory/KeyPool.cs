using KeyValium.Collections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Memory
{
    internal sealed class KeyPool
    {
        const int MaxItems = 32;

        internal KeyPool(int size)
        {
            Size = size;

            Pool = new KvList<KeyPoolSlot>(MaxItems);
        }

        internal KvList<KeyPoolSlot> Pool;

        internal int Size;

        internal int Count;

        internal byte[] Rent()
        {
            if (Pool.RemoveLast(out var slot))
            {
                return slot.Bytes;
            }
            else
            {
                return new byte[Size];
            }
        }

        internal KeyFromPool CopyKey(ByteSpan key)
        {
            var bytes = Rent();
            key.ReadOnlySpan.CopyTo(bytes);

            return new KeyFromPool(bytes, key.Length);
        }
        internal void Return(byte[] bytes)
        {
            if (bytes.Length != Size)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Returned KeyFromPool has wrong size!");
            }

            if (Count < MaxItems)
            {
                Pool.InsertFirst(new KeyPoolSlot(bytes));
                Count++;
            }

            // otherwise do nothing
        }

        internal void Return(KeyFromPool? kp)
        {
            if (kp.HasValue)
            {
                Return(kp.Value.Bytes);
            }
        }

        internal struct KeyPoolSlot
        {
            internal KeyPoolSlot(byte[] bytes)
            {
                Bytes = bytes;
            }

            internal byte[] Bytes;
        }
    }
}
