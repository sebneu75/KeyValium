namespace KeyValium
{
    internal struct MetaEntry
    {
        internal MetaEntry(KvPagenumber pageno, KvTid tid, KvPagenumber dataroot, KvPagenumber fsroot, KvPagenumber lastpage,
                           ulong datagc, ulong datalc, ulong fsgc, ulong fslc)
        {
            Perf.CallCount();

            PageNumber = pageno;
            Tid = tid;
            DataRootPage = dataroot;
            FsRootPage = fsroot;
            LastPage = lastpage;
            DataTotalCount = datagc;
            DataLocalCount = datalc;
            FsTotalCount = fsgc;
            FsLocalCount = fslc;
        }

        internal readonly KvPagenumber PageNumber;

        internal readonly KvTid Tid;

        internal readonly KvPagenumber DataRootPage;

        internal readonly KvPagenumber FsRootPage;

        internal readonly KvPagenumber LastPage;

        internal readonly ulong DataTotalCount;

        internal readonly ulong DataLocalCount;

        internal readonly ulong FsTotalCount;

        internal readonly ulong FsLocalCount;
    }
}