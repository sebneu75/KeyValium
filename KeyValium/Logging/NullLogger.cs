namespace KeyValium.Logging
{
    internal class NullLogger : ILogger
    {
        public LogTopics Topics
        {
            get;
            set;
        }

        public void LogDebug(LogTopics topics, string format, params object[] args)
        {
        }

        public void LogDebug(LogTopics topics, KvTid tid, string format, params object[] args)
        {
        }

        public void LogError(LogTopics topics, string format, params object[] args)
        {
        }

        public void LogError(LogTopics topics, Exception ex, string format, params object[] args)
        {
        }

        public void LogError(LogTopics topics, Exception ex)
        {
        }

        public void LogError(LogTopics topics, KvTid tid, string format, params object[] args)
        {
        }

        public void LogError(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
        }

        public void LogError(LogTopics topics, KvTid tid, Exception ex)
        {
        }

        public void LogFatal(LogTopics topics, string format, params object[] args)
        {
        }

        public void LogFatal(LogTopics topics, Exception ex, string format, params object[] args)
        {
        }

        public void LogFatal(LogTopics topics, Exception ex)
        {
        }

        public void LogFatal(LogTopics topics, KvTid tid, string format, params object[] args)
        {
        }

        public void LogFatal(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args)
        {
        }

        public void LogFatal(LogTopics topics, KvTid tid, Exception ex)
        {
        }

        public void LogInfo(LogTopics topics, string format, params object[] args)
        {
        }

        public void LogInfo(LogTopics topics, KvTid tid, string format, params object[] args)
        {
        }

        public void LogWarn(LogTopics topics, string format, params object[] args)
        {
        }

        public void LogWarn(LogTopics topics, KvTid tid, string format, params object[] args)
        {
        }
    }
}
