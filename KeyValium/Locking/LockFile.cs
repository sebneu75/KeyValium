using System.Security.Cryptography;
using System.Text;

namespace KeyValium.Locking
{
    public unsafe class LockFile : IDisposable
    {
        public const int MAX_WRITERS = 1;
        public const int MAX_READERS = 1022;

        public const int ENTRY_SIZE = 64;
        public const int HEADER_SIZE = 64;

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        public const int TX_TIMEOUT = 600; // 10 Minutes transaction timeout

        /// <summary>
        /// Grace period in seconds (adds to Timeout)
        /// </summary>
        public const int TX_GRACE = 60; // 1 Minute grace period

        #region Constructor

        internal LockFile(Database db)
        {
            Perf.CallCount();

            Database = db;

            Filename = Database.Filename + ".lock";

            Filename_Lock = Filename + ".lock";

            Filesize = HEADER_SIZE + (MAX_WRITERS + MAX_READERS) * ENTRY_SIZE;

            _lockfile = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, Filesize, FileOptions.SequentialScan);

            EnsureLockFile();

            _processid = Process.GetCurrentProcess().Id;
            _machineid = GetMachineId();
        }

        #endregion

        #region Variables

        public readonly Database Database;

        public readonly string Filename;

        public readonly string Filename_Lock;

        public readonly int Filesize;

        private readonly FileStream _lockfile;

        private FileStream _lockfile_lock;

        private object _lock = new object();

        //private static object _synclock = new object();

        //private static object _syncunlock = new object();

        private volatile bool _islocked;

        private readonly byte[] _machineid;

        private readonly int _processid;

        #endregion

