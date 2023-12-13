using KeyValium.TestBench;
using KeyValium;
using KeyValium.TestBench.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace KeyValium.TestBench.ActionProviders
{
    /// <summary>
    /// provides ActionEntries from a logfile
    /// </summary>
    internal class LogActionProvider : ActionProvider
    {
        public LogActionProvider(TestDescription td, string actionlog, ulong tid) : base(td)
        {
            ActionLog = actionlog;
            BackupTid = tid;
        }

        public string ActionLog
        {
            get;
            private set;
        }

        public string TdPath
        {
            get;
            private set;
        }

        public string OldDbPath
        {
            get;
            private set;
        }

        public ulong BackupTid
        {
            get;
            private set;
        }

        /// <summary>
        /// depth level of child transactions
        /// </summary>
        private int _txchilds = 0;

        /// <summary>
        /// depth level of child transactions
        /// </summary>
        private int _tx = 0;

        public override IEnumerable<ActionEntry> GetActions()
        {
            Tools.WriteColor(ConsoleColor.DarkYellow, string.Format("Replaying actions from {0} ...", ActionLog), null);

            using (var reader = new StreamReader(ActionLog, Encoding.UTF8))
            {
                var lastpc = 0;

                if (BackupTid != 0)
                {
                    Tools.WriteColor(ConsoleColor.DarkYellow, string.Format("Fast forwarding to Tid {0} ...", BackupTid), null);
                    while (!reader.EndOfStream)
                    {
                        var action = ActionEntry.Parse(reader.ReadLine());
                        if (action.Type == ActionType.CommitTx && action.Key.Path[0] == (long)BackupTid)
                        {
                            break;
                        }
                    }
                }

                while (!reader.EndOfStream)
                {
                    var pc = reader.BaseStream.Position / (double)reader.BaseStream.Length * 100.0;

                    if ((int)pc > lastpc)
                    {
                        Console.WriteLine("[REPLAY]: {0:#.0#}%", pc);
                        lastpc = (int)pc;
                    }

                    var action = ActionEntry.Parse(reader.ReadLine());

                    if (action.Type == ActionType.None)
                    {
                        continue;
                    }

                    if (action.Type != ActionType.ERROR)
                    {
                        Console.WriteLine("[REPLAY]: {0}", action.Line);
                    }

                    switch (action.Type)
                    {
                        case ActionType.BeginTx:
                            _tx++;
                            break;

                        case ActionType.CommitTx:
                        case ActionType.RollbackTx:
                            _tx--;
                            break;

                        case ActionType.BeginChildTx:
                            _txchilds++;
                            break;

                        case ActionType.CommitChildTx:
                        case ActionType.RollbackChildTx:
                            _txchilds--;
                            break;
                    }

                    if (action.Type == ActionType.ERROR)
                    {
                        break;
                    }

                    yield return action;
                }
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
            // commit open transactions
            //
            while (_tx > 0)
            {
                _tx--;
                yield return new ActionEntry() { Type = ActionType.CommitTx };
            }
        }
    }
}
