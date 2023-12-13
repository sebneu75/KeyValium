using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    internal enum InternalTrackingScope
    {
        None = Limits.TrackingScope_None,
        TransactionChain = Limits.TrackingScope_TransactionChain,
        Database = Limits.TrackingScope_Database,
    }
}
