using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    internal class SuspendedKeyPointer
    {
        public SuspendedKeyPointer(ulong pageno, int keyindex) 
        {
            Perf.CallCount();

            PageNumber = pageno;
            KeyIndex = keyindex;
        }

        public readonly ulong PageNumber;

        public readonly int KeyIndex;
    }
}
