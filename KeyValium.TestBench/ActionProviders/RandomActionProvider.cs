using KeyValium.TestBench;
using KeyValium;
using KeyValium.TestBench.Helpers;
using LightningDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace KeyValium.TestBench.ActionProviders
{
    /// <summary>
    /// provides random ActionEntries
    /// </summary>
    internal class RandomActionprovider : ActionProvider
    {
        public RandomActionprovider(TestDescription td)
            : base(td)
        {
            Rnd = new Random();
        }

        private readonly Random Rnd;

        /// <summary>
        /// depth level of child transactions
        /// </summary>
        private int _txchilds = 0;

        public override IEnumerable<ActionEntry> GetActions()
        {
            //Tools.WriteColor(ConsoleColor.DarkYellow, string.Format("Generating random actions for {0} ...", Description.DbFilename), null);

            for (int txcount = 0; txcount < Description.TxCount; txcount++)
            {
                //
                // always start with BeginTx
                //
                yield return new ActionEntry() { Type = ActionType.BeginTx };

                var count = Rnd.Next(Description.CommitSize) + 1;

                //
                // generate <count> actions
                //
                for (int i = 0; i < count; i++)
                {
                    var keypath = GetRandomKeyPath();
                    var actiontype = GetNextAction();

                    if (actiontype == ActionType.BeginChildTx)
                    {
                        _txchilds++;
                    }
                    else if (actiontype == ActionType.CommitChildTx || actiontype == ActionType.RollbackChildTx)
                    {
                        _txchilds--;
                    }

                    yield return new ActionEntry() { Type = actiontype, Key = keypath };
                }

                //
                // commit open child transactions
                //
                while (_txchilds > 0)
                {
                    _txchilds--;
                    yield return new ActionEntry() { Type = ActionType.CommitChildTx };
                }

                //
                // always end with CommitTx
                //
                yield return new ActionEntry() { Type = ActionType.CommitTx };
            }
        }

        /// <summary>
        /// returns a random action type
        /// </summary>
        /// <returns></returns>
        private ActionType GetNextAction()
        {
            // probability of child transactions
            if (Rnd.NextDouble() < 0.02)
            {
                if (Rnd.NextDouble() < 0.5)
                {
                    return ActionType.BeginChildTx;
                }
                else if (_txchilds > 0)
                {
                    if (Rnd.NextDouble() < 0.5)
                    {
                        return ActionType.CommitChildTx;
                    }
                    else
                    {
                        return ActionType.RollbackChildTx;
                    }
                }
            }

            var action = Rnd.Next(11);

            switch (action)
            {
                case 0:
                    return ActionType.Delete;
                case 1:
                    return ActionType.Exists;
                case 2:
                    return ActionType.Get;
                case 3:
                    //return ActionType.GetNext;
                    break;
                case 4:
                    //return ActionType.GetPrevious;
                    break;
                case 5:
                    return ActionType.Insert;
                case 6:
                    return ActionType.Update;
                case 7:
                    return ActionType.Upsert;
                case 8:
                    return ActionType.CreateCursor;
                case 9:
                    return ActionType.IterateForward;
                case 10:
                    return ActionType.IterateBackward;
            }

            return ActionType.Get;
        }

        private PathToKey GetRandomKeyPath()
        {
            var len = Rnd.Next(3) + 1;
            var vals = new List<long>();

            for (int i = 0; i < len; i++)
            {
                vals.Add(Rnd.NextInt64(Description.KeyCount));
            }

            return new PathToKey(vals);
        }
    }
}
