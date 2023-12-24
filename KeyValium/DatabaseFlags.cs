using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Options
{
    [Flags]
    internal enum DatabaseFlags : ushort
    {
        None = 0x0000,
        IndexedAccess = 0x0001,
    }
}
