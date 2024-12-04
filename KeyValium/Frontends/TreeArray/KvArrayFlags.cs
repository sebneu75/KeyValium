using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    [Flags]
    public enum KvArrayFlags : ushort
    {
        None = 0x0000,
        Zipped = 0x0001,
    }
}
