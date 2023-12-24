using KeyValium.Options;

namespace KeyValium.Pages.Headers
{
    /// <summary>
    /// common base of headers
    /// </summary>
    internal unsafe struct UniversalHeader
    {
        internal const ushort HeaderSize = 32;

        internal UniversalHeader(ByteSpan header, uint pagesize)
        {
            Perf.CallCount();

            Header = header;
            PageSize = pagesize;
        }

        internal ByteSpan Header;

        internal uint PageSize;

        #region Common Header

        public void InitCommonHeader(ushort pagetype, KvPagenumber pageno)
        {
            Perf.CallCount();

            Magic = Limits.Magic;
            Unused1 = 0;
            PageType = pagetype;
            PageNumber = pageno;
        }

        /// <summary>
        /// 0x00 : 4 Bytes Magic
        /// </summary>
        public uint Magic
        {
            get
            {
                Perf.CallCount();

                return Header.ReadUInt(0x00);
            }
            set
            {
                Perf.CallCount();

                Header.WriteUInt(0x00, value);
            }
        }

        /// <summary>
        /// 0x04 : 2 Bytes PageType
        /// </summary>
        public ushort PageType
        {
            get
            {
                Perf.CallCount();

                return Header.ReadUShort(0x04);
            }
            set
            {
                Perf.CallCount();

                Header.WriteUShort(0x04, value);
            }
        }

        /// <summary>
        /// 0x06 : 2 Bytes Unused
        /// </summary>
        public ushort Unused1
        {
            get
            {
                Perf.CallCount();

                return Header.ReadUShort(0x06);
            }
            set
            {
                Perf.CallCount();

                Header.WriteUShort(0x06, value);
            }
        }

        /// <summary>
        /// 0x08 : 8 Bytes pagenumber
        /// </summary>
        public KvPagenumber PageNumber
        {
            get
            {
                Perf.CallCount();

                return Header.ReadULong(0x08);
            }
            private set
            {
                Perf.CallCount();

                Header.WriteULong(0x08, value);
            }
        }

        #endregion

        #region Shared by different headers

        /// <summary>
        /// 0x10 : 8 Bytes Transaction Id
        /// </summary>
        public KvTid Tid
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert((PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE) || PageType == PageTypes.DataOverflow || PageType == PageTypes.Meta, "Invalid page type!");

                return Header.ReadULong(0x10);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert((PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE) || PageType == PageTypes.DataOverflow || PageType == PageTypes.Meta, "Invalid page type!");

                Header.WriteULong(0x10, value);
            }
        }

        #endregion

        #region Content Header

        public void InitContentHeader(KvTid tid)
        {
            Perf.CallCount();

            KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

            KeyCount = 0;
            // TODO find better way to determine
            Low = (PageType == PageTypes.DataIndex || PageType == PageTypes.FsIndex) ? (ushort)sizeof(ulong) : (ushort)0;
            High = (ushort)(PageSize - HeaderSize - 1);
            Tid = tid;
            Unused2 = 0;
        }

        /// <summary>
        /// 0x18 : 2 Bytes Low (offset of first free byte in content)
        /// </summary>
        public ushort Low
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return Header.ReadUShort(0x18);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                Header.WriteUShort(0x18, value);
            }
        }

        /// <summary>
        /// 0x1a : 2 Bytes High (offset of last free byte in content)
        /// </summary>
        public ushort High
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return Header.ReadUShort(0x1a);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                Header.WriteUShort(0x1a, value);
            }
        }

        /// <summary>
        /// 0x1c : 2 Bytes KeyCount
        /// </summary>
        public ushort KeyCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return Header.ReadUShort(0x1c);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                Header.WriteUShort(0x1c, value);
            }
        }

        /// <summary>
        /// 0x1e : 2 Bytes Unused
        /// </summary>
        public ushort Unused2
        {
            get
            {
                Perf.CallCount();

                return Header.ReadUShort(0x1e);
            }
            set
            {
                Perf.CallCount();

                Header.WriteUShort(0x1e, value);
            }
        }


        public ushort FreeSpace
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return (ushort)(High - Low + 1);
            }
        }

        public ushort UsedSpace
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return (ushort)(ContentSize - FreeSpace);
            }
        }

        public ushort ContentSize
        {
            get
            {
                Perf.CallCount();

                //KvDebug.Assert(PageType >= PageTypes.MIN_CONTENT_PAGETYPE && PageType <= PageTypes.MAX_CONTENT_PAGETYPE, "Invalid page type!");

                return (ushort)(PageSize - HeaderSize);
            }
        }

        #endregion

        #region FileHeader

        public void InitFileHeader(ReadonlyDatabaseOptions options, ushort pagesizeexponent)
        {
            Perf.CallCount();

            KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

            InitCommonHeader(PageTypes.FileHeader, 0);

            Version = options.Version;
            PageSizeExponent = pagesizeexponent;
            InternalTypeCode = options.InternalTypeCode;
            UserTypeCode = options.UserTypeCode;
            Flags = options.Flags;
        }

        /// <summary>
        /// 0x10 : 2 Bytes Version
        /// </summary>
        public ushort Version
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return Header.ReadUShort(0x10);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUShort(0x10, value);
            }
        }

        /// <summary>
        /// 0x12 : 2 Bytes PageSize (exponent to base 2)
        /// </summary>
        public ushort PageSizeExponent
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return Header.ReadUShort(0x12);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUShort(0x12, value);
            }
        }

        /// <summary>
        /// 0x14 : Flags
        /// </summary>
        internal DatabaseFlags Flags
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return (DatabaseFlags)Header.ReadUShort(0x14);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUShort(0x14, (ushort)value);
            }
        }

        /// <summary>
        /// 0x16 : 2 bytes Unused
        /// </summary>
        internal ushort Unused3
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return Header.ReadUShort(0x16);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUShort(0x16, (ushort)value);
            }
        }

        /// <summary>
        /// 0x18 : 4 Bytes Internal TypeCode
        /// </summary>
        public uint InternalTypeCode
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return Header.ReadUInt(0x18);
            }
            internal set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUInt(0x18, value);
            }
        }

        /// <summary>
        /// 0x1c : 4 Bytes User TypeCode
        /// </summary>
        public uint UserTypeCode
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                return Header.ReadUInt(0x1c);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FileHeader, "Invalid page type!");

                Header.WriteUInt(0x1c, value);
            }
        }

        #endregion

        #region Meta Header

        #endregion

        #region Overflow Header

        public void InitOverflowHeader(KvTid tid)
        {
            Perf.CallCount();

            KvDebug.Assert(PageType == PageTypes.DataOverflow, "Invalid page type!");

            ContentLength = 0;
            Tid = tid;
        }

        /// <summary>
        /// 0x18 : 8 Bytes Content Length for overflow pages
        /// </summary>
        public ulong ContentLength
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataOverflow, "Invalid page type!");

                return Header.ReadULong(0x18);
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataOverflow, "Invalid page type!");

                Header.WriteULong(0x18, value);
            }
        }

        /// <summary>
        /// PageCount (number of pages including the page with header)
        /// </summary>
        public ulong PageCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataOverflow, "Invalid page type!");

                var pagesize = (ulong)PageSize;

                return (ContentLength + HeaderSize + pagesize - 1) / pagesize;
            }
        }

        #endregion
    }
}
