using KeyValium.TestBench.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestShared
{
    internal class TxLogger
    {
        public TxLogger(SharedTestInfo ti)
        {
            TestInfo = ti;
            _logfile = TestInfo.WriteTxLog;
            _machinename = Environment.MachineName;
        }

        private readonly SharedTestInfo TestInfo;
        private string _logfile;
        private string _machinename;

        public void LogTxStart(Transaction tx)
        {
            var msg = string.Format("Begin WriteTransaction {0}", tx.Tid);
            LogWriter(msg);
        }

        internal void LogReadTxStart(Transaction tx, string threadname)
        {
            var msg = string.Format("Begin ReadTransaction {0}", tx.Tid);
            LogReader(msg, threadname);
        }

        public void LogTxEnd(Transaction tx)
        {
            var msg = string.Format("End WriteTransaction {0}", tx.Tid);
            LogWriter(msg);
        }

        internal void LogReadTxEnd(Transaction tx, string threadname)
        {
            var msg = string.Format("End ReadTransaction {0}", tx.Tid);
            LogReader(msg, threadname);
        }

        internal void LogGet(string key, string val, string threadname)
        {
            var msg = string.Format("Get {0} = {1}", key, val);
            LogReader(msg, threadname);
        }

        public void LogUpsert(string key, string val)
        {
            var msg = string.Format("Upsert {0} {1}", key, val);
            LogWriter(msg);
        }

        private static object _lock = new object();

        public void LogWriter(string msg)
        {
            var text = string.Format("{0:yyyy-MM_dd-HH:mm:ss} {1} {2}", DateTime.Now, _machinename, msg);

            lock (_lock)
            {
                using (var writer = new StreamWriter(_logfile, true))
                {
                    writer.WriteLine(text);
                }
            }

            Console.WriteLine(text);
        }

        public void LogReader(string msg, string threadname)
        {
            var text = string.Format("{0:yyyy-MM_dd-HH:mm:ss} {1} {2}", DateTime.Now, _machinename, msg);

            var file = TestInfo.GetReadTxLog(threadname);

            lock (_lock)
            {
                using (var writer = new StreamWriter(file, true))
                {
                    writer.WriteLine(text);
                }
            }

            Console.WriteLine(text);
        }

    }
}
