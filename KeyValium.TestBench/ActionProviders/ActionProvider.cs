using KeyValium.TestBench.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench.ActionProviders
{
    /// <summary>
    /// provides ActionEntries
    /// </summary>
    internal abstract class ActionProvider
    {
        public ActionProvider(TestDescription td)
        {
            Description = td;
        }

        public readonly TestDescription Description;

        public abstract IEnumerable<ActionEntry> GetActions();
    }
}
