using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Performance
{
    internal class CallInfo
    {
        public CallInfo()
        {
        }

        public long Count;

        public Dictionary<string, long> Callers = new Dictionary<string, long>();

        internal void AddCall(string caller)
        {
            if (!Callers.ContainsKey(caller))
            {
                Callers.Add(caller, 0);
            }

            Callers[caller]++;
        }
    }
}
