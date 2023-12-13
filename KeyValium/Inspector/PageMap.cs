namespace KeyValium.Inspector
{
    public class PageMap
    {
        internal PageMap()
        {
            Entries = new List<EntryInfo>();
        }

        public KvPagenumber PageNumber
        {
            get;
            internal set;
        }

        public uint PageSize
        {
            get;
            internal set;
        }

        public PageTypesI PageType
        {
            get;
            internal set;
        }

        public ushort? PageTypeReal
        {
            get;
            internal set;
        }

        public uint? Magic
        {
            get;
            internal set;
        }

        public KvTid? Tid
        {
            get;
            internal set;
        }

        public ushort? KeyCount
        {
            get;
            internal set;
        }

        public ushort? Low
        {
            get;
            internal set;
        }

        public ushort? High
        {
            get;
            internal set;
        }

        public uint? ContentSize
        {
            get;
            internal set;
        }

        public ulong? ContentLength
        {
            get;
            internal set;
        }

        public uint? FreeSpace
        {
            get;
            internal set;
        }

        public uint? UsedSpace
        {
            get;
            internal set;
        }

        public List<EntryInfo> Entries
        {
            get;
            private set;
        }

        public byte[] Bytes
        {
            get;
            internal set;
        }

        public ByteRange Map
        {
            get;
            set;
        }
    }
}
