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
            DataGlobalCount = datagc;
            DataLocalCount = datalc;
            FsGlobalCount = fsgc;
            FsLocalCount = fslc;
        }

        internal readonly KvPagenumber PageNumber;

        internal readonly KvTid Tid;

        internal readonly KvPagenumber DataRootPage;

        internal readonly KvPagenumber FsRootPage;

        internal readonly KvPagenumber LastPage;

        internal readonly ulong DataGlobalCount;

        internal readonly ulong DataLocalCount;

        internal readonly ulong FsGlobalCount;

        internal readonly ulong FsLocalCount;
    }
}