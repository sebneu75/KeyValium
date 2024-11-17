using KeyValium.Collections;
using KeyValium.Options;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace KeyValium.Locking
{
    internal unsafe class LockFile : IDisposable
    {
        const int WRITERSLOT = 0;
        const int MAX_WRITERS = 1;

        const int READERSLOT = 1;
        const int MAX_READERS = 62;

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        internal const int TX_TIMEOUT = 600; // 10 Minutes transaction timeout

        /// <summary>
        /// Grace period in seconds (adds to Timeout)
        /// </summary>
        const int TX_GRACE = 60; // 1 Minute grace period

        #region Constructor

        internal LockFile(Database db)
        {
            Perf.CallCount();

            Database = db;

            SharingMode = db.Options.InternalSharingMode;

            Filename = db.Filename + ".lock";

            if (db.Options.InternalSharingMode == InternalSharingModes.SharedLocal)
            {
                Lockable = new MutexLock(db);
                FileOptions = FileOptions.None; // FileOptions.WriteThrough | Limits.FileFlagNoBuffering;
            }
            else if (db.Options.InternalSharingMode == InternalSharingModes.SharedNetwork)
            {
                Lockable = new FileLock(db, this);

                // necessary for files on network drives
                FileOptions = FileOptions.WriteThrough | Limits.FileFlagNoBuffering;
            }
            else
            {
                throw new KeyValiumException(ErrorCodes.InvalidParameter, "Unsupported sharing mode.");
            }

            using (var proc = Process.GetCurrentProcess())
            {
                _processid = proc.Id;
            }

            _machineid = GetMachineId();
            _machinename = GetMachineName();

            Filesize = LockHeader.HEADER_SIZE + (MAX_WRITERS + MAX_READERS) * LockEntry.ENTRY_SIZE;

            if (Filesize != 4096)
            {
                throw new KeyValiumException(ErrorCodes.InvalidParameter, "Lockfile size must be 4096.");
            }

            FileBuffer = new byte[Filesize];

            EnsureLockFile();
        }

        #endregion

        #region Variables

        internal readonly Database Database;

        internal readonly string Filename;

        internal readonly int Filesize;

        internal readonly InternalSharingModes SharingMode;

        internal readonly FileOptions FileOptions;

        private FileStream _lockfile;

        private readonly byte[] _machineid;

        private readonly byte[] _machinename;

        private readonly int _processid;

        internal readonly ILockable Lockable;

        private byte[] FileBuffer;

        private bool IsFileBufferValid;

        #endregion

        #region Lockfile creation

        /// <summary>
        /// creates and/or verifies the lockfile 
        /// This method always uses the FileLock.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void EnsureLockFile()
        {
            Perf.CallCount();

            try
            {
                // always use filelock
                LockForCreation();

                _lockfile = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, Filesize, FileOptions);

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
                UnlockForCreation();
            }
        }

        #endregion

        #region Reading and Writing the Lockfile 

        private void WriteHeader()
        {
            Perf.CallCount();

            var header = new LockHeader(FileBuffer.AsSpan().Slice(0, LockHeader.HEADER_SIZE));

            header.Magic = Limits.Magic;
            header.PageType = PageTypes.LockFile;
            header.SharingMode = SharingMode;
            header.MachineId = _machineid;
            header.MachineName = _machinename;
            header.LockGuid = GetLockGuid();

            // declare FileBuffer valid
            IsFileBufferValid = true;

            WriteLockfile(true);
        }

        private void ValidateHeader()
        {
            Perf.CallCount();

            ReadLockfile(true);

            var header = new LockHeader(FileBuffer.AsSpan().Slice(0, LockHeader.HEADER_SIZE));

            if (header.Magic != Limits.Magic)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (Magic mismatch)");
            }

            if (header.PageType != PageTypes.LockFile)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (Pagetype mismatch)");
            }

            if (header.SharingMode != SharingMode)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (SharingMode mismatch)");
            }

            if (SharingMode == InternalSharingModes.SharedLocal)
            {
                if (!header.MachineId.SequenceEqual(_machineid))
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (MachineId mismatch)");
                }

                if (header.LockGuid == Guid.Empty)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (LockGuid is empty)");
                }
            }

            // create secondary objects
            Lockable.CreateLock(header.LockGuid);
        }

        private void ReadLockfile(bool iscreation)
        {
            Perf.CallCount();

            if (iscreation)
            {
                ValidateCreationLock(true);
            }
            else
            {
                ValidateLock(true);
            }

            if (!IsFileBufferValid)
            {
                _lockfile.Flush();
                _lockfile.Seek(0, SeekOrigin.Begin);
                var bytesread = _lockfile.Read(FileBuffer);

                if (bytesread != Filesize)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Lockfile size mismatch.");
                }

                IsFileBufferValid = true;
            }
        }

        private void WriteLockfile(bool iscreation)
        {
            Perf.CallCount();

            if (iscreation)
            {
                ValidateCreationLock(true);
            }
            else
            {
                ValidateLock(true);
            }

            if (!IsFileBufferValid)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "LockFile.FileBuffer not valid.");
            }

            _lockfile.Seek(0, SeekOrigin.Begin);
            _lockfile.Write(FileBuffer);
            _lockfile.Flush(true);
        }

        #endregion

        #region Locking Wrappers

        private void Lock()
        {
            ValidateLock(false);

            Lockable.Lock();

            // declare buffer invalid after getting the lock
            IsFileBufferValid = false;

            ValidateLock(true);
        }

        internal void Unlock()
        {
            ValidateLock(true);

            Lockable.Unlock();

            ValidateLock(false);
        }

        private void ValidateLock(bool expected)
        {
            Lockable.ValidateLock(expected);
        }

        private void LockForCreation()
        {
            ValidateCreationLock(false);

            Lockable.LockForCreation();

            // declare buffer invalid after getting the lock
            IsFileBufferValid = false;

            ValidateCreationLock(true);
        }

        internal void UnlockForCreation()
        {
            ValidateCreationLock(true);

            Lockable.UnlockForCreation();

            ValidateCreationLock(false);
        }

        private void ValidateCreationLock(bool expected)
        {
            Lockable.ValidateCreationLock(expected);
        }

        #endregion

        #region Helpers

        private Guid GetLockGuid()
        {
            switch (SharingMode)
            {
                case InternalSharingModes.SharedLocal:
                    return Guid.NewGuid();
            }

            return Guid.Empty;
        }

        private byte[] GetMachineId()
        {
            Perf.CallCount();

            var md5 = MD5.Create();
            var ret = md5.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));

            if (ret.Length != 16)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "MachineId length mismatch.");
            }

            return ret;
        }

        private byte[] GetMachineName()
        {
            Perf.CallCount();

            var ret = new byte[16];

            var val = Environment.MachineName ?? "";
            var buf = Encoding.UTF8.GetBytes(val);

            buf.AsSpan().Slice(0, Math.Min(buf.Length, 16)).CopyTo(ret.AsSpan());

            return ret;
        }

        #endregion

        internal KvTid GetMinTid()
        {
            Perf.CallCount();

            ValidateLock(true);

            var mintid = KvTid.MaxValue;

            for (int i = 0; i < MAX_READERS + MAX_WRITERS; i++)
            {
                var entry = GetLockEntry(i);

                if (entry.Type != LockEntryTypes.Free && entry.Tid < mintid)
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
                while (true)
                {
                    Lock();

                    // check if writer slot is empty or expired
                    var entry = GetLockEntry(WRITERSLOT);
                    if (IsFreeOrExpired(entry))
                    {
                        Logger.LogInfo(LogTopics.Lock, "WaitForWriterSlot succeeded.");
                        return;
                    }

                    Unlock();

                    Logger.LogInfo(LogTopics.Lock, "Waiting for WriterSlot...");

                    // wait if no writer found
                    Thread.Sleep(Database.Options.LockInterval);
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

            if (entry.Type == LockEntryTypes.Free)
            {
                return true;
            }

            if (DateTime.UtcNow > entry.ExpiresUtc.AddSeconds(TX_GRACE))
            {
                entry.Clear();
                WriteLockEntry(entry); // update Lockfile

                return true;
            }

            return false;
        }

        private LockEntry GetLockEntry(int index)
        {
            Perf.CallCount();

            ReadLockfile(false);

            var span = FileBuffer.AsSpan().Slice(LockHeader.HEADER_SIZE + index * LockEntry.ENTRY_SIZE, LockEntry.ENTRY_SIZE);

            return new LockEntry(span, index);
        }

        private void WriteLockEntry(LockEntry entry)
        {
            Perf.CallCount();

            WriteLockfile(false);

            Logger.LogInfo(LogTopics.Lock, "LockEntry written: {0}", entry.ToString());
        }

        internal void AddWriter(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            var entry = GetLockEntry(WRITERSLOT);

            if (entry.Type != LockEntryTypes.Free)
            {
                throw new InvalidOperationException("Writer slot is not free.");
            }

            entry.Type = LockEntryTypes.Writer;
            entry.MachineId = _machineid;
            entry.ProcessId = _processid;
            entry.Oid = tx.Root.Oid;
            entry.Tid = tx.Tid;
            entry.ExpiresUtc = tx.ExpiresUtc;
            entry.MachineName = _machinename;

            WriteLockEntry(entry);
        }

        internal void LockAndVerify(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                Lock();

                LockEntry entry;
                var found = false;
                if (tx.IsReadOnly)
                {
                    found = TryFindReaderEntry(tx, out entry);
                }
                else
                {
                    found = TryFindWriterEntry(tx, out entry);
                }

                if (!found)
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

        private bool TryFindWriterEntry(Transaction tx, out LockEntry entry)
        {
            Perf.CallCount();

            ValidateLock(true);

            entry = GetLockEntry(WRITERSLOT);
            if (IsMatch(entry, tx))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds a reader slot. if tx is null it returns the first free slot
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="entry">the entry, only valid if returns true</param>
        /// <returns>false if no free slot has been found, otherwise false</returns>
        private bool TryFindReaderEntry(Transaction tx, out LockEntry entry)
        {
            Perf.CallCount();

            ValidateLock(true);

            // to satisfy compiler
            entry = GetLockEntry(READERSLOT);

            for (int i = READERSLOT; i < READERSLOT + MAX_READERS; i++)
            {
                entry = GetLockEntry(i);
                if (tx == null)
                {
                    if (IsFreeOrExpired(entry))
                    {
                        return true;
                    }
                }
                else
                {
                    if (IsMatch(entry, tx))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void WaitForReaderSlot()
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                while (true)
                {
                    Lock();

                    // find empty reader slot
                    if (TryFindReaderEntry(null, out var entry))
                    {
                        return;
                    }

                    Unlock();

                    Logger.LogInfo(LogTopics.Lock, "Waiting for ReaderSlot...");

                    // wait if no reader found
                    Thread.Sleep(Database.Options.LockInterval);
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
                if (entry.Type != LockEntryTypes.Reader)
                    return false;
            }
            else
            {
                if (entry.Type != LockEntryTypes.Writer)
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

            if (entry.MachineName.SequenceCompareTo(_machinename) != 0)
            {
                return false;
            }

            return true;
        }

        internal void AddReader(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            if (!TryFindReaderEntry(null, out var entry))
            {
                throw new InvalidOperationException("Reader slot is not free.");
            }

            entry.Type = LockEntryTypes.Reader;
            entry.MachineId = _machineid;
            entry.ProcessId = _processid;
            entry.Oid = tx.Root.Oid;
            entry.Tid = tx.Tid;
            entry.ExpiresUtc = tx.ExpiresUtc;
            entry.MachineName = _machinename;

            WriteLockEntry(entry);
        }

        internal void RemoveAndUnlock(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            try
            {
                LockEntry entry;
                var found = false;
                if (tx.IsReadOnly)
                {
                    found = TryFindReaderEntry(tx, out entry);
                }
                else
                {
                    found = TryFindWriterEntry(tx, out entry);
                }

                if (!found)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "LockEntry not found.");
                }

                entry.Clear();

                WriteLockEntry(entry);
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

                    Lockable.Dispose();
                    _lockfile?.Dispose();

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
