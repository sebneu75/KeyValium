using System.Buffers.Binary;

namespace KeyValium
{
    internal static unsafe class Unsafe
    {
        //public static ushort ReadUShort(byte* p)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *(ushort*)p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(ushort*)p);
        //    }
        //}

        //public static ushort ReadUShort(ushort* p)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*p);
        //    }
        //}

        //public static void WriteUShort(byte* p, ushort val)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *(ushort*)p = val;
        //    }
        //    else
        //    {
        //        *(ushort*)p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        //public static void WriteUShort(ushort* p, ushort val)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *p = val;
        //    }
        //    else
        //    {
        //        *p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        public static ulong ReadULong(byte* p)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                return *(ulong*)p;
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(ulong*)p);
            }
        }

        public static void WriteULong(byte* p, ulong val)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                *(ulong*)p = val;
            }
            else
            {
                *(ulong*)p = BinaryPrimitives.ReverseEndianness(val);
            }
        }

        //public static long ReadLong(byte* p)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *(long*)p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(long*)p);
        //    }
        //}

        //public static void WriteLong(byte* p, long val)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *(long*)p = val;
        //    }
        //    else
        //    {
        //        *(long*)p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        //public static uint ReadUInt(byte* p)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *(uint*)p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(uint*)p);
        //    }
        //}

        //public static void WriteUInt(byte* p, uint val)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *(uint*)p = val;
        //    }
        //    else
        //    {
        //        *(uint*)p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        //public static int ReadInt(byte* p)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return *(int*)p;
        //    }
        //    else
        //    {
        //        return BinaryPrimitives.ReverseEndianness(*(int*)p);
        //    }
        //}

        //public static void WriteInt(byte* p, int val)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        *(int*)p = val;
        //    }
        //    else
        //    {
        //        *(int*)p = BinaryPrimitives.ReverseEndianness(val);
        //    }
        //}

        public static ushort ReadUShortBE(byte* p)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                return BinaryPrimitives.ReverseEndianness(*(ushort*)p);
            }
            else
            {
                return *(ushort*)p;
            }
        }

        public static uint ReadUIntBE(byte* p)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                return BinaryPrimitives.ReverseEndianness(*(uint*)p);
            }
            else
            {
                return *(uint*)p;
            }
        }

        public static ulong ReadULongBE(byte* p)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                return BinaryPrimitives.ReverseEndianness(*(ulong*)p);
            }
            else
            {
                return *(ulong*)p;
            }
        }

        public static void WriteULongBE(byte* p, ulong val)
        {
            Perf.CallCount();

            if (BitConverter.IsLittleEndian)
            {
                *(ulong*)p = BinaryPrimitives.ReverseEndianness(val);                
            }
            else
            {
                *(ulong*)p = val;
            }
        }
    }
}
