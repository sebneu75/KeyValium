using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    internal class SuspendedTreeRef
    {
        public SuspendedTreeRef(ulong tid, ulong treerefoid)
        {
            Perf.CallCount();

            Tid = tid;
            TreeRefOid = treerefoid;
        }

        public readonly ulong Tid;

        public readonly ulong TreeRefOid;

        public readonly List<SuspendedKeyPointer> KeyPointers = new();
    }
}
