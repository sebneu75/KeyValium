namespace KeyValium.Logging
{
    internal interface ILogger
    {
        LogTopics Topics
        {
            get;
            set;
        }

        void LogDebug(LogTopics topics, string format, params object[] args);

        void LogDebug(LogTopics topics, KvTid tid, string format, params object[] args);

        void LogInfo(LogTopics topics, string format, params object[] args);

        void LogInfo(LogTopics topics, KvTid tid, string format, params object[] args);

        void LogWarn(LogTopics topics, string format, params object[] args);

        void LogWarn(LogTopics topics, KvTid tid, string format, params object[] args);

        void LogError(LogTopics topics, string format, params object[] args);

        void LogError(LogTopics topics, Exception ex, string format, params object[] args);

        void LogError(LogTopics topics, Exception ex);

        void LogError(LogTopics topics, KvTid tid, string format, params object[] args);

        void LogError(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args);

        void LogError(LogTopics topics, KvTid tid, Exception ex);

        void LogFatal(LogTopics topics, string format, params object[] args);

        void LogFatal(LogTopics topics, Exception ex, string format, params object[] args);

        void LogFatal(LogTopics topics, Exception ex);

        void LogFatal(LogTopics topics, KvTid tid, string format, params object[] args);

        void LogFatal(LogTopics topics, KvTid tid, Exception ex, string format, params object[] args);

        void LogFatal(LogTopics topics, KvTid tid, Exception ex);
    }
}
