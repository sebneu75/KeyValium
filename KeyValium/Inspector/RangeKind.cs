using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector
{
    internal enum RangeKind : short
    {
        FullBlock=0,
        Start=1,
        Center = 3,
        End =2,        
    }
}
