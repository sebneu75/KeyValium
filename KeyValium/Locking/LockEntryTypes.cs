using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    internal static class LockEntryTypes
    {
        // Entry types
        internal const ushort Free = 0x0000;    // free entry
        internal const ushort Reader = 0x5252;  // RR   reader entry
        internal const ushort Writer = 0x5757;  // WW   writer entry
    }
}
