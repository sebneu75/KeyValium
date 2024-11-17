namespace KeyValium.Inspector
{
    internal class PageInfo
    {
        public KvPagenumber PageNumber
        {
            get;
            internal set;
        }

        public PageTypesI PageType
        {
            get;
            internal set;
        }

        public int UnusedSpace
        {
            get;
            internal set;
        }

        public RangeKind RangeKind
        {
            get;
            internal set;
        }
    }
}
