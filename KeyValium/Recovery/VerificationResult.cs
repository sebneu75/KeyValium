using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Recovery
{
    internal class VerificationResult
    {
        internal VerificationResult()
        {
            Errors = new List<string>();
        }

        public void ThrowOnError()
        {
            if (Errors.Count > 0)
            {
                var msg = string.Join("\n", Errors);
                throw new KeyValiumException(ErrorCodes.InternalError, msg);
            }
        }

        public List<string> Errors
        {
            get;
            private set;
        }

        internal void AddError(string msg)
        {
            Errors.Add(msg);
        }
    }
}
