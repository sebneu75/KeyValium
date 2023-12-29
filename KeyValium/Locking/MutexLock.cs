using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    internal class MutexLock : ILockable
    {
        internal MutexLock(Database db, LockFile lockfile)
        {
            _lock = lockfile.LockObject;

            MutexName = "KeyValium-" + lockfile.Filename + ".lock";
            MutexName = MutexName.Replace("\\", "_");
            MutexName = MutexName.Replace("/", "_");
            MutexName = MutexName.Replace(":", "_");
            MutexName = MutexName.ToLowerInvariant();

            Mutex = new Mutex(false, MutexName);

            Timeout = db.Options.LockTimeout;
        }

        internal readonly string MutexName;

        internal Mutex Mutex;

        internal readonly object _lock;

        internal volatile bool _islocked;

        internal readonly int Timeout;

        public void Lock()
        {
            Perf.CallCount();

            Monitor.Enter(_lock);

            if (_islocked)
            {
                // already locked
                return;
            }

            try
            {
                if (!Mutex.WaitOne(Timeout))
                {
                    throw new TimeoutException("Could not aquire lock within timeout.");
                }

                _islocked = true;
            }
            catch (Exception ex)
            {
                Monitor.Exit(_lock);
                throw;
            }
        }

        public void Unlock()
        {
            Perf.CallCount();

            try
            {
                Monitor.Enter(_lock);

                if (!_islocked)
                {
                    // already unlocked
                    return;
                }

                Mutex.ReleaseMutex();

                _islocked = false;
            }
            finally
            {
                Monitor.Exit(_lock);
                Logger.LogInfo(LogTopics.Lock, "Lock released.");
            }
        }

        public void ValidateLock(bool expected)
        {
            Perf.CallCount();

            if (_islocked != expected)
            {
                throw new InvalidOperationException("Lock has wrong state.");
            }
        }

        public void Dispose()
        {
            Mutex?.Dispose();
            Mutex = null;
        }
    }
}
