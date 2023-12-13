using System.Buffers.Binary;

namespace KeyValium.Pages.Entries
{
    /// <summary>
    /// wrapper for a Leaf Entry that does not reside within a node
    /// (for insert)
    /// </summary>
    internal struct FsEntryExtern
    {
        internal FsEntryExtern(KvPagenumber first, KvPagenumber last, KvTid tid)
        {
            Perf.CallCount();
                        
            FirstPage = first;
            LastPage = last;
            Tid = tid;
        }
                
        internal readonly KvPagenumber FirstPage;
        internal readonly KvPagenumber LastPage;
        internal readonly KvTid Tid;

        public ulong PageCount
        {
            get
            {
                Perf.CallCount();

                return LastPage - FirstPage + 1;
            }
        }
    }
}

