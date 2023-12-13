using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Iterators
{
    public sealed class KVItem
    {
        private readonly Cursor _cursor;

        internal KVItem(Cursor cursor)
        {
            Perf.CallCount();

            _cursor = cursor;
        }

        public ValueRef Value
        {
            get
            {
                Perf.CallCount();

                lock (_cursor.CurrentTransaction.TxLock)
                {
                    return _cursor.GetCurrentValue();
                }
            }
        }
    }
}
