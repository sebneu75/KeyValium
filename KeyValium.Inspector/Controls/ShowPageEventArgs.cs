using System;

namespace KeyValium.Inspector
{
    internal class ShowPageEventArgs : EventArgs
    {
        internal ShowPageEventArgs(KvPagenumber pageno)
        {
            PageNumber = pageno;
        }

        public KvPagenumber PageNumber
        {
            get;
            private set;
        }
    }
}