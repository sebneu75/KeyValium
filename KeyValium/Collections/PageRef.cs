
using System.Runtime.InteropServices;

namespace KeyValium.Collections
{
    /// <summary>
    /// represents an entry in the page cache
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal struct PageRef
    {
        /// <summary>
        /// constructs an instance
        /// </summary>
        /// <param name="pageno">the page number</param>
        /// <param name="page">the actual page</param>
        /// <param name="tid">the transaction id</param>
        internal PageRef(KvPagenumber pageno, AnyPage page, KvTid tid)
        {
            Perf.CallCount();

            _page = page;
            _page?.AddRef();

            PageNumber = pageno;            
            Tid = tid;
            Slot = -1;
        }

        internal readonly KvPagenumber PageNumber;

        internal KvTid Tid;
        
        internal int Slot;

        private AnyPage _page;

        internal AnyPage Page
        {
            get
            {
                Perf.CallCount();

                return _page;
            }
            set
            {
                Perf.CallCount();

                if (value != _page)
                {
                    value?.AddRef();
                    _page?.Dispose();
                    _page = value;
                }
            }
        }
    }
}
