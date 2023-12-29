using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium
{
    /// <summary>
    /// Tracking scopes. Used by TreeRefs and Cursors.
    /// </summary>
    public enum TrackingScope    
    {
        /// <summary>
        /// The object is tracked for the lifetime of the transaction instance.
        /// </summary>
        TransactionChain = Limits.TrackingScope_TransactionChain,

        /// <summary>
        /// The object is tracked for the lifetime of the database instance.
        /// </summary>
        Database = Limits.TrackingScope_Database
    }
}
