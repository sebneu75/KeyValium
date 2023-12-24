using KeyValium.Options;
using System.Security.Cryptography;
using System.Text;

namespace KeyValium.Locking
{
    internal unsafe class LockFile : IDisposable
    {
        const int WRITERSLOT = 0;
        const int MAX_WRITERS = 1;

        const int READERSLOT = 1;
        const int MAX_READERS = 1022;

        internal const int ENTRY_SIZE = 64;
        const int HEADER_SIZE = 64;

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

            SharingMode = db.Options.SharingMode;

            Filename = db.Filename + ".lock";

            FileLock = new FileLock(db, this);

            if (db.Options.SharingMode == SharingModes.SharedNetwork)
            {
                SelectedLock = FileLock;
            }
            else if (db.Options.SharingMode == SharingModes.SharedLocal)
            {
                SelectedLock = new MutexLock(db, this);
            }
            else
            {
                throw new KeyValiumException(ErrorCodes.InvalidParameter, "Unsupported sharing mode!");
            }

            _processid = Process.GetCurrentProcess().Id;
            _machineid = GetMachineId();

            Filesize = HEADER_SIZE + (MAX_WRITERS + MAX_READERS) * ENTRY_SIZE;

            _lockfile = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, Filesize, FileOptions.SequentialScan);

            EnsureLockFile();
        }

        #endregion

        #region Variables

        public readonly string Filename;

        public readonly int Filesize;

        internal readonly SharingModes SharingMode;

        private readonly FileStream _lockfile;

        internal readonly object LockObject = new object();

        private readonly byte[] _machineid;

        private readonly int _processid;

        internal readonly FileLock FileLock;

        internal readonly ILockable SelectedLock;

        #endregion

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

        internal void Lock()
        {
            SelectedLock.Lock();
        }

        internal void Unlock()
        {
            SelectedLock.Unlock();
        }

        internal void ValidateLock(bool expected)
        {
            SelectedLock.ValidateLock(expected);
        }

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
                FileLock.Lock();

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
                FileLock.Unlock();
            }
        }

        private void WriteHeader()
        {
            Perf.CallCount();

            FileLock.ValidateLock(true);

            var header = new byte[HEADER_SIZE];

            fixed (byte* ptr = header)
            {
                var span = new ByteSpan(ptr, HEADER_SIZE);
                span.WriteUInt(0, Limits.Magic);
                span.WriteUShort(0x04, PageTypes.LockFile);
                span.WriteUShort(0x06, (ushort)SharingMode);
                _machineid.CopyTo(span.Slice(0x10, _machineid.Length).Span);
            }

            _lockfile.Seek(0, SeekOrigin.Begin);
            _lockfile.Write(header, 0, HEADER_SIZE);
            _lockfile.Flush();
        }

        private void ValidateHeader()
        {
            Perf.CallCount();

            FileLock.ValidateLock(true);

            if (_lockfile.Length < HEADER_SIZE)
            {
                throw new KeyValiumException(ErrorCodes.InvalidFileFormat, "Invalid lockfile.");
            }

            var header = new byte[HEADER_SIZE];

            _lockfile.Seek(0, SeekOrigin.Begin);
            _lockfile.Read(header, 0, HEADER_SIZE);

            fixed (byte* ptr = header)
            {
                var span = new ByteSpan(ptr, HEADER_SIZE);

                var magic = span.ReadUInt(0x00);
                var pagetype = span.ReadUShort(0x04);
                var sharingmode = (SharingModes)span.ReadUShort(0x06);
                var machineid = span.Slice(0x10, _machineid.Length);

                if (magic != Limits.Magic)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (Magic mismatch)");
                }

                if (pagetype != PageTypes.LockFile)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (Pagetype mismatch)");
                }

                if (sharingmode != SharingMode)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (SharingMode mismatch)");
                }

                if (SharingMode == SharingModes.SharedLocal)
                {
                    if (!machineid.Span.SequenceEqual(_machineid))
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, "Invalid lockfile. (MachineId mismatch)");
                    }
                }
            }
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
                while (true)
                {
                    Lock();

                    // check if writer slot is empty or expired
                    var entry = ReadLockEntry(WRITERSLOT);
                    if (IsFreeOrExpired(entry))
                    {
                        Logger.LogInfo(LogTopics.Lock, "WaitForWriterSlot succeeded.");
                        return;
                    }

                    Unlock();

                    Logger.LogInfo(LogTopics.Lock, "Waiting for WriterSlot...");
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
            _lockfile.Flush(true);

            Logger.LogInfo(LogTopics.Lock, "LockEntry written: {0}", entry);
        }

        internal void AddWriter(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(true);

            var entry = ReadLockEntry(WRITERSLOT);
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

        internal void LockAndVerify(Transaction tx)
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                Lock();

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

            var entry = ReadLockEntry(WRITERSLOT);
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

            for (int i = READERSLOT; i < READERSLOT + MAX_READERS; i++)
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
                while (true)
                {
                    Lock();

                    // check if reader slot is empty or expired
                    var entry = FindReaderEntry(null);
                    if (entry != null)
                    {
                        return;
                    }

                    Unlock();

                    Logger.LogInfo(LogTopics.Lock, "Waiting for ReaderSlot...");
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

                    FileLock.Dispose();
                    SelectedLock.Dispose();
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
