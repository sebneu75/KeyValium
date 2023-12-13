using System.Text;

namespace KeyValium.Logging
{
    internal class FileLogger : ILogger
    {
        public FileLogger(string path, LogLevel level)
        {
            Logfile = path;
            Level = level;

            _procid = Process.GetCurrentProcess().Id;

            Topics = LogTopics.All;
        }

        public LogTopics Topics
        {
            get;
            set;
        }

        private int _procid;

        private static object _lock = new object();

        public string Logfile
        {
            get;
            private set;
        }

        public LogLevel Level
        {
            get;
            private set;
        }

        private void Log(KvTid? tid, LogLevel level, LogTopics topic, Exception ex, string format, params object[] args)
        {
            if (level < Level)
                return;

            if ((Topics & topic) == 0)
            {
                return;
            }

            var threadid = Thread.CurrentThread.ManagedThreadId;

            if (format == null)
            {
                format = ex?.Message;
            }

            var msg1 = string.Format(format, args);
            var msg2 = string.Format("{0:yyyy-MM-dd_HH:mm:ss.ffffff} {1}.{2} Tx{3} {4} [{5}] {6}", DateTime.Now, _procid, threadid, tid.HasValue ? tid.Value : "-", level, topic, msg1);

            string exmsg = null;

            if (ex != null)
            {
                exmsg = ex.ToString();
                exmsg = "    " + exmsg.Trim().Replace("\n", "\n    ");
            }

            lock (_lock)
            {
                using (var writer = new StreamWriter(Logfile, true, Encoding.UTF8))
                {
                    writer.WriteLine(msg2);
                    if (exmsg != null)
                    {
                        writer.WriteLine(exmsg);
                    }
                }
            }
        }

        #region ILogger implementation

        public void LogDebug(LogTopics topics, string format, params object[] args)
        {
            Log(null, LogLevel.Debug, topics, null, format, args);
        }

        public void LogDebug(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            Log(tid, LogLevel.Debug, topics, null, format, args);
        }

        public void LogError(LogTopics topics, string format, params object[] args)
        {
            Log(null, LogLevel.Error, topics, null, format, args);
        }

        public void LogError(LogTopics topics, Exception ex, string format, params object[] args)
        {
            Log(null, LogLevel.Error, topics, ex, format, args);
        }

        public void LogError(LogTopics topics, Exception ex)
        {
            Log(null, LogLevel.Error, topics, ex, null);
        }

        public void LogError(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            Log(tid, LogLevel.Error, topics, null, format, args);
        }

        public void LogError(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
            Log(tid, LogLevel.Error, topics, ex, format, args);
        }

        public void LogError(LogTopics topics, KvTid tid, Exception ex)
        {
            Log(tid, LogLevel.Error, topics, ex, null);
        }

        public void LogFatal(LogTopics topics, string format, params object[] args)
        {
            Log(null, LogLevel.Fatal, topics, null, format, args);
        }

        public void LogFatal(LogTopics topics, Exception ex, string format, params object[] args)
        {
            Log(null, LogLevel.Fatal, topics, ex, format, args);
        }

        public void LogFatal(LogTopics topics, Exception ex)
        {
            Log(null, LogLevel.Fatal, topics, ex, null);
        }

        public void LogFatal(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            Log(tid, LogLevel.Fatal, topics, null, format, args);
        }

        public void LogFatal(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
            Log(tid, LogLevel.Fatal, topics, ex, format, args);
        }

        public void LogFatal(LogTopics topics, KvTid tid, Exception ex)
        {
            Log(tid, LogLevel.Fatal, topics, ex, null);
        }

        public void LogInfo(LogTopics topics, string format, params object[] args)
        {
            Log(null, LogLevel.Info, topics, null, format, args);
        }

        public void LogInfo(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            Log(tid, LogLevel.Info, topics, null, format, args);
        }

        public void LogWarn(LogTopics topics, string format, params object[] args)
        {
            Log(null, LogLevel.Warn, topics, null, format, args);
        }

        public void LogWarn(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            Log(tid, LogLevel.Warn, topics, null, format, args);
        }

        #endregion
    }
}
