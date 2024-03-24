using KeyValium.Collections;
using KeyValium.Encryption;
using KeyValium.Memory;
using KeyValium.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    /// <summary>
    /// A PageProvider manages access to individual pages.
    /// </summary>
    internal abstract class PageProvider : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The Database for which to create the PageProvider</param>
        internal PageProvider(Database db)
        {
            Perf.CallCount();

            Database = db;
            DbFile = db.DbFile;
            PageSize = db.Options.PageSize;
            Allocator = db.Allocator;
            
            Encryptor = db.Encryptor;

            Validator = db.Validator;

            MaxPagenumber = ((ulong)long.MaxValue - PageSize) / PageSize;

            Cache = new LruCache(Database.Options.CachedItems);
        }

        #region Variables

        /// <summary>
        /// The Cache
        /// </summary>
        protected internal readonly LruCache Cache;

        /// <summary>
        /// The Database instance
        /// </summary>
        protected internal readonly Database Database;

        /// <summary>
        /// The PageAllocator
        /// </summary>
        protected internal readonly PageAllocator Allocator;

        /// <summary>
        /// The FileStream of the Database file
        /// </summary>
        protected internal readonly FileStream DbFile;

        /// <summary>
        /// The Encryptor
        /// </summary>
        protected internal readonly IEncryption Encryptor;

        /// <summary>
        /// The PageAllocator
        /// </summary>
        internal readonly PageValidator Validator;

        /// <summary>
        /// Maximum page number
        /// </summary>
        internal readonly KvPagenumber MaxPagenumber;

        /// <summary>
        /// The page size
        /// </summary>
        protected internal readonly uint PageSize;

        #endregion

        #region Accessing the filestream

        private readonly object _seeklock = new();

        /// <summary>
        /// Reads one page from the database file and decrypts the data into the given AnyPage
        /// </summary>
        /// <param name="page">the page to be read</param>
        /// <param name="createheader">if true header and content structures are created on the AnyPage</param>
        /// <exception cref="NotSupportedException"></exception>
        protected void ReadLocked(AnyPage page, bool createheader)
        {
            Perf.CallCount();

            KvDebug.Assert(page.Bytes.Length == PageSize, "Pagesize mismatch!");

            DbFile.Seek((long)(page.PageNumber * PageSize), SeekOrigin.Begin);
            var read = DbFile.Read(page.Bytes.Span);

            if (read != page.Bytes.Length)
            {
                var msg = string.Format("Read length mismatch! ({0} != {1}) PageNo={2}", read, page.Bytes.Length, page.PageNumber);
                throw new NotSupportedException(msg);
            }
            KvDebug.Assert(read == page.Bytes.Length, string.Format("Read length mismatch! ({0} != {1})", read, page.Bytes.Length));

            Encryptor.Decrypt(page);
            
            if (createheader)
            {
                page.CreateHeaderAndContent(null, 0);
            }

            Validator.ValidatePage(page, page.PageNumber, false);
        }

        protected void WriteLocked(AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page.Bytes.Length == PageSize, "Pagesize mismatch!");

            Validator.ValidatePage(page, page.PageNumber, true);

            DbFile.Seek((long)(page.PageNumber * PageSize), SeekOrigin.Begin);
            DbFile.Write(Encryptor.Encrypt(page));
        }

        internal virtual void Flush()
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                DbFile.Flush(Database.Options.FlushToDisk);
            }
        }

        internal virtual void SetFilesize(long length)
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                DbFile.SetLength(length);
            }
        }

        #endregion

        internal void ClearWriteCache()
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                ClearWriteCacheInternal();
            }
        }

        protected virtual void ClearWriteCacheInternal()
        {
            Perf.CallCount();

            // do nothing by default
        }

        internal void CommitWriteCache(KvTid sourcetid, KvTid tid)
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                CommitWriteCacheInternal(sourcetid, tid);
            }
        }

        protected virtual void CommitWriteCacheInternal(KvTid sourcetid, KvTid tid)
        {
            Perf.CallCount();

            // do nothing by default
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="pagenumber"></param>
        /// <param name="createheader"></param>
        /// <param name="unspill"></param>
        /// <returns>the page with incremented RefCount</returns>
        internal AnyPage ReadPage(Transaction tx, KvPagenumber pagenumber, bool createheader, bool unspill = false)
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                return ReadPageInternal(tx, pagenumber, createheader, unspill);
            }
        }

        protected abstract AnyPage ReadPageInternal(Transaction tx, KvPagenumber pagenumber, bool createheader, bool unspill);

        internal void WritePage(Transaction tx, AnyPage page)
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                WritePageInternal(tx, page);
            }
        }

        protected virtual void RemoveCachedPagesInternal(Transaction tx, PageRange range)
        {
            Perf.CallCount();

            // do nothing by default
        }

        internal void RemoveCachedPages(Transaction tx, PageRange range)
        {
            Perf.CallCount();

            lock (_seeklock)
            {
                RemoveCachedPagesInternal(tx, range);
            }
        }

        protected abstract void WritePageInternal(Transaction tx, AnyPage page);

        // TODO fix method (does not seem to belong here) or put somewhere else 
        public AnyPage ReadPageInspector(KvPagenumber pageno, uint pagesize, bool createheader)
        {
            Perf.CallCount();

            var page = Allocator.GetPage(pageno, createheader, null, 0);

            lock (_seeklock)
            {
                ReadLocked(page, createheader);
            }

            return page;
        }

        internal abstract CacheStats GetCacheStats();

        internal abstract void ClearCacheStats();

        private ulong _lastreadahead = 0;

        private const int _buffersize = 4 * 1024 * 1024;

        private byte[] _buffer = new byte[_buffersize];

        // TODO fix method (does not seem to belong here) or put somewhere else 
        internal void ReadAheadInspector(KvPagenumber pageno)
        {
            Perf.CallCount();

            var pagecount = _buffersize / PageSize;

            if (_lastreadahead == 0 || pageno < _lastreadahead || pageno >= _lastreadahead + pagecount)
            {
                lock (_seeklock)
                {
                    DbFile.Seek((long)(pageno * PageSize), SeekOrigin.Begin);
                    var read = DbFile.Read(_buffer);
                    _lastreadahead = pageno;
                }
            }
        }

        #region IDisposable

        // set only in base class
        protected bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!disposedValue)
            {
                if (disposing)
                {
                    Encryptor?.Dispose();
                }

                disposedValue = true;
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
