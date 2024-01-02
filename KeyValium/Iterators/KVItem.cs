using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Iterators
{
    /// <summary>
    /// Wrapper for a ValueRef.
    /// </summary>
    public sealed class KvItem
    {
        internal KvItem(Cursor cursor)
        {
            Perf.CallCount();

            _cursor = cursor;
        }

        private readonly Cursor _cursor;

        /// <summary>
        /// Returns the current ValueRef
        /// </summary>
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
