using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    public enum TrackingScope
    {
        TransactionChain = Limits.TrackingScope_TransactionChain,
        Database= Limits.TrackingScope_Database,
    }
}
