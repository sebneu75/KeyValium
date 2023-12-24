namespace KeyValium.Inspector
{
    public class DatabaseProperties
    {
        internal DatabaseProperties()
        {
            MetaInfos = new List<MetaInfo>();
        }

        public string Filename
        {
            get;
            internal set;
        }

        public ushort MaxKeyAndValueSize
        {
            get;
            internal set;
        }

        public ushort MaxKeySize
        {
            get;
            internal set;
        }

        public ushort MetaPages
        {
            get;
            internal set;
        }

        public ushort MinKeysPerIndexPage
        {
            get;
            internal set;
        }

        public ushort MinKeysPerLeafPage
        {
            get;
            internal set;
        }

        public uint PageSize
        {
            get;
            internal set;
        }

        public bool SwapEndianess
        {
            get;
            internal set;
        }

        public ushort Version
        {
            get;
            internal set;
        }

        public KvPagenumber FirstMetaPage
        {
            get;
            internal set;
        }

        public KvPagenumber FirstDataPage
        {
            get
            {
                return FirstMetaPage + MetaPages;
            }
        }

        public long FileSize
        {
            get;
            internal set;
        }

        public long PageCount
        {
            get
            {
                return FileSize / PageSize;
            }
        }

        public uint InternalTypecode
        {
            get;
            internal set;
        }

        public uint UserTypecode
        {
            get;
            internal set;
        }

        public List<MetaInfo> MetaInfos
        {
            get;
            private set;
        }

    }
}
