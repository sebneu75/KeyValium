namespace KeyValium.Locking
{
    internal sealed class LockTimeout
    {
        public const int LOCK_TIMEOUT = 60000;
        public const int LOCK_INTERVAL = 50;

        public LockTimeout()
        {
            Perf.CallCount();
        }

        private int _current;

        //public bool IsTimedOut
        //{
        //    get
        //    {
        //        return _current >= LOCK_TIMEOUT;
        //    }
        //}

        public void Wait()
        {
            Perf.CallCount();

            if (_current >= LOCK_TIMEOUT)
            {
                throw new TimeoutException("Could not aquire lock within timeout.");
            }

            Thread.Sleep(LOCK_INTERVAL);
            _current += LOCK_INTERVAL;
        }
    }
}
