using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Performance
{
    internal class DefaultStopwatch : IKvStopwatch
    {
        long IKvStopwatch.GetTicks()
        {
            return Stopwatch.GetTimestamp();
        }
    }
}
