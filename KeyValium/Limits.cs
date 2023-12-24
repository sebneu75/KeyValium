using KeyValium.Pages.Headers;

namespace KeyValium
{
    internal class Limits
    {
        static Limits()
        {
            MaxByteArraySize = Array.MaxLength; // 2147483591
            MaxByteArrayIndex = MaxByteArraySize - 1;
        }

        /// <summary>
        /// Internal constant offsets and sizes
        /// </summary>
        
        // Offsets of keylength and keybytes are fixed
        internal const ushort OffsetFlags = 0;
        internal const ushort OffsetKeyLength = OffsetFlags + sizeof(ushort);
        internal const ushort OffsetKeyBytes = OffsetKeyLength + sizeof(ushort);
        
        // size of offset entries
        internal const ushort OffsetEntrySize = sizeof(ushort);

        // Freespace Entries have fixed length
        internal const int FreespaceKeySize = sizeof(KvPagenumber);
        internal const ushort FreespaceIndexEntrySize = OffsetKeyBytes + FreespaceKeySize;
        internal const ushort FreespaceLeafEntrySize = FreespaceIndexEntrySize + sizeof(KvPagenumber) + sizeof(KvTid);

        /// <summary>
        /// Constants for TrackingScope and CursorScope
        /// </summary>
        internal const int TrackingScope_None = 1;
        internal const int TrackingScope_TransactionChain = 2;
        internal const int TrackingScope_Database = 3;

        /// <summary>
        /// Magic value in headers
        /// </summary>
        public static readonly uint Magic = (uint)(('K' << 8 | 'V') << 8 | '£') << 8 | 'µ'; // KeyVa£iuµ

        /// <summary>
        /// to detect differences in endianess
        /// </summary>
        //public static readonly uint ReverseMagic = BinaryPrimitives.ReverseEndianness(Magic);

        /// <summary>
        /// maximum size of byte array that can be allocated
        /// </summary>
        public static readonly int MaxByteArraySize; // = 2147483591

        /// <summary>
        /// maximum index of byte array
        /// </summary>
        public static readonly int MaxByteArrayIndex; // = MaxByteArraySize - 1;

        /// <summary>
        /// minimum page size
        /// </summary>
        public const int MinPageSize = 256;

        /// <summary>
        /// maximum page size
        /// </summary>
        public const int MaxPageSize = 65536;

        /// <summary>
        /// minimum number of keys that must fit on a leaf page
        /// </summary>
        public const ushort MinKeysPerLeafPage = 2;

        /// <summary>
        /// minimum number of keys that must fit on an index page
        /// </summary>
        public const ushort MinKeysPerIndexPage = 3;

        /// <summary>
        /// Number of MetaPages
        /// </summary>
        public const ushort MetaPages = 2;

        /// <summary>
        /// Pagenumber of the First MetaPage
        /// </summary>
        public const KvPagenumber FirstMetaPage = 1;

        /// <summary>
        /// Pagenumber of first data page
        /// </summary>
        public const KvPagenumber MinDataPageNumber = FirstMetaPage + MetaPages;

        public static ushort GetMaxKeyLength(uint pagesize)
        {
            Perf.CallCount();

            ValidatePageSize(pagesize);

            switch (pagesize)
            {
                //case 128:
                //    return 16;
                case 256:
                    return 48;
                default:
                    return (ushort)(pagesize >> 2);
            }
        }

        // maximum size of metadata per data leaf entry (currently 32 Bytes)
        internal const int MetaDataSizePerEntry = sizeof(ushort) +           // Flags
                                                  sizeof(ushort) +           // KeyLength
                                                  sizeof(KvPagenumber) +     // Subtree
                                                  sizeof(ulong) +            // TotalCount
                                                  sizeof(ulong) +            // LocalCount
                                                  sizeof(ushort) +           // ValueLength
                                                  sizeof(ushort);            // OffsetTableEntry

        public static ushort GetMaxKeyValueSize(uint pagesize)
        {
            Perf.CallCount();

            ValidatePageSize(pagesize);

            return (ushort)((pagesize - UniversalHeader.HeaderSize - MinKeysPerLeafPage * MetaDataSizePerEntry) / MinKeysPerLeafPage);
        }

        /// <summary>
        /// checks the pagesize and returns log2
        /// </summary>
        /// <param name="pagesize">pagesize in bytes</param>
        /// <returns>log2 of pagesize</returns>
        public static ushort ValidatePageSize(uint pagesize)
        {
            Perf.CallCount();

            switch (pagesize)
            {
                case 256:
                    return 8;
                case 512:
                    return 9;
                case 1024:
                    return 10;
                case 2048:
                    return 11;
                case 4096:
                    return 12;
                case 8192:
                    return 13;
                case 16384:
                    return 14;
                case 32768:
                    return 15;
                case 65536:
                    return 16;
                default:
                    var msg = string.Format("PageSize must be a power of 2 in the range of {0}-{1} inclusive.", MinPageSize, MaxPageSize);
                    throw new NotSupportedException(msg);
            }
        }

        ///// <summary>
        ///// maximum entry size including metadata
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //public static ushort MaxTotalEntrySize(ContentPage page)
        //{
        //    if (page.IsIndexPage)
        //    {
        //        return (ushort)(GetMaxKeyLength(page.Page.PageSize) + sizeof(ushort) + sizeof(ushort) + page.BranchSize + page.OffsetEntrySize);
        //    }
        //    else
        //    {
        //        return (ushort)(page.Header.ContentSize / MinKeysPerLeafPage);
        //    }

        //    //return (ushort)((pagesize - UniversalHeader.HeaderSize - (MinKeysPerLeafPage * 16)) / MinKeysPerLeafPage);
        //}

        public static ushort GetMaxInlineValueSize(uint pagesize, ushort keysize)
        {
            Perf.CallCount();

            ValidatePageSize(pagesize);

            return (ushort)(GetMaxKeyValueSize(pagesize) - keysize);
        }

        public Limits(Database database)
        {
            Perf.CallCount();

            PageSize = database.Options.PageSize;
            MaxKeySize = GetMaxKeyLength(PageSize);
            MaxKeyValueSize = GetMaxKeyValueSize(PageSize);
        }

        #region Limits

        /// <summary>
        /// maximum Size of a Key
        /// </summary>
        public readonly ushort MaxKeySize;

        /// <summary>
        /// Maximum Length of Key + Value
        /// </summary>
        public readonly ushort MaxKeyValueSize;

        /// <summary>
        /// page size
        /// </summary>
        public readonly uint PageSize;

        /// <summary>
        /// maximum Length of a value that is stored inline (depends on keysize)
        /// </summary>
        public ushort MaxInlineValueSize(ushort keysize)
        {
            Perf.CallCount();

            return (ushort)(MaxKeyValueSize - keysize);
        }

        #endregion
    }
}
