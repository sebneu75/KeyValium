
using System.Runtime.InteropServices;

namespace KeyValium.Collections
{
    [StructLayout(LayoutKind.Auto)]
    internal struct PageRange : IComparable<PageRange>
    {
        internal static readonly PageRange Empty = new PageRange(KvPagenumber.MaxValue, 0);

        internal PageRange(KvPagenumber first, KvPagenumber last)
        {
            Perf.CallCount();

            First = first;
            Last = last;
        }

        internal KvPagenumber First;

        internal KvPagenumber Last;

        internal ulong PageCount
        {
            get
            {
                Perf.CallCount();

                return Last - First + 1;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                Perf.CallCount();

                return First > Last;
            }
        }

        internal bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return First <= pageno && pageno <= Last;
        }

        /// <summary>
        /// checks if range is completely contained within this range
        /// </summary>
        /// <param name="range"></param>
        /// <returns>true if range is completely contained within this range</returns>
        internal bool Contains(PageRange range)
        {
            Perf.CallCount();

            return First <= range.First && range.Last <= Last;
        }

        internal bool Overlaps(ref PageRange other)
        {
            Perf.CallCount();

            return this.Contains(other.First) || this.Contains(other.Last) ||
                   other.Contains(this.First) || other.Contains(this.Last);
        }

        #region IComparable

        public int CompareTo(PageRange other)
        {
            Perf.CallCount();

            if (First < other.First)
            {
                return -1;
            }
            else if (First > other.First)
            {
                return +1;
            }

            return 0;
        }

        #endregion

        public override string ToString()
        {
            Perf.CallCount();

            return string.Format("[{0}-{1}]", First, Last);
        }
    }
}
