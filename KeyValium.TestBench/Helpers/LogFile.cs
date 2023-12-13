using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeyValium.TestBench.Helpers
{
    public class LogFile
    {
        public LogFile(string fullpath)
        {
            FullPath = fullpath;
        }

        public string FullPath
        {
            get;
            private set;
        }

        internal void LogAction(ActionType action, ulong? txid, PathToKey? key, KVEntry entry)
        {
            using (var writer = new StreamWriter(FullPath, true, Encoding.UTF8))
            {
                var msg = string.Format("{0}: {1} : {2} : {3} : {4}", action, key != null ? key : txid, entry?.KeyLength, entry?.ValueSeed, entry?.ValueLength);
                writer.WriteLine(msg);
            }
        }

        internal void Log(string what, string msg)
        {
            using (var writer = new StreamWriter(FullPath, true, Encoding.UTF8))
            {
                var msg2 = string.Format("# {0}: {1}", what, msg);
                writer.WriteLine(msg2);
            }
        }

        internal void LogError(Exception ex)
        {
            using (var writer = new StreamWriter(FullPath, true, Encoding.UTF8))
            {
                var msg = string.Format("ERROR: {0}", ex.Message);
                writer.WriteLine(msg);
                writer.WriteLine(ex.StackTrace);
            }
        }
    }
}
