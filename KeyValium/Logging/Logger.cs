using System.Text;

namespace KeyValium.Logging
{
    internal class Logger
    {
        static Logger()
        {
            // create default instance
            _instance = new NullLogger();
        }

        internal static LogTopics Topics
        {
            get
            {
                return _instance.Topics;
            }
            set
            {
                _instance.Topics = value;
            }
        }

        [Conditional("DEBUG")]
        internal static void CreateInstance(string path, LogLevel level, LogTopics topics)
        {
            _instance = new FileLogger(path, level);
            _instance.Topics = topics;
        }

        private static ILogger _instance;

        #region ILogger implementation

        [Conditional("DEBUG")]
        public static void LogDebug(LogTopics topics, string format, params object[] args)
        {
            _instance.LogDebug(topics, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            _instance.LogDebug(topics, tid, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(LogTopics topics, string format, params object[] args)
        {
            _instance.LogInfo(topics, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            _instance.LogInfo(topics, tid, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogWarn(LogTopics topics, string format, params object[] args)
        {
            _instance.LogWarn(topics, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogWarn(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            _instance.LogWarn(topics, tid, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, string format, params object[] args)
        {
            _instance.LogError(topics, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, Exception ex, string format, params object[] args)
        {
            _instance.LogError(topics, ex, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, Exception ex)
        {
            _instance.LogError(topics, ex);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            _instance.LogError(topics, tid, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
            _instance.LogError(topics, tid, ex, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogError(LogTopics topics, KvTid tid, Exception ex)
        {
            _instance.LogError(topics, tid, ex);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, string format, params object[] args)
        {
            _instance.LogFatal(topics, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, Exception ex, string format, params object[] args)
        {
            _instance.LogFatal(topics, ex, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, Exception ex)
        {
            _instance.LogFatal(topics, ex);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, KvTid tid, string format, params object[] args)
        {
            _instance.LogFatal(topics, tid, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
            _instance.LogFatal(topics, tid, ex, format, args);
        }

        [Conditional("DEBUG")]
        public static void LogFatal(LogTopics topics, KvTid tid, Exception ex)
        {
            _instance.LogFatal(topics, tid, ex);
        }

        #endregion

        #region Utils

        public static string ToHex(byte[] val)
        {
            if (val == null)
                return "<null>";

            var sb = new StringBuilder(val.Length * 2);

            for (int i = 0; i < val.Length; i++)
            {
                sb.AppendFormat("{0:x2}", val[i]);
            }

            return sb.ToString();
        }

        #endregion
    }
}