        private byte[] GetMachineId()
        {
            Perf.CallCount();

            var md5 = MD5.Create();
            var ret = md5.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));

            KvDebug.Assert(ret.Length == 16, "MachineId length mismatch.");

            return ret;
        }

        /// <summary>
        /// checks the file size and the header of the lockfile
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void EnsureLockFile()
        {
            Perf.CallCount();

            try
            {
                Lock();

                if (_lockfile.Length == 0)
                {
                    // set filesize
                    _lockfile.SetLength(Filesize);

                    // write header
                    WriteHeader();
                }
                else if (_lockfile.Length < Filesize)
                {
                    _lockfile.SetLength(Filesize);
                }

                ValidateHeader();

                // TODO clear expired entries?
            }
            finally
            {
                Unlock();
            }
        }

        private void WriteHeader()
        {
            Perf.CallCount();

            ValidateLock(true);

            _lockfile.Seek(0, SeekOrigin.Begin);

            var header = new byte[HEADER_SIZE];

            fixed (byte* ptr = header)
            {
                var span = new ByteSpan(ptr, HEADER_SIZE);
                span.WriteUInt(0, Limits.Magic);
                span.WriteUShort(0x04, PageTypes.LockFile);
            }

            _lockfile.Write(header, 0, HEADER_SIZE);
            _lockfile.Flush();
        }

        private void ValidateHeader()
        {
            Perf.CallCount();

            ValidateLock(true);

            if (_lockfile.Length < HEADER_SIZE)
            {
                throw new KeyValiumException(0, "Invalid lockfile.");
            }

            var header = new byte[HEADER_SIZE];

            _lockfile.Seek(0, SeekOrigin.Begin);
            _lockfile.Read(header, 0, HEADER_SIZE);

            fixed (byte* ptr = header)
            {
                var span = new ByteSpan(ptr, HEADER_SIZE);

                var magic = span.ReadUInt(0);
                var pagetype = span.ReadUShort(0x04);

                if (magic != Limits.Magic || pagetype != PageTypes.LockFile)
                {
                    throw new KeyValiumException(0, "Invalid lockfile.");
                }
            }
        }

        public void Lock()
        {
            Perf.CallCount();

            Lock(new LockTimeout());
        }

        private void Lock(LockTimeout timeout)
        {
            Perf.CallCount();

            //lock (_synclock)
            //{
            Monitor.Enter(_lock);

            if (_islocked)
            {
                // already locked
                return;
            }

            try
            {
                while (true)
                {
                    try
                    {
                        _lockfile_lock = new FileStream(Filename_Lock, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

                        //_lockfile.Lock(0, Filesize);
                        _islocked = true;
                        Logger.LogInfo(LogTopics.Lock, "Lock taken.");

                        return;
                    }
                    catch (IOException ex)
                    {
                        //
                        // most of the time an IOException is thrown
                        //
                        Logger.LogInfo(LogTopics.Lock, "Waiting for Lock...");
                        timeout.Wait();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        //
                        // in rare cases an UnauthorizedAccessException is thrown
                        //
                        Logger.LogInfo(LogTopics.Lock, "Waiting for Lock...");
                        timeout.Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Exit(_lock);
                throw;
            }
            //}
        }

        public void Unlock()
        {
            Perf.CallCount();

            //lock (_syncunlock)
            //{
            Monitor.Enter(_lock);

            if (!_islocked)
            {
                // already unlocked
                return;
            }

            try
            {
                _lockfile_lock.Dispose();

                //_lockfile.Unlock(0, Filesize);
                _islocked = false;

                Monitor.Exit(_lock);
                Logger.LogInfo(LogTopics.Lock, "Lock released.");
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            //}
        }

        internal KvTid GetMinTid()
        {
            Perf.CallCount();

            ValidateLock(true);

            var mintid = KvTid.MaxValue;

            for (int i = 0; i < MAX_READERS + MAX_WRITERS; i++)
            {
                var entry = ReadLockEntry(i);

                if (entry.Type != 0 && entry.Tid < mintid)
                {
                    mintid = entry.Tid;
                }
            }

            return mintid;
        }

        internal void WaitForWriterSlot()
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                var timeout = new LockTimeout();

                while (true)
                {
                    Lock(timeout);

                    // check if writer slot is empty or expired
                    var entry = ReadLockEntry(0);
                    if (IsFreeOrExpired(entry))
                    {
                        Logger.LogInfo(LogTopics.Lock, "WaitForWriterSlot succeeded.");
                        return;
                    }

                    Unlock();
                    Logger.LogInfo(LogTopics.Lock, "Waiting for WriterSlot...");
                    timeout.Wait();
                }
            }
            catch (Exception)
            {
                Unlock();
                throw;
            }
        }

        private bool IsFreeOrExpired(LockEntry entry)
        {
            Perf.CallCount();

            ValidateLock(true);

            if (entry.Type == 0)
            {
                return true;
            }

            if (DateTime.UtcNow > entry.ExpiresUtc.AddSeconds(TX_GRACE))
            {
                entry.Type = 0;
                WriteLockEntry(entry); // TODO check if necessary
                return true;
            }

            return false;
        }

        private LockEntry ReadLockEntry(int index)
        {
            Perf.CallCount();

            ValidateLock(true);

            _lockfile.Flush();
            _lockfile.Seek(HEADER_SIZE + index * ENTRY_SIZE, SeekOrigin.Begin);
            var bytes = new byte[ENTRY_SIZE];
            _lockfile.Read(bytes, 0, ENTRY_SIZE);

            return new LockEntry(bytes, index);
        }

        private void WriteLockEntry(LockEntry entry)
        {
            Perf.CallCount();

            ValidateLock(true);

            _lockfile.Seek(HEADER_SIZE + entry.Index * ENTRY_SIZE, SeekOrigin.Begin);
            _lockfile.Write(entry._data.Span);
            _lockfile.Flush();

            Logger.LogInfo(LogTopics.Lock, "LockEntry written: {0}", entry);
        }

        internal void AddWriter(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            var entry = ReadLockEntry(0);
            if (entry.Type != 0)
            {
                throw new InvalidOperationException("Writer slot is not free.");
            }

            entry.Type = 0x02;
            _machineid.CopyTo(entry.MachineId);
            entry.ProcessId = _processid;
            entry.Oid = tx.Root.Oid;
            entry.Tid = tx.Tid;
            entry.ExpiresUtc = tx.ExpiresUtc;

            WriteLockEntry(entry);
        }

        private void ValidateLock(bool val)
        {
            Perf.CallCount();

            if (_islocked != val)
            {
                throw new InvalidOperationException("Lock has wrong state.");
            }
        }

        internal void LockAndVerify(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                var timeout = new LockTimeout();

                Lock(timeout);

                var entry = tx.IsReadOnly ? FindReaderEntry(tx) : FindWriterEntry(tx);

                if (entry == null)
                {
                    throw new KeyValiumException(0, "Transaction timed out.");
                }

                if (IsFreeOrExpired(entry))
                {
                    throw new KeyValiumException(0, "Transaction timed out.");
                }
            }
            catch (Exception)
            {
                Unlock();
                throw;
            }
        }

        private LockEntry FindWriterEntry(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            // writer is at slot 0
            var entry = ReadLockEntry(0);
            if (IsMatch(entry, tx))
            {
                return entry;
            }

            return null;
        }

        /// <summary>
        /// Finds a reader slot. if tx is null it returns the first free slot
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        private LockEntry FindReaderEntry(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            // readers start at slot 1
            for (int i = 1; i <= MAX_READERS; i++)
            {
                var entry = ReadLockEntry(i);
                if (tx == null)
                {
                    if (IsFreeOrExpired(entry))
                    {
                        return entry;
                    }
                }
                else
                {
                    if (IsMatch(entry, tx))
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        internal void WaitForReaderSlot()
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                var timeout = new LockTimeout();

                while (true)
                {
                    Lock(timeout);

                    // check if reader slot is empty or expired
                    var entry = FindReaderEntry(null);
                    if (entry != null)
                    {
                        return;
                    }

                    Unlock();
                    timeout.Wait();
                }
            }
            catch (Exception)
            {
                Unlock();
                throw;
            }
        }

        private bool IsMatch(LockEntry entry, Transaction tx)
        {
            Perf.CallCount();

            if (tx.IsReadOnly)
            {
                if (entry.Type != 0x01)
                    return false;
            }
            else
            {
                if (entry.Type != 0x02)
                    return false;
            }

            if (entry.MachineId.SequenceCompareTo(_machineid) != 0)
            {
                return false;
            }

            if (entry.ProcessId != _processid)
            {
                return false;
            }

            if (entry.Oid != tx.Root.Oid)
            {
                return false;
            }

            if (entry.Tid != tx.Tid)
            {
                return false;
            }

            return true;
        }

        internal void AddReader(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            var entry = FindReaderEntry(null);
            if (entry == null)
            {
                throw new InvalidOperationException("Reader slot is not free.");
            }

            entry.Type = 0x01;
            _machineid.CopyTo(entry.MachineId);
            entry.ProcessId = _processid;
            entry.Oid = tx.Root.Oid;
            entry.Tid = tx.Tid;
            entry.ExpiresUtc = tx.ExpiresUtc;

            WriteLockEntry(entry);
        }

        internal void RemoveAndUnlock(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            try
            {
                var entry = tx.IsReadOnly ? FindReaderEntry(tx) : FindWriterEntry(tx);
                if (entry == null)
                {
                    throw new KeyValiumException(0, "LockEntry not found.");
                }

                entry.Type = 0x00;
                this.WriteLockEntry(entry);
            }
            finally
            {
                Unlock();
            }
        }

        #region IDisposable

        private bool _isdisposed;

        protected virtual void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                    _lockfile.Dispose();

                    try
                    {
                        // try to delete the Lockfile
                        File.Delete(Filename);
                    }
                    catch
                    {
                        // ignore errors
                    }
                }

                _isdisposed = true;
            }
        }

        public void Dispose()
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
