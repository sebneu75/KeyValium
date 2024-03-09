using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    internal class FileLock : ILockable
    {
        public FileLock(Database db, LockFile lockfile)
        {
            _lock = lockfile.LockObject;
            Path = lockfile.Filename + ".lock";

            Timeout = new LockTimeout(db.Options.LockTimeout, db.Options.LockInterval, db.Options.LockIntervalVariance);
        }

        internal readonly string Path;        

        internal FileStream LockFileLock;

        internal readonly object _lock;

        internal volatile bool _islocked;       

        internal readonly LockTimeout Timeout;

        public void Lock()
        {
            Perf.CallCount();

            Timeout.Reset();

            Monitor.Enter(_lock);

            if (_islocked)
            {
                // already locked
                Monitor.Exit(_lock);

                return;
            }

            try
            {
                while (true)
                {
                    try
                    {
                        LockFileLock = new FileStream(Path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

                        //_lockfile.Lock(0, Filesize);
                        _islocked = true;
                        Logger.LogInfo(LogTopics.Lock, "Lock taken.");

                        return;
                    }
                    catch (IOException ex)
                    {
                        //
                        // most of the time an IOException is thrown if the file already exists
                        //                        
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        //
                        // in rare cases an UnauthorizedAccessException is thrown if the file already exists
                        //
                    }

                    Logger.LogInfo(LogTopics.Lock, "Waiting for Lock...");
                    Timeout.Wait();
                }
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

                try
                {
                    LockFileLock.Dispose();                    
                }
                finally 
                {
                    _islocked = false;
                    Monitor.Exit(_lock);
                }                
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
            LockFileLock?.Dispose();
            LockFileLock = null;
        }
    }
}
