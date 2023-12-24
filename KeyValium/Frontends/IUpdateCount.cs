using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends
{
    internal interface IUpdateCount
    {
        ulong SetCount(ulong count);
    }
}
