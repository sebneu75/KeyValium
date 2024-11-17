using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Shared
{
    public class PrivateData
    {
        // Source Directory Debug
        public string SourceDirectoryDebug;

        // Source Directory Release
        public string SourceDirectoryRelease;

        // Source Directory of Tools to be copied
        public string SourceDirectory
        {
            get
            {
#if DEBUG
                return SourceDirectoryDebug;
#else
                return SourceDirectoryRelease;
#endif
            }
        }

        // the folder containing the control files
        public string NetworkPath;

        // the machines to run the test on
        public List<MachineInfo> Machines = new List<MachineInfo>();
    }
}
