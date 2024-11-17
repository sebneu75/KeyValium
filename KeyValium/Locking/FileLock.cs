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
        #region Constructor

        internal FileLock(Database db, LockFile lockfile)
        {
            Path = lockfile.Filename + ".lock";

            LockTimeout = db.Options.LockTimeout;
            LockInterval = db.Options.LockInterval;
            LockVariance = db.Options.LockIntervalVariance;
        }

        #endregion

        #region Variables

        internal FileStream LockFileLock;

        internal int LockTimeout;
        internal int LockInterval;
        internal int LockVariance;

        internal readonly string Path;

        internal readonly object _lock = new object();

        #endregion

        #region ILockable implementation

        public void Lock()
        {
            Perf.CallCount();

            ValidateLock(false);

            Monitor.Enter(_lock);
            Logger.LogInfo(LogTopics.Lock, "Monitor entered. (lock)");

            try
            {
                if (LockFileLock != null)
                {
                    throw new InvalidOperationException("LockFileLock already exists.");
                }

                // create LockTimeout per caller
                var timeout = new LockTimeout(LockTimeout, LockInterval, LockVariance);

                while (true)
                {
                    try
                    {
                        LockFileLock = new FileStream(Path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 0, FileOptions.DeleteOnClose);

                        Logger.LogInfo(LogTopics.Lock, "Lock taken.");

                        return;
                    }
                    catch (IOException ex)
                    {
                        //
                        // most of the time an IOException is thrown if the file already exists
                        //

                        if ((uint)ex.HResult == 0x80070050)
                        {
                            // expected error
                            // The file '...' already exists.
                        }
                        //else if (ex is FileNotFoundException && (uint)ex.HResult == 80070002)
                        //{
                        //    // expected error
                        //    // for some strange reason in rare cases a FileNotFoundException is thrown 
                        //    // Could not find file '...'.
                        //}
                        else
                        {
                            Logger.LogError(LogTopics.Lock, ex, "Creation of LockFileLock failed. " + ex.HResult.ToString("X8"));
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        //
                        // in rare cases an UnauthorizedAccessException is thrown if the file already exists
                        //
                        Logger.LogError(LogTopics.Lock, ex, "Creation of LockFileLock failed. " + ex.HResult.ToString("X8"));
                    }

                    Logger.LogInfo(LogTopics.Lock, "Waiting for Lock...");
                    timeout.Wait();
                }
            }
            catch (TimeoutException ex)
            {
                LockFileLock?.Dispose();
                LockFileLock = null;

                Monitor.Exit(_lock);
                Logger.LogInfo(LogTopics.Lock, "Monitor exited (lock timeout).");

                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Lock, ex, "Error while locking.");

                LockFileLock?.Dispose();
                LockFileLock = null;

                Monitor.Exit(_lock);
                Logger.LogInfo(LogTopics.Lock, "Monitor exited (lock error).");

                throw;
            }
        }

        public void Unlock()
        {
            Perf.CallCount();

            ValidateLock(true);

            try
            {
                Monitor.Enter(_lock);
                Logger.LogInfo(LogTopics.Lock, "Monitor entered (unlock).");

                try
                {
                    LockFileLock.Dispose();
                    LockFileLock = null;
                }
                finally
                {
                    //IsLocked = false;
                    Monitor.Exit(_lock);
                    Logger.LogInfo(LogTopics.Lock, "Monitor exited. (unlock1)");
                }
            }
            finally
            {
                Monitor.Exit(_lock);
                Logger.LogInfo(LogTopics.Lock, "Monitor exited (unlock2).");
                Logger.LogInfo(LogTopics.Lock, "Lock released.");
            }
        }

        public void ValidateLock(bool expected)
        {
            Perf.CallCount();

            var msg = "";

            var islocked = Monitor.IsEntered(_lock);
            if (islocked != expected)
            {
                msg += string.Format("Lock has wrong state. Actual={0} Expected={1}.", islocked, expected);
            }

            if (islocked)
            {
                // if locked then filehandle must be nonzero
                if (LockFileLock == null)
                {
                    msg += string.Format(" LockFileLock has wrong state. Actual={0} Expected={1}.", LockFileLock != null, expected);
                }
            }

            if (msg != "")
            {
                throw new InvalidOperationException(msg.Trim());
            }
        }

        public void LockForCreation()
        {
            Lock();
        }

        public void UnlockForCreation()
        {
            Unlock();
        }

        public void ValidateCreationLock(bool expected)
        {
            ValidateLock(expected);
        }

        public void CreateLock(Guid guid)
        {
            // must be empty for FileLock
            if (guid != Guid.Empty)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "LockGuid is not empty.");
            }

            // do nothing
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            LockFileLock?.Dispose();
            LockFileLock = null;
        }

        #endregion
    }
}
