namespace KeyValium.Pages
{
    public static unsafe class UniversalComparer
    {
        /// <summary>
        /// Same as CompareBytes(BytePointer, byte[])
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        public static int CompareBytes(byte[] key1, byte[] key2)
        {
            Perf.CallCount();

            fixed (byte* ptr = key1)
            {
                var bp = new ByteSpan(ptr, key1 == null ? (ushort)0 : (ushort)key1.Length);
                return CompareBytesByteWise(bp, key2);
            }
        }

        internal static int CompareBytesByteWise(ByteSpan key1, Span<byte> key2)
        {
            Perf.CallCount();

            var len2 = key2 == null ? 0 : key2.Length;
            var len = key1.Length < len2 ? key1.Length : len2;

            var keyptr1 = key1.Pointer;
            fixed (byte* ptr = key2)
            {
                var keyptr2 = ptr;

                for (int i = 0; i < len; i++, keyptr1++, keyptr2++)
                {
                    if (*keyptr1 == *keyptr2)
                        continue;

                    return *keyptr1 < *keyptr2 ? -1 : +1;
                }

                if (key1.Length == len2)
                {
                    return 0;
                }

                return key1.Length < len2 ? -1 : +1;
            }
        }

        internal static int CompareBytesUlongWise(ByteSpan key1, ref ReadOnlySpan<byte> key2)
        {
            Perf.CallCount();

            var len2 = key2 == null ? 0 : key2.Length;
            var len = key1.Length < len2 ? key1.Length : len2;

            var keyptr1 = key1.Pointer;
            fixed (byte* ptr = key2)
            {
                var keyptr2 = ptr;

                var octets = len >> 3;
                var remainder = len & 7;

                for (int i = 0; i < octets; i++, keyptr1 += sizeof(ulong), keyptr2 += sizeof(ulong))
                {
                    var u1 = Unsafe.ReadULongBE(keyptr1);
                    var u2 = Unsafe.ReadULongBE(keyptr2);

                    if (u1 == u2)
                        continue;

                    return u1 < u2 ? -1 : +1;
                }

                // compare 4 bytes
                if (remainder >= 4)
                {
                    var u1 = Unsafe.ReadUIntBE(keyptr1);
                    var u2 = Unsafe.ReadUIntBE(keyptr2);

                    if (u1 != u2)
                    {
                        return u1 < u2 ? -1 : +1;
                    }

                    keyptr1 += sizeof(uint);
                    keyptr2 += sizeof(uint);
                    remainder -= sizeof(uint);
                }

                // compare 2 bytes
                if (remainder >= 2)
                {
                    var u1 = Unsafe.ReadUShortBE(keyptr1);
                    var u2 = Unsafe.ReadUShortBE(keyptr2);

                    if (u1 != u2)
                    {
                        return u1 < u2 ? -1 : +1;
                    }

                    keyptr1 += sizeof(ushort);
                    keyptr2 += sizeof(ushort);
                    remainder -= sizeof(ushort);
                }

                // compare 1 byte
                if (remainder >= 1)
                {
                    if (*keyptr1 != *keyptr2)
                    {
                        return *keyptr1 < *keyptr2 ? -1 : +1;
                    }
                }

                if (key1.Length == len2)
                {
                    return 0;
                }

                return key1.Length < len2 ? -1 : +1;
            }
        }

