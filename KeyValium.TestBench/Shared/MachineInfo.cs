using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Shared
{
    /// <summary>
    /// represents the machines used in testing
    /// </summary>
    public class MachineInfo
    {
        public MachineInfo()
        {
        }

        public string Name;

        public string RemotePath;

        public string LocalPath;

        public string ProcStartFile;
    }
}
