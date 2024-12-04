using KeyValium.Memory;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium
{
    [StructLayout(LayoutKind.Auto)]
    internal unsafe struct ByteSpan
    {
        internal ByteSpan(byte* pointer, int length)
        {
            Perf.CallCount();

            Pointer = pointer;
            Length = length;
        }

        internal readonly byte* Pointer;

        internal readonly int Length;

        internal Span<byte> Span
        {
            get
            {
                Perf.CallCount();

                return new Span<byte>(Pointer, Length);
            }
        }

        internal ReadOnlySpan<byte> ReadOnlySpan
        {
            get
            {
                Perf.CallCount();

                return new ReadOnlySpan<byte>(Pointer, Length);
            }
        }

        internal ByteSpan Slice(int offset, int length)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, length);

            return new ByteSpan(Pointer + offset, length);
        }

        internal ByteSpan Slice(int offset)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, Length - offset);

            return new ByteSpan(Pointer + offset, Length - offset);
        }

        internal ByteSpan Slice(byte* pointer, int length)
        {
            Perf.CallCount();

            RangeCheck(pointer, length);

            return new ByteSpan(pointer, length);
        }

        #region Moving Data

        internal void MoveBytes(byte* ktarget, byte* ksource, int size)
        {
            Perf.CallCount();

            // only check target because source may be in another ByteSpan
            RangeCheck(ktarget, size);

            MemUtils.MemoryMove(ktarget, ksource, size);
        }

        #endregion

        public byte[] ToArray()
        {
            Perf.CallCount();

            return Span.ToArray();
        }

        #region Reading and Writing

        public ushort ReadUShort(byte* p)
        {
            Perf.CallCount();

            RangeCheck(p, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                return *(ushort*)p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(ushort*)p);
            }
        }

        public ushort ReadUShort(int offset)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                return *(ushort*)(Pointer + offset);
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(ushort*)(Pointer + offset));
            }
        }

        public ushort ReadUShort(ushort* p)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                return *p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*p);
            }
        }

        public void AddUShort(ushort* p, ushort delta)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                *p += delta;
            }
            else
            {
                var val = BinaryPrimitives.ReverseEndianness(*p);
                val += delta;
                *p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public void WriteUShort(byte* p, ushort val)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                *(ushort*)p = val;
            }
            else
            {
                *(ushort*)p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public void WriteUShort(int offset, ushort val)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                *(ushort*)(Pointer + offset) = val;
            }
            else
            {
                *(ushort*)(Pointer + offset) = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public void WriteUShort(ushort* p, ushort val)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                *p = val;
            }
            else
            {
                *p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public ulong ReadULong(byte* p)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                return *(ulong*)p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(ulong*)p);
            }
        }

        public ulong ReadULong(int offset)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                return *(ulong*)(Pointer + offset);
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(ulong*)(Pointer + offset));
            }
        }

        public void WriteULong(byte* p, ulong val)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                *(ulong*)p = val;
            }
            else
            {
                *(ulong*)p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public void WriteULong(int offset, ulong val)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                *(ulong*)(Pointer + offset) = val;
            }
            else
            {
                *(ulong*)(Pointer + offset) = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public long ReadLong(byte* p)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(long));

            if (BitConverter.IsLittleEndian)
            {
                return *(long*)p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(long*)p);
            }
        }

        public void WriteLong(byte* p, long val)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(long));

            if (BitConverter.IsLittleEndian)
            {
                *(long*)p = val;
            }
            else
            {
                *(long*)p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public uint ReadUInt(byte* p)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(uint));

            if (BitConverter.IsLittleEndian)
            {
                return *(uint*)p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(uint*)p);
            }
        }

        public uint ReadUInt(int offset)
        {
            Perf.CallCount();

            RangeCheck((Pointer + offset), sizeof(uint));

            if (BitConverter.IsLittleEndian)
            {
                return *(uint*)(Pointer + offset);
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(uint*)(Pointer + offset));
            }
        }

        public void WriteUInt(byte* p, uint val)
        {
            Perf.CallCount();

            RangeCheck((byte*)p, sizeof(uint));

            if (BitConverter.IsLittleEndian)
            {
                *(uint*)p = val;
            }
            else
            {
                *(uint*)p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        public void WriteUInt(int offset, uint val)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(uint));

            if (BitConverter.IsLittleEndian)
            {
                *(uint*)(Pointer + offset) = val;
            }
            else
            {
                *(uint*)(Pointer + offset) = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        //public int ReadInt(byte* p)
        //{
        //    Perf.CallCount();

        //    RangeCheck((byte*)p, sizeof(int));

        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *(int*)p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(int*)p);
        //    }
        //}

        //public void WriteInt(byte* p, int val)
        //{
        //    Perf.CallCount();

        //    RangeCheck((byte*)p, sizeof(int));

        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *(int*)p = val;
        //    }
        //    else
        //    {
        //        *(int*)p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        //public ushort ReadUShortBE(byte* p)
        //{
        //    Perf.CallCount();

        //    RangeCheck((byte*)p, sizeof(ushort));

        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(ushort*)p);
        //    }
        //    else
        //    {
        //        return *(ushort*)p;
        //    }
        //}

        //public uint ReadUIntBE(byte* p)
        //{
        //    Perf.CallCount();

        //    RangeCheck((byte*)p, sizeof(uint));

        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(uint*)p);
        //    }
        //    else
        //    {
        //        return *(uint*)p;
        //    }
        //}

        public ulong ReadULongBE(int offset)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                return BinaryPrimitives.ReverseEndianness(*(ulong*)(Pointer + offset));
            }
            else
            {
                return *(ulong*)(Pointer + offset);
            }
        }

        public void WriteULongBE(int offset, ulong val)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, sizeof(ulong));

            if (BitConverter.IsLittleEndian)
            {
                *(ulong*)(Pointer + offset) = BinaryPrimitives.ReverseEndianness(val);
            }
            else
            {
                *(ulong*)(Pointer + offset) = val;                
            }
        }

        [Conditional("BYTESPAN_BOUNDSCHECK")]
        [Conditional("DEBUG")]
        private void RangeCheck(byte* p, int size)
        {
            Perf.CallCount();

            if (Pointer == null)
            {
                throw new NullReferenceException("ByteSpan is null!");
            }

            if (p < Pointer || p + size > Pointer + Length)
            {
                throw new ArgumentOutOfRangeException("Pointer", "Pointer is out of bounds!");
            }
        }

        internal void WriteBytes(int offset, ref ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            RangeCheck(Pointer + offset, (int)key.Length);

            fixed (byte* psource = key)
            {
                MemUtils.MemoryMove(Pointer + offset, psource, key.Length);
            }
        }

        #endregion
    }
}