        internal static int CompareBytesUlongWise(ByteSpan key1, ByteSpan key2)
        {
            Perf.CallCount();

            var len = key1.Length < key2.Length ? key1.Length : key2.Length;

            var keyptr1 = key1.Pointer;
            var keyptr2 = key2.Pointer;

            var octets = len >> 3;
            var remainder = len & 7;

            for (int i = 0; i < octets; i++, keyptr1 += sizeof(ulong), keyptr2 += sizeof(ulong))
            {
                var u1 = Unsafe.ReadULongBE(keyptr1);
                var u2 = Unsafe.ReadULongBE(keyptr2);

                if (u1 == u2)
                    continue;

                return u1 < u2 ? -1 : +1;
            }

            // compare 4 bytes
            if (remainder >= 4)
            {
                var u1 = Unsafe.ReadUIntBE(keyptr1);
                var u2 = Unsafe.ReadUIntBE(keyptr2);

                if (u1 != u2)
                {
                    return u1 < u2 ? -1 : +1;
                }

                keyptr1 += sizeof(uint);
                keyptr2 += sizeof(uint);
                remainder -= sizeof(uint);
            }

            // compare 2 bytes
            if (remainder >= 2)
            {
                var u1 = Unsafe.ReadUShortBE(keyptr1);
                var u2 = Unsafe.ReadUShortBE(keyptr2);

                if (u1 != u2)
                {
                    return u1 < u2 ? -1 : +1;
                }

                keyptr1 += sizeof(ushort);
                keyptr2 += sizeof(ushort);
                remainder -= sizeof(ushort);
            }

            // compare 1 byte
            if (remainder >= 1)
            {
                if (*keyptr1 != *keyptr2)
                {
                    return *keyptr1 < *keyptr2 ? -1 : +1;
                }
            }

            if (key1.Length == key2.Length)
            {
                return 0;
            }

            return key1.Length < key2.Length ? -1 : +1;
        }

        internal static int CompareBytesAsSequence(ByteSpan key1, ref ReadOnlySpan<byte> key2)
        {
            Perf.CallCount();

            return key1.ReadOnlySpan.SequenceCompareTo(key2);
        }

        /// <summary>
        /// key1 points to the entry, so we need to respect the endianness
        /// key2 points to internal memory
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        internal static int CompareFreespace(ByteSpan key1, ref ReadOnlySpan<byte> key2)
        {
            Perf.CallCount();

            KvDebug.Assert(key1.Length == Limits.FreespaceKeySize && key2.Length == Limits.FreespaceKeySize, "Length mismatch!");

            fixed (byte* bp = key2)
            {
                var entryfirst = Unsafe.ReadULong(key1.Pointer + 0x08);
                var first = Unsafe.ReadULong(bp + 0x08);
                if (entryfirst == first)
                {
                    var entrylast = Unsafe.ReadULong(key1.Pointer + 0x10);
                    var last = Unsafe.ReadULong(bp + 0x10);
                    if (entrylast == last)
                    {
                        var entrytid = Unsafe.ReadULong(key1.Pointer + 0x00);
                        var tid = Unsafe.ReadULong(bp);
                        if (entrytid == tid)
                        {
                            return 0;
                        }

                        return entrytid < tid ? -1 : +1;
                    }

                    return entrylast < last ? -1 : +1;
                }

                return entryfirst < first ? -1 : +1;
            }
        }

        //public static int CompareFreespace(BytePointer key1, BytePointer key2)
        //{
        //    KvDebug.Assert(key1.Length == EntryInline.FREESPACE_KEY_LENGTH && key2.Length == EntryInline.FREESPACE_KEY_LENGTH, "Length mismatch!");

        //    var entrytid = Unsafe.ReadULong(key1.Pointer);
        //    var tid = Unsafe.ReadULong(key2.Pointer);

        //    if (entrytid == tid)
        //    {
        //        var entryfirst = Unsafe.ReadULong(key1.Pointer + 0x08);
        //        var first = Unsafe.ReadULong(key2.Pointer + 0x08);
        //        if (entryfirst == first)
        //        {
        //            var entrylast = Unsafe.ReadULong(key1.Pointer + 0x10);
        //            var last = Unsafe.ReadULong(key2.Pointer + 0x10);
        //            if (entrylast == last)
        //            {
        //                return 0;
        //            }

        //            return entrylast < last ? -1 : +1;
        //        }

        //        return entryfirst < first ? -1 : +1;
        //    }

        //    return entrytid < tid ? -1 : +1;
        //}
    }
}

