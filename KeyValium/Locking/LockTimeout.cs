using System.Runtime.InteropServices;

namespace KeyValium.Locking
{
    [StructLayout(LayoutKind.Auto)]
    internal struct LockTimeout
    {
        static readonly Random Random = new Random();

        internal LockTimeout(int timeout, int interval, int variance)
        {
            Perf.CallCount();

            _current = 0;
            _timeout = timeout;
            _interval = interval + Random.Next(variance);
        }

        private int _current;

        private readonly int _timeout;

        private readonly int _interval;

        public void Wait()
        {
            Perf.CallCount();

            if (_current >= _timeout)
            {
                throw new TimeoutException("Could not aquire lock within timeout.");
            }

            Thread.Sleep(_interval);
            _current += _interval;
        }

        public void Reset()
        {
            Perf.CallCount();

            _current = 0;
        }
    }
}
