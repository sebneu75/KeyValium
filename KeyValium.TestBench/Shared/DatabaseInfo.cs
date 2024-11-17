using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Shared
{
    public class DatabaseInfo
    {
        // path to database file
        public string Filename;

        // sharing mode to use
        public InternalSharingModes SharingMode;

        // number of database instances per process
        public int Instances;

        // number of writer threads per instance
        public readonly int Writers = 1;

        // number of reader threads per instance
        public int Readers;
    }
}
