using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector
{
    public class FsEntry
    {
        internal FsEntry()
        {
        }

        public KvPagenumber Pagenumber;

        public ushort Index;

        public KvTid Tid;

        public KvPagenumber FirstPage;

        public KvPagenumber LastPage;
    }
}
