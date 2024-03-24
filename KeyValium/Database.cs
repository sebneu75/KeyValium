using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Encryption;
using KeyValium.Locking;
using KeyValium.Memory;
using KeyValium.Options;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KeyValium
{
    public sealed class Database : IDisposable
    {
        internal Database(string dbfile, DatabaseOptions dboptions)
        {
            Perf.CallCount();

            Filename = dbfile;

            Logger.CreateInstance(dbfile + ".log", LogLevel.All,
                //LogTopics.Lock | LogTopics.Transaction | LogTopics.Meta);
                //LogTopics.All);
                //LogTopics.Freespace | LogTopics.Allocation | LogTopics.Transaction);
                //LogTopics.Transaction | LogTopics.Validation | LogTopics.Insert);
                LogTopics.All & ~(LogTopics.Tracking | LogTopics.Allocation | LogTopics.Validation | LogTopics.Cursor));

            Options = new ReadonlyDatabaseOptions(dboptions);

            Validator = new PageValidator(this, Options.ValidationMode);

            if (!File.Exists(dbfile))
            {
                if (Options.CreateIfNotExists && !Options.ReadOnly)
                {
                    CreateDatabase(dbfile);
                }
                else
                {
                    throw new KeyValiumException(ErrorCodes.InvalidFileFormat, "Database file not found.");
                }
            }

            var fileaccess = Options.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;
            var fileshare = Options.InternalSharingMode == InternalSharingModes.Exclusive ? FileShare.None : FileShare.ReadWrite;

            var fileoptions = Options.InternalSharingMode == InternalSharingModes.SharedNetwork ? Limits.FileFlagNoBuffering : FileOptions.RandomAccess;

            DbFile = new FileStream(dbfile, FileMode.Open, fileaccess, fileshare, 4096, fileoptions);

            try
            {
                OpenDatabase(dboptions);
            }
            catch (Exception)
            {
                DbFile?.Dispose();
                throw;
            }

            MetaEntries = new MetaEntry[Limits.MetaPages];

            IsShared = Options.InternalSharingMode != InternalSharingModes.Exclusive;

            Limits = new Limits(this);

            // TODO check zero option
            Allocator = GetAllocator(Options.PageSize, Options.ZeroPagesOnEvict);
            Encryptor = GetEncryptor(Options.PageSize);
            Tracker = new Cursors.CursorTracker(this);
            Pager = IsShared ? new SharedPageProvider(this) : new ExclusivePageProvider(this);
            LockFile = IsShared ? new LockFile(this) : null;

            Pool = new KeyPool(Limits.MaximumKeySize);

            if (Options.FillCache)
            {
                FillCache();
            }
        }

        #region Properties

        internal readonly ReadonlyDatabaseOptions Options;

        internal readonly bool IsShared;

        public readonly Limits Limits;

        internal readonly LockFile LockFile;

        internal readonly PageProvider Pager;

        internal readonly PageAllocator Allocator;

        internal readonly FileStream DbFile;

        internal readonly Cursors.CursorTracker Tracker;

        internal readonly string Filename;

        internal readonly IEncryption Encryptor;

        internal readonly KeyPool Pool;

        internal readonly PageValidator Validator;

        //public readonly uint PageSize;

        ///// <summary>
        ///// ByteOrder of the database file
        ///// </summary>
        //public ByteOrder ByteOrder
        //{
        //    get
        //    {
        //        //isl   swap
        //        // 0      0       Big
        //        // 0      1       Little
        //        // 1      0       Little
        //        // 1      1       Big

        //        var islittleendian = BitConverter.IsLittleEndian ^ SwapEndianess;

        //        return islittleendian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
        //    }
        //}

        #endregion

        private IEncryption GetEncryptor(uint pagesize)
        {
            Perf.CallCount();

            if (Options.IsEncrypted)
            {
                return new AesEncryption(pagesize, Options.Password, Options.KeyFile);
            }

            return new NoEncryption();
        }

        private static PageAllocator GetAllocator(uint pagesize, bool zeropages)
        {
            Perf.CallCount();

            return new PageAllocator(pagesize, zeropages);
        }

        internal void OpenDatabase(DatabaseOptions dboptions)
        {
            Perf.CallCount();

            try
            {
                lock (_dblock)
                {
                    Logger.LogInfo(LogTopics.Database, "Opening Database {0} ...", DbFile.Name);

                    var hpany = ReadHeader();

                    if (hpany.Header.Magic != Limits.Magic)
                    {
                        throw new KeyValiumException(ErrorCodes.InvalidFileFormat, "Not a KeyValium-Database.");
                    }

                    var pagesize = 1u << hpany.Header.PageSizeExponent;

                    // will throw if out of bounds
                    Limits.ValidatePageSize(pagesize);

                    dboptions.Version = hpany.Header.Version;
                    if (dboptions.Version != 1)
                    {
                        throw new KeyValiumException(ErrorCodes.InvalidVersion, string.Format("Database version {0} not supported.", dboptions.Version));
                    }

                    // check typecodes
                    var itc = hpany.Header.InternalTypeCode;
                    if (dboptions.InternalTypeCode != InternalTypes.Raw && dboptions.InternalTypeCode != itc)
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, string.Format("Internal type code mismatch. Expected {0} but found {1}.", dboptions.InternalTypeCode, itc));
                    }
                    else
                    {
                        dboptions.InternalTypeCode = itc;
                    }

                    var utc = hpany.Header.UserTypeCode;
                    if (dboptions.UserTypeCode != 0 && dboptions.UserTypeCode != utc)
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, string.Format("User type code mismatch. Expected {0} but found {1}.", dboptions.UserTypeCode, utc));
                    }
                    else
                    {
                        dboptions.UserTypeCode = utc;
                    }

                    dboptions.EnableIndexedAccess = hpany.Header.Flags.HasFlag(DatabaseFlags.IndexedAccess);

                    Logger.LogInfo(LogTopics.Database, "Version = {0}", dboptions.Version);
                    Logger.LogInfo(LogTopics.Database, "PageSize = {0}", pagesize);
                    Logger.LogInfo(LogTopics.Database, "MaxKeySize = {0}", Limits.GetMaxKeyLength(pagesize));
                    Logger.LogInfo(LogTopics.Database, "MaxKeyAndValueSize = {0}", Limits.GetMaxKeyValueSize(pagesize));
                    Logger.LogInfo(LogTopics.Database, "MetaPages = {0}", Limits.MetaPages);
                    Logger.LogInfo(LogTopics.Database, "FirstMetaPage = {0}", 1);
                    Logger.LogInfo(LogTopics.Database, "InternalTypecode = {0}", dboptions.InternalTypeCode);
                    Logger.LogInfo(LogTopics.Database, "UserTypecode = {0}", dboptions.UserTypeCode);
                    Logger.LogInfo(LogTopics.Database, "Flags = {0}", dboptions.Flags);

                    Logger.LogInfo(LogTopics.Database, "Database opened successfully.");

                    dboptions.PageSize = pagesize;
                }
            }
            catch (Exception ex)
            {
                Logger.LogFatal(LogTopics.Database, ex);
                throw;
            }
        }

        internal void FillCache()
        {
            Perf.CallCount();

            using (var tx = BeginReadTransaction())
            {
                ScanTree(tx);
            }
        }

        /// <summary>
        /// Scans the tree
        /// </summary>
        /// <param name="rootpageno">Pagenumber of the starting page</param>
        /// <param name="mintid">Minimum Tid</param>
        internal void ScanTree(Transaction tx)
        {
            if (tx.Meta.DataRootPage == 0)
            {
                return;
            }

            var level = 0;      // for debugging purposes

            var scanqueue = new PageRangeList();
            scanqueue.AddPage(tx.Meta.DataRootPage);

            var pagestoscan = new List<KvPagenumber>();

            // walk the tree *breadth first*
            // if the tree is walked depth first it can take hours to scan
            // an uncached big file on mechanical hard disks
            while (true)
            {
                while (!scanqueue.IsEmpty && !Pager.Cache.IsFull)
                {
                    if (!scanqueue.TryTakePage(out var pageno))
                    {
                        throw new Exception("Unexpected empty queue!");
                    }

                    using (var page = Pager.ReadPage(tx, pageno, true))
                    {
                        switch (page.PageType)
                        {
                            case PageTypes.DataIndex:
                            case PageTypes.FsIndex:
                                ref var ipage = ref page.AsContentPage;
                                for (int i = 0; i <= ipage.EntryCount; i++)
                                {
                                    var p = ipage.GetLeftBranch(i);
                                    pagestoscan.Add(p);
                                }
                                break;

                            case PageTypes.DataLeaf:

                                ref var lpage = ref page.AsContentPage;
                                for (int i = 0; i < lpage.EntryCount; i++)
                                {
                                    var entry = lpage.GetEntryAt(i);

                                    var p = entry.SubTree;
                                    if (p.HasValue && p.Value != 0)
                                    {
                                        pagestoscan.Add(p.Value);
                                    }

                                    var p2 = entry.OverflowPageNumber;
                                    if (p2 != 0)
                                    {
                                        pagestoscan.Add(p2);
                                    }
                                }
                                break;

                            //case PageTypes.DataOverflow:
                            //    ref var ovpage = ref page.AsOverflowPage;
                            //    break;

                            //case PageTypes.FsLeaf:

                            //    lpage = ref page.AsContentPage;
                            //    for (int i = 0; i < lpage.EntryCount; i++)
                            //    {
                            //        var entry = lpage.GetEntryAt(i);
                            //    }
                            //    break;

                            default:
                                //throw new NotSupportedException("Unexpected Pagetype.");
                                break;
                        }
                    }
                }

                if (pagestoscan.Count == 0 || Pager.Cache.IsFull)
                {
                    break;
                }

                pagestoscan = pagestoscan.OrderBy(x => x).ToList();
                pagestoscan.ForEach(x => scanqueue.AddPage(x));
                pagestoscan.Clear();
                level++;
            }
        }

        private AnyPage ReadHeader()
        {
            Perf.CallCount();

            DbFile.Seek(0, SeekOrigin.Begin);

            var encheader = GetEncryptor(Limits.MinPageSize);

            var page = new AnyPage(null, Limits.MinPageSize);
            page.Initialize(0, false, null, 0);
            var read = DbFile.Read(page.Bytes.Span);
            encheader.Decrypt(page);
            page.CreateHeaderAndContent(null, 0);

            PageValidator.ValidateFileHeader(page, 0, false);

            return page;
        }

        /// <summary>
        /// creates a new Database
        /// </summary>
        private void CreateDatabase(string dbfile)
        {
            Perf.CallCount();

            try
            {
                Logger.LogInfo(LogTopics.Database, "Creating Database {0} ...", dbfile);

                if (Options.Version != 1)
                {
                    throw new KeyValiumException(ErrorCodes.InvalidParameter, "Version must be 1.");
                }

                var psexpo = Limits.ValidatePageSize(Options.PageSize);

                // open file in exclusive mode for creation
                using (var stream = new FileStream(dbfile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    var pages = new List<AnyPage>();

                    var enc = GetEncryptor(Options.PageSize);

                    // Create FileHeader
                    var header = new AnyPage(null, Options.PageSize);
                    header.Initialize(0, true, PageTypes.FileHeader, 0);
                    header.Header.InitFileHeader(Options, psexpo);
                    pages.Add(header);

                    // Create MetaPages
                    for (ulong i = 0; i < Limits.MetaPages; i++)
                    {
                        var mpany = new AnyPage(null, Options.PageSize);
                        mpany.Initialize(i + 1, true, PageTypes.Meta, 0);
                        var mp = new MetaPage(mpany);
                        mp.InitPage();
                        mp.LastPage = Limits.MetaPages;
                        pages.Add(mpany);
                    }

                    foreach (var page in pages)
                    {
                        KvDebug.Assert(page.PageType == PageTypes.FileHeader || page.PageType == PageTypes.Meta, "Unexpected pagetype!");

                        Validator.ValidatePage(page, page.PageNumber, true);

                        var cipher = enc.Encrypt(page);

                        stream.Seek((long)(page.PageNumber * Options.PageSize), SeekOrigin.Begin);
                        stream.Write(cipher);
                    }
                }

                Logger.LogInfo(LogTopics.Database, "Database created successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogFatal(LogTopics.Database, ex);
                throw;
            }
        }

        #region Meta

        internal MetaEntry[] MetaEntries;

        internal MetaPage RefreshMetaEntries(bool returnoldest)
        {
            Perf.CallCount();

            if (IsShared)
            {
                // flush buffers before reading metas
                // (metas could be changed by another process in shared mode)
                Pager.Flush();
            }

            Logger.LogInfo(LogTopics.Meta, "Refreshing all MetaEntries... ({0})", Limits.MetaPages);

            var mintid = KvTid.MaxValue;
            MetaPage selectedpage = null;

            for (ulong i = 0; i < Limits.MetaPages; i++)
            {
                var page = Pager.ReadPage(null, Limits.FirstMetaPage + i, true);
                var mp = new MetaPage(page);

                ValidateTids(mp);

                MetaEntries[i] = new MetaEntry(mp.Page.PageNumber, mp.Tid, mp.DataRootPage, mp.FsRootPage, mp.LastPage,
                                               mp.DataTotalCount, mp.DataLocalCount, mp.FsTotalCount, mp.FsLocalCount);
                if (returnoldest)
                {
                    if (mp.Tid < mintid)
                    {
                        mintid = mp.Tid;
                        selectedpage?.Page?.Dispose();
                        selectedpage = mp;
                    }
                    else
                    {
                        page.Dispose();
                    }
                }
                else
                {
                    page.Dispose();
                }
            }

            Logger.LogInfo(LogTopics.Meta, "Refreshing all MetaEntries done.");
            Logger.LogInfo(LogTopics.Meta, "Got oldest MetaPage: {0}", selectedpage?.Page?.PageNumber);

            return selectedpage;
        }

        /// <summary>
        /// returns the index of the Meta with the largest (newest) TID
        /// </summary>
        /// <returns></returns>
        internal int GetIndexOfNewestTid()
        {
            var index = 0;
            var tid = MetaEntries[0].Tid;

            for (int i = 1; i < Limits.MetaPages; i++)
            {
                ref var me = ref MetaEntries[i];
                if (me.Tid > tid)
                {
                    tid = me.Tid;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// returns the index of the Meta with the smallest (oldest) TID
        /// </summary>
        /// <returns></returns>
        internal int GetIndexOfOldestTid()
        {
            var index = 0;
            var tid = MetaEntries[0].Tid;

            for (int i = 1; i < Limits.MetaPages; i++)
            {
                ref var me = ref MetaEntries[i];
                if (me.Tid < tid)
                {
                    tid = me.Tid;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal KvTid GetOldestTid()
        {
            KvTid tid = MetaEntries[0].Tid;

            for (int i = 1; i < Limits.MetaPages; i++)
            {
                ref var me = ref MetaEntries[i];
                if (me.Tid < tid)
                {
                    tid = me.Tid;
                }
            }

            return tid;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal void GetMinMaxLastPage(out KvPagenumber min, out KvPagenumber max)
        {
            min = MetaEntries[0].LastPage;
            max = min;

            for (int i = 1; i < Limits.MetaPages; i++)
            {
                ref var me = ref MetaEntries[i];

                if (me.LastPage < min)
                {
                    min = me.LastPage;
                }

                if (me.LastPage > max)
                {
                    max = me.LastPage;
                }
            }
        }

        /// <summary>
        /// Save Meta, overwrite oldest
        /// </summary>
        /// <param name="meta"></param>
        internal void SaveMeta(Transaction tx, Meta meta)
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Meta, tx.Tid, "Saving Meta...");

            lock (_dblock)
            {
                var mp = RefreshMetaEntries(true);

                try
                {
                    mp.DataRootPage = meta.DataRootPage;
                    mp.FsRootPage = meta.FsRootPage;
                    mp.LastPage = meta.LastPage;
                    mp.Tid = meta.Tid;
                    mp.HeaderTid = meta.Tid;
                    mp.FooterTid = meta.Tid;
                    mp.DataTotalCount = meta.DataTotalCount;
                    mp.DataLocalCount = meta.DataLocalCount;
                    mp.FsTotalCount = meta.FsTotalCount;
                    mp.FsLocalCount = meta.FsLocalCount;

                    Pager.WritePage(tx, mp.Page);
                }
                finally
                {
                    mp.Page.Dispose();
                }
            }

            Logger.LogInfo(LogTopics.Meta, tx.Tid, "Saving done.");
        }

        internal Meta GetLatestMeta(bool inctid, bool prevsnapshot)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(_dblock), "_dblock not taken!");

            // TODO select meta in parameter

            RefreshMetaEntries(false);

            // TODO optimize
            //metas = metas.OrderByDescending(x => x.Tid).ToList();

            var metaindex = prevsnapshot ? GetIndexOfOldestTid() : GetIndexOfNewestTid();

            //
            // determine oldest running transaction
            //
            var mintid = GetOldestTid();

            if (_writetransaction != null && _writetransaction.Tid < mintid)
            {
                mintid = _writetransaction.Tid;
            }

            foreach (var tx in _readtransactions.Values)
            {
                if (tx.Tid < mintid)
                {
                    mintid = tx.Tid;
                }
            }

            // get oldest transaction from lockfile
            if (LockFile != null)
            {
                var mintidlock = LockFile.GetMinTid();
                if (mintidlock < mintid)
                {
                    mintid = mintidlock;
                }
            }

            ref var meta = ref MetaEntries[metaindex];

            var newtid = meta.Tid + (ulong)(inctid ? 1 : 0);

            GetMinMaxLastPage(out var minlastpage, out var maxlastpage);

            // return copy with minimum tid and actual last page
            // if maximum last page is returned it leads to gaps in the file
            // this can be fixed but the file would never shrink
            return new Meta(ref meta, newtid, mintid, minlastpage, maxlastpage);
        }

        private void ValidateTids(MetaPage mp)
        {
            Perf.CallCount();

            if (mp.HeaderTid != mp.Tid || mp.Tid != mp.FooterTid)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Tid mismatch in Meta page!");
            }
        }



        #endregion

        public void Validate(bool needwriteaccess)
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating Database.");

            if (_isdisposed)
            {
                throw new ObjectDisposedException("Database is already disposed.");
            }

            if (needwriteaccess && Options.ReadOnly)
            {
                throw new ObjectDisposedException("Database is in readonly mode.");
            }
        }

        #region Transactions

        private readonly object _dblock = new();

        private readonly Dictionary<ulong, Transaction> _readtransactions = new();

        private Transaction _writetransaction = null;

        /// <summary>
        /// Starts a read transaction.
        /// </summary>
        /// <returns>A read transaction.</returns>
        public Transaction BeginReadTransaction()
        {
            Perf.CallCount();

            lock (_dblock)
            {
                Validate(false);

                try
                {
                    // wait for free slot in lockfile
                    LockFile?.WaitForReaderSlot();

                    var meta = GetLatestMeta(false, false);

                    var tx = new Transaction(this, meta, true);
                    _readtransactions.Add(tx.Oid, tx);

                    // write lockfile entry
                    LockFile?.AddReader(tx);

                    Tracker.OnBeginRootTransaction(tx);

                    return tx;
                }
                finally
                {
                    LockFile?.Unlock();
                }
            }
        }

        /// <summary>
        /// Starts a read transaction for the previous snapshot.
        /// </summary>
        /// <returns>A read transaction.</returns>
        public Transaction BeginPreviousSnapshotReadTransaction()
        {
            Perf.CallCount();

            lock (_dblock)
            {
                Validate(false);

                try
                {
                    // wait for free slot in lockfile
                    LockFile?.WaitForReaderSlot();

                    var meta = GetLatestMeta(false, true);

                    var tx = new Transaction(this, meta, true);
                    _readtransactions.Add(tx.Oid, tx);

                    // write lockfile entry
                    LockFile?.AddReader(tx);

                    Tracker.OnBeginRootTransaction(tx);

                    return tx;
                }
                finally
                {
                    LockFile?.Unlock();
                }
            }
        }

        public Transaction BeginWriteTransaction()
        {
            Perf.CallCount();

            lock (_dblock)
            {
                Validate(true);

                try
                {
                    if (_writetransaction != null)
                    {
                        throw new KeyValiumException(ErrorCodes.TransactionFailed, "A write transaction is already in progress.");
                    }

                    // TODO check flags and file accessibility

                    // wait for free slot in lockfile
                    LockFile?.WaitForWriterSlot();

                    var meta = GetLatestMeta(true, false);

                    var tx = new Transaction(this, meta, false);
                    _writetransaction = tx;

                    // write lockfile entry
                    LockFile?.AddWriter(tx);

                    Tracker.OnBeginRootTransaction(tx);

                    return tx;
                }
                finally
                {
                    LockFile?.Unlock();
                }
            }
        }

        internal void EndTransaction(Transaction tx)
        {
            Perf.CallCount();

            lock (_dblock)
            {
                // TODO: deal with expired transaction (Abort-Method?)

                //if (tx.State == TransactionStates.Active)
                //{
                //    //// TODO fix deadlock caused by recursive call
                //    //tx.Abort(false);

                //    throw new KeyValiumException(ErrorCodes.TransactionFailed, "Cannot end active transaction");
                //}

                if (_writetransaction == tx)
                {
                    _writetransaction = null;
                }
                else
                {
                    _readtransactions.Remove(tx.Oid);
                }
            }
        }

        //private Pager GetPager(Meta meta)
        //{
        //    var pager = Options.Exclusive ? Pager : new Pager(this, Options.PageSize, Options.CacheSizeTransactionMB);
        //    //pager.NextPage = meta.LastPage + 1;

        //    return pager;
        //}

        /// <summary>
        /// sets the filesize according to the maximum of the Meta's lastpage
        /// must be called after Meta is updated
        /// </summary>
        internal void UpdateFilesize()
        {
            Perf.CallCount();

            lock (_dblock)
            {
                // TODO make faster
                // check if MetaEntry.MaxLastpage can be used
                RefreshMetaEntries(false);
                GetMinMaxLastPage(out _, out var lastpage);

                var filelength = DbFile.Length;
                var metalength = (long)(lastpage + 1) * Options.PageSize;

                if (filelength > metalength)
                {
                    SetFilesize(metalength);
                }
                else if (filelength < metalength)
                {
                    // this can happen if the last allocated pages of a transaction turn into loose pages (keys get inserted then deleted)
                    // loose pages are not written to disk but are referenced as freespace
                    // TODO ignore for now
                    //throw new KeyValiumException(ErrorCodes.InternalError, "File too small.");
                    //Console.WriteLine("File too small.");
                }
            }
        }

        #endregion

        public static Database Open(string filename)
        {
            Perf.CallCount();

            return Open(filename, new DatabaseOptions());
        }

        public static Database Open(string filename, DatabaseOptions options)
        {
            Perf.CallCount();

            options.Validate();

            return new Database(filename, options);
        }

        /// <summary>
        /// Only outside of transactions!
        /// </summary>
        /// <param name="path"></param>
        internal void CopyTo(string path)
        {
            Perf.CallCount();

            // TODO check if transaction is active
            lock (_dblock)
            {
                DbFile.Flush();

                DbFile.Seek(0, SeekOrigin.Begin);

                var length = DbFile.Length;
                var block = new byte[1024 * 1024];

                using (var writer = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    for (long pos = 0; pos < length; pos += block.Length)
                    {
                        var bytesread = DbFile.Read(block, 0, block.Length);
                        writer.Write(block, 0, bytesread);
                    }
                }

                DbFile.Flush();
            }
        }

        internal void SetFilesize(long length)
        {
            Perf.CallCount();

            Pager.SetFilesize(length);
        }

        #region IDisposable support

        private bool _isdisposed;

        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    lock (_dblock)
                    {
                        // dispose transactions
                        foreach (var tx in _readtransactions.Values)
                        {
                            tx.Dispose(false);
                        }

                        _readtransactions.Clear();

                        _writetransaction?.Dispose();
                        _writetransaction = null;

                        Tracker?.Dispose();
                        Pager?.Dispose();
                        Allocator?.Dispose();
                        LockFile?.Dispose();
                        DbFile?.Dispose();
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
