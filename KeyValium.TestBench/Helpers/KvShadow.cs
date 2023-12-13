using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Helpers
{
    internal class KvShadow
    {
        public KvShadow()
        {
            Items = new Dictionary<string, KVEntry>();
        }

        private KvShadow(Dictionary<string, KVEntry> items)
        {
            Items = items;
        }

        public readonly Dictionary<string, KVEntry> Items;

        internal void Clear()
        {
            Items.Clear();
        }

        internal KvShadow Copy()
        {
            var ret = new KvShadow(Items.ToDictionary(x => x.Key, x => x.Value));

            return ret;
        }
    }
}
