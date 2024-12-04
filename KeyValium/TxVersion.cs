using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium
{
    [StructLayout(LayoutKind.Auto)]
    internal struct TxVersion
    {
        internal TxVersion(Transaction tx)
        {
            Perf.CallCount();

            Tx = tx;
            Version = tx.Version;
        }

        internal readonly Transaction Tx;

        internal readonly int Version;

        internal bool IsValid
        {
            get
            {
                Perf.CallCount();

                return Tx != null && Tx.State == TransactionStates.Active && Tx.Version == Version;
            }
        }

        internal void Validate()
        {
            Perf.CallCount();

            if (Tx == null)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "The transaction is invalid.");
            }

            if (Tx.State != TransactionStates.Active)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "The transaction is not active.");
            }

            if (Tx.Version != Version)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "The transaction has been modified.");
            }
        }
    }
}
