using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace KeyValium
{
    internal class Util
    {
        public static unsafe void Copy(byte* source, byte* target, int length)
        {
            var srcspan = new ReadOnlySpan<byte>(source, length);
            var count = length / Vector<byte>.Count;

            ReadOnlySpan<Vector<byte>> sv = MemoryMarshal.Cast<byte, Vector<byte>>(srcspan);
            Span<Vector<byte>> tv = MemoryMarshal.Cast<byte, Vector<byte>>(new Span<byte>(target, length));

            for (var i = 0; i < count; i++)
            {
                tv[i] = sv[i];
            }

            //var x = Vector<byte>.Count;

        }

        public static string GetHexString(ReadOnlySpan<byte> bytes)
        {
            return GetHexString(bytes.ToArray());
        }

        //public static string GetHexString(ByteStream bytes)
        //{
        //    return GetHexString(bytes.ToArray());
        //}

        public static string GetHexString(byte[] bytes)
        {
            if (bytes == null)
            {
                return "<null>";
            }
            if (bytes.Length == 0)
            {
                return "<0>";
            }

            var sb = new StringBuilder(2 * bytes.Length);

            for (int i = 0; i < bytes.Length; i++)
            {
                //if (i > 0 && i % 8 == 0)
                //{
                //    sb.Append(".");
                //}

                sb.Append(bytes[i].ToString("x2"));
            }

            var ret = sb.ToString().TrimStart('0');
            if (ret == "")
            {
                ret = "0";
            }

            return ret;
        }

        public static unsafe string GetHexString(ByteSpan bp, int maxlen)
        {
            if (bp.Length == 0)
            {
                return "";
            }

            var len = Math.Min(maxlen, bp.Length);

            var sb = new StringBuilder(2 * bp.Length);

            for (int i = 0; i < len; i++)
            {
                var b = *(bp.Pointer + i);
                sb.Append(b.ToString("x2"));
            }

            if (bp.Length > maxlen)
            {
                sb.Append("...");
            }

            return sb.ToString();
        }


        public static unsafe void MemoryCopy(byte* source, byte* target, int length)
        {
            if (source == target || length <= 0)
            {
                return;
            }

            // TODO 8-byte alignment

            var octets = length >> 3;
            var remainder = length & 7;

            // backward copy when
            // source< target && source+length>target

            // A) S -------             forward
            //              T-------

            // B)           S-------    forward
            //    T-------

            // C) S-------              backward
            //        T-------

            // D)     S-------          forward
            //    T-------

            if (source < target && (source + length) > target)
            {
                //
                // copy backward
                //

                source += length;
                target += length;

                // compare 4 bytes
                if (remainder >= 4)
                {
                    source -= sizeof(uint);
                    target -= sizeof(uint);

                    *(uint*)target = *(uint*)source;

                    remainder -= sizeof(uint);
                }

                // compare 2 bytes
                if (remainder >= 2)
                {
                    source -= sizeof(ushort);
                    target -= sizeof(ushort);

                    *(ushort*)target = *(ushort*)source;

                    remainder -= sizeof(ushort);
                }

                // compare 1 byte
                if (remainder >= 1)
                {
                    *--target = *--source;
                }

                while (octets > 0)
                {
                    source -= sizeof(ulong);
                    target -= sizeof(ulong);

                    *(ulong*)target = *(ulong*)source;

                    octets--;
                }
            }
            else
            {
                //
                // copy forward
                //
                while (octets > 0)
                {
                    *(ulong*)target = *(ulong*)source;
                    source += sizeof(ulong);
                    target += sizeof(ulong);

                    octets--;
                }

                // compare 4 bytes
                if (remainder >= 4)
                {
                    *(uint*)target = *(uint*)source;

                    source += sizeof(uint);
                    target += sizeof(uint);
                    remainder -= sizeof(uint);
                }

                // compare 2 bytes
                if (remainder >= 2)
                {
                    *(ushort*)target = *(ushort*)source;

                    source += sizeof(ushort);
                    target += sizeof(ushort);
                    remainder -= sizeof(ushort);
                }

                // compare 1 byte
                if (remainder >= 1)
                {
                    *target = *source;
                }
            }
        }

        public static unsafe void MemoryZero(byte* target, int length)
        {
            if (length <= 0)
            {
                return;
            }

            // TODO 8-byte alignment
            var octets = length >> 3;
            var remainder = length & 7;

            while (octets > 0)
            {
                *(ulong*)target = 0;

                target += sizeof(ulong);
                octets--;
            }

            // compare 4 bytes
            if (remainder >= 4)
            {
                *(uint*)target = 0;

                target += sizeof(uint);
                remainder -= sizeof(uint);
            }

            // compare 2 bytes
            if (remainder >= 2)
            {
                *(ushort*)target = 0;

                target += sizeof(ushort);
                remainder -= sizeof(ushort);
            }

            // compare 1 byte
            if (remainder >= 1)
            {
                *target = 0;
            }
        }
    }
}
