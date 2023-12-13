using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium
{
    internal class KvDebugOptions
    {
        static KvDebugOptions()
        {
            //ValidateIndex = true;
            //ValidateEntries= true;
            //FillFreeSpace = true;
            PerformanceCount = true;
        }

        public static bool ValidateIndex { get; internal set; }
        public static bool ValidateEntries { get; internal set; }
        public static bool FillFreeSpace { get; internal set; }
        public static bool PerformanceCount { get; internal set; }
    }
}
