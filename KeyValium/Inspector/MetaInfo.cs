using System.Globalization;

namespace KeyValium.Inspector
{
    public sealed class MetaInfo
    {
        internal MetaInfo() 
        { 
        } 

        public ushort Index
        {
            get;
            internal set;
        }

        public KvPagenumber DataRootPage
        {
            get;
            internal set;
        }

        public KvPagenumber FsRootPage
        {
            get;
            internal set;
        }

        public KvPagenumber PageNumber
        {
            get;
            internal set;
        }

        public KvTid Tid
        {
            get;
            internal set;
        }

        public KvTid HeaderTid
        {
            get;
            internal set;
        }

        public KvTid FooterTid
        {
            get;
            internal set;
        }

        public KvPagenumber LastPage
        {
            get;
            internal set;
        }

        public ulong DataTotalCount
        {
            get;
            internal set;
        }

        public ulong DataLocalCount
        {
            get;
            internal set;
        }

        public ulong FsTotalCount
        {
            get;
            internal set;
        }

        public ulong FsLocalCount
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "Meta {0:N0} (Tid: {1:N0})", Index, Tid);
        }
    }
}
