using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    internal class CursorTrackerStats
    {
        internal CursorTrackerStats(int treerefs, int cursors, int suspendedcount)
        {
            KeyRefCount = treerefs;
            CursorCount = cursors;
            SuspendedKeyRefCount = suspendedcount;
        }

        public readonly int KeyRefCount;
        public readonly int CursorCount;
        public readonly int SuspendedKeyRefCount;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("CursorTracker.KeyRefCount: {0}\n", KeyRefCount);
            sb.AppendFormat("CursorTracker.CursorCount: {0}\n", CursorCount);
            sb.AppendFormat("CursorTracker.SuspendedKeyRefCount: {0}\n", SuspendedKeyRefCount);

            return sb.ToString();
        }
    }
}
