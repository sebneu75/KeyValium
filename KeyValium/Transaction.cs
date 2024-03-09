using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Cursors;
using KeyValium.Iterators;
using KeyValium.Locking;
using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System;
using static System.Formats.Asn1.AsnWriter;

namespace KeyValium
{
    public sealed class Transaction : IDisposable
    {
        private static ulong OidCounter = 0;

        #region Constructors

        /// <summary>
        /// Constructor for root transactions
        /// </summary>
        /// <param name="db"></param>
        /// <param name="meta"></param>
        /// <param name="isreadonly"></param>
        internal Transaction(Database db, Meta meta, bool isreadonly)
        {
            Perf.CallCount();

            Oid = Interlocked.Increment(ref OidCounter);
            Version = 1;
            Database = db;
            IsReadOnly = isreadonly;
            Meta = meta;
            PageSize = Database.Options.PageSize;
            Tid = Meta.Tid;
            Pager = Database.Pager;
            Allocator = Database.Allocator;

            State = TransactionStates.Active;
            ExpiresUtc = DateTime.UtcNow.AddSeconds(LockFile.TX_TIMEOUT);
            Root = this;

            TxLock = new object();

            Pager.ClearWriteCache();

            CreateDefaultCursors();

            Logger.LogInfo(LogTopics.Transaction, Tid, "{0}Transaction started.", isreadonly ? "Read" : "Write");
        }

        /// <summary>
        /// Constructor for nested transactions
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="meta"></param>
        private Transaction(Transaction parent, Meta meta)
        {
            Perf.CallCount();

            // todo check if new Oid is needed/works
            Oid = Interlocked.Increment(ref OidCounter);
            Version = 1;
            Database = parent.Database;
            PageSize = Database.Options.PageSize;
            Meta = meta;
            Tid = Meta.Tid;
            Pager = parent.Pager;
            Allocator = parent.Allocator;
            IsReadOnly = false;
            State = TransactionStates.Active;

            // share lock with parent
            TxLock = parent.TxLock;

            parent.Child = this;
            Parent = parent;

            ExpiresUtc = parent.ExpiresUtc;

            Root = parent.Root;

            CreateDefaultCursors();

            Logger.LogInfo(LogTopics.Transaction, Tid, "{0}Transaction child started.", IsReadOnly ? "Read" : "Write");
        }

        #endregion

        #region Variables

        internal readonly object TxLock;

        internal int Version;

        internal readonly Meta Meta;

        internal readonly Database Database;

        internal readonly PageAllocator Allocator;

        internal readonly PageProvider Pager;

        internal Cursor DataCursor;

        internal Cursor FsCursor1;

        internal Cursor FsCursor2;

        internal readonly ulong Oid;

        internal readonly KvTid Tid;

        /// <summary>
        /// Returns true if the transaction is read only.
        /// </summary>
        public readonly bool IsReadOnly;

        internal readonly uint PageSize;

        /// <summary>
        /// Returns the root transaction of the transaction chain.
        /// </summary>
        public readonly Transaction Root;

        /// <summary>
        /// Returns the UTC-timestamp when the transaction expires.
        /// </summary>
        public readonly DateTime ExpiresUtc;

        /// <summary>
        /// Enables or disables append mode. The default is false. 
        /// This changes how the split algorithm works. When disabled pages are splitted roughly in half.
        /// If enabled it works as follows: When a page is full and the new entry would have to be appended 
        /// at the end of the page a new empty page is created and the key is inserted there.
        /// This should be enabled if many keys are inserted in ascending order to be more space efficient.
        /// </summary>
        public bool AppendMode;

        /// <summary>
        /// User defined tag.
        /// </summary>
        public object Tag;

        #endregion

        #region Properties

        public TransactionStates State
        {
            get;
            private set;
        }

        public Transaction Parent
        {
            get;
            private set;
        }

        public Transaction Child
        {
            get;
            private set;
        }

        #endregion

        #region Helpers

        private void CreateDefaultCursors()
        {
            Perf.CallCount();

            DataCursor = new Cursor(this, null, InternalTrackingScope.None, false, true);

            if (!IsReadOnly)
            {
                FsCursor1 = new Cursor(this, null, InternalTrackingScope.None, true, true);
                FsCursor2 = new Cursor(this, null, InternalTrackingScope.None, true, true);
            }
        }

        #endregion

        #region Validation

        internal void Validate(bool needwriteaccess)
        {
            Logger.LogInfo(LogTopics.Validation, "Validating Transaction {0} ({1}).", Tid, Oid);

            Perf.CallCount();

            //Database.Validate();

            if (_isdisposed)
            {
                throw new ObjectDisposedException("Transaction is already disposed.");
            }

            if (needwriteaccess && IsReadOnly)
            {
                throw new NotSupportedException("Transaction is readonly.");
            }

            if (Database.IsShared && DateTime.UtcNow > ExpiresUtc)
            {
                throw new NotSupportedException("Transaction timed out.");
            }

            if (Child != null)
            {
                throw new NotSupportedException("Transaction has open child transaction.");
            }

            if (State == TransactionStates.Committed)
            {
                throw new NotSupportedException("Transaction is already committed.");
            }
            else if (State == TransactionStates.RolledBack)
            {
                throw new NotSupportedException("Transaction is already rolled back.");
            }
            else if (State == TransactionStates.Failed)
            {
                throw new NotSupportedException("Transaction has failed.");
            }
        }

        internal void ValidateKey(ref ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            if (key.Length > Database.Limits.MaximumKeySize)
            {
                throw new NotSupportedException("Key too long.");
            }
        }

        internal TxVersion GetVersion()
        {
            KvDebug.Assert(Monitor.IsEntered(TxLock), "TxLock not held!");

            return new TxVersion(this);
        }

        #endregion

        #region Nested Transactions

        public Transaction BeginChildTransaction()
        {
            Perf.CallCount();

            lock (TxLock)
            {
                Validate(true);

                var meta = new Meta(Meta);  // make copy

                var tx = new Transaction(this, meta);

                tx.CopyParentFreeSpace();

                Database.Tracker.OnBeginChildTransaction(this, tx);

                return tx;
            }
        }

        /// <summary>
        /// Copies the freespace and loose page entries from the parent
        /// </summary>
        /// <param name="transaction"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void CopyParentFreeSpace()
        {
            Perf.CallCount();

            // TODO optimize by tracking only the actually used pages

            // copy loose pages
            Pages.CopyLoosePages(Parent.Pages);

            // copy flag
            _fsexhausted = Parent._fsexhausted;

            // copy last reserved entry
            _fslastreserved = Parent._fslastreserved;

            // copy reserved entries
            foreach (var pair in Parent._fsreserved)
            {
                _fsreserved.Add(pair.Key, pair.Value);
            }

            // copy ranges
            _fsreservedranges = Parent._fsreservedranges.Copy();
            _fstouchedranges = Parent._fstouchedranges.Copy();

            // copy entries to delete
            _fstodelete.UnionWith(Parent._fstodelete);
        }

        /// <summary>
        /// Moves the freespace bookkeeping to the parent transaction on commit
        /// </summary>
        /// <param name="transaction"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void UpdateParentFreeSpace()
        {
            Perf.CallCount();

            // copy flag
            Parent._fsexhausted = _fsexhausted;

            // copy last reserved entry
            Parent._fslastreserved = _fslastreserved;

            // move reserved entries
            Parent._fsreserved = _fsreserved;

            // move ranges
            Parent._fsreservedranges = _fsreservedranges;
            Parent._fstouchedranges = _fstouchedranges;

            // move entries to delete
            Parent._fstodelete = _fstodelete;
        }

        #endregion

        #region Cursors

        internal Cursor GetCursor(TreeRef treeref, InternalTrackingScope scope, bool track = true)
        {
            Perf.CallCount();

            //Validate(!IsReadOnly);

            var cursor = new Cursor(this, treeref, scope, false, false);

            if (track && scope != InternalTrackingScope.None)
            {
                Database.Tracker.Add(cursor);
            }

            return cursor;
        }

        internal Cursor GetCursorEx(TreeRef treeref, InternalTrackingScope scope)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                return GetCursor(treeref, scope);
            }
        }

        [SkipLocalsInit]
        internal Cursor GetDataCursor(TreeRef treeref)
        {
            Perf.CallCount();

            var ret = DataCursor;
            ret.TreeRef = treeref;
            return ret;
        }

        [SkipLocalsInit]
        internal Cursor GetFsCursor1()
        {
            Perf.CallCount();

            return FsCursor1;
        }

        [SkipLocalsInit]
        internal Cursor GetFsCursor2()
        {
            Perf.CallCount();

            return FsCursor2;
        }

        #endregion

        #region external API

        /// <summary>
        /// Returns the local number of keys (not including subtrees) in the subtree pointed to by treeref.
        /// If treeref is null the root tree is used.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <returns>The number of keys in tree excluding its subtrees.</returns>
        public ulong GetLocalCount(TreeRef treeref)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    return treeref == null ? Meta.DataLocalCount : treeref.LocalCount;
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns the total number of keys (including subtrees) in the subtree pointed to by treeref.
        /// If treeref is null the root tree is used.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <returns>The number of keys in tree including its subtrees.</returns>
        public ulong GetTotalCount(TreeRef treeref)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    return treeref == null ? Meta.DataTotalCount : treeref.TotalCount;
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }            
        }

        /// <summary>
        /// Gets the value associated with the key.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A reference to the value. If the key does not exist the returned ValueRef is invalid.
        /// The returned ValueRef becomes invalid when the transaction ends or gets modified.
        /// </returns>
        public ValueRef Get(TreeRef treeref, ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                return cursor.GetCurrentValue();
                            }
                        }

                        return new ValueRef(this, null, 0);
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates the value associated with the key. An exception is thrown when the key does not exist.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public void Update(TreeRef treeref, ReadOnlySpan<byte> key, ReadOnlySpan<byte> val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            Update(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates the value associated with the key. The whole stream is taken as value. 
        /// The stream must be seekable. An exception is thrown when the key does not exist.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        public void Update(TreeRef treeref, ReadOnlySpan<byte> key, Stream val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            Update(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates the value associated with the key. Takes length bytes from the current position 
        /// of the stream as the value. An exception is thrown when the key does not exist.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Update(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, length);

            Update(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates the value associated with the key. Takes length bytes from the given position 
        /// of the stream as the value. The stream must be seekable. An exception is thrown when the key does not exist.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="position">The position in the stream where the value starts.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Update(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long position, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, position, length);

            Update(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates the value associated with the key. An exception is thrown when the key does not exist.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        internal void Update(TreeRef treeref, ReadOnlySpan<byte> key, ref ValInfo val)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(true);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                // save subtree info
                                cursor.GetCurrentEntryInfo(out var subtree, out var tc, out var lc, out var ovpageno);
                                if (ovpageno != 0)
                                {
                                    // mark overflow pages as free
                                    DeleteOverflowPages(ovpageno);
                                }

                                ovpageno = 0;
                                ulong ovlength = 0;

                                if (val.Length > Database.Limits.MaxInlineValueSize((ushort)key.Length))
                                {
                                    CreateOverflowPages(ref val, out ovpageno, out ovlength);
                                }

                                var entry = new EntryExtern(key, val, subtree, tc, lc, ovpageno, ovlength);
                                cursor.UpdateKey(ref entry);

                                Version++;

                                SpillCheck();

                                //Console.WriteLine(cursor.Dump());
                                KvDebug.ValidateCursor2(this, cursor, treeref, key);
                            }
                            else
                            {
                                throw new KeyValiumException(ErrorCodes.KeyNotFound, "Key does not exist.");
                            }
                        }
                    }
                    catch (KeyValiumException ex)
                    {
                        if (ex.ErrorCode != ErrorCodes.KeyNotFound)
                        {
                            Fail(true);
                        }

                        throw;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates or inserts the value associated with the key.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public bool Upsert(TreeRef treeref, ReadOnlySpan<byte> key, ReadOnlySpan<byte> val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            return Upsert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates or inserts the value associated with the key. The whole stream is taken as value. 
        /// The stream must be seekable.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        public void Upsert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            Upsert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates or inserts the value associated with the key. Takes length bytes from the current position 
        /// of the stream as the value. 
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Upsert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, length);

            Upsert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates or inserts the value associated with the key. Takes length bytes from the given position 
        /// of the stream as the value. The stream must be seekable.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="position">The position in the stream where the value starts.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Upsert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long position, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, position, length);

            Upsert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Updates or inserts the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns>true, if the key has been inserted otherwise false</returns>
        internal bool Upsert(TreeRef treeref, ReadOnlySpan<byte> key, ref ValInfo val)
        {
            Perf.CallCount();

            var ret = false;

            lock (TxLock)
            {
                try
                {
                    Validate(true);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                // save subtree info
                                cursor.GetCurrentEntryInfo(out var subtree, out var tc, out var lc, out var ovpageno);
                                if (ovpageno != 0)
                                {
                                    // mark overflow pages as free
                                    DeleteOverflowPages(ovpageno);
                                }

                                ovpageno = 0;
                                ulong ovlength = 0;

                                if (val.Length > Database.Limits.MaxInlineValueSize((ushort)key.Length))
                                {
                                    CreateOverflowPages(ref val, out ovpageno, out ovlength);
                                }

                                var entry = new EntryExtern(key, val, subtree, tc, lc, ovpageno, ovlength);
                                cursor.UpdateKey(ref entry);


                                Version++;
                            }
                            else
                            {
                                ulong ovpageno = 0;
                                ulong ovlength = 0;

                                if (val.Length > Database.Limits.MaxInlineValueSize((ushort)key.Length))
                                {
                                    CreateOverflowPages(ref val, out ovpageno, out ovlength);
                                }

                                var entry = new EntryExtern(key, val, null, 0, 0, ovpageno, ovlength);
                                cursor.InsertKey(ref entry);

                                Version++;

                                ret = true;
                            }

                            //Console.WriteLine(cursor.Dump());
                            KvDebug.ValidateCursor2(this, cursor, treeref, key);
                        }

                        SpillCheck();

                        return ret;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Inserts the value associated with the key. An exception is thrown when the key already exists.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public void Insert(TreeRef treeref, ReadOnlySpan<byte> key, ReadOnlySpan<byte> val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            Insert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Inserts the value associated with the key. The whole stream is taken as value. 
        /// The stream must be seekable. An exception is thrown when the key already exists.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        public void Insert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val);

            Insert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Inserts the value associated with the key. Takes length bytes from the current position 
        /// of the stream as the value. An exception is thrown when the key already exists.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Insert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, length);

            Insert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Inserts the value associated with the key. Takes length bytes from the given position 
        /// of the stream as the value. The stream must be seekable. An exception is thrown when the key already exists.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="val">The stream that represents the value.</param>
        /// <param name="position">The position in the stream where the value starts.</param>
        /// <param name="length">The number of bytes to read from the stream.</param>
        public void Insert(TreeRef treeref, ReadOnlySpan<byte> key, Stream val, long position, long length)
        {
            Perf.CallCount();

            var valinfo = new ValInfo(val, position, length);

            Insert(treeref, key, ref valinfo);
        }

        /// <summary>
        /// Inserts the key. Will fail if the key already exists.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        internal void Insert(TreeRef treeref, ReadOnlySpan<byte> key, ref ValInfo val)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(true);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                throw new KeyValiumException(ErrorCodes.KeyAlreadyExists, "Key already exists.");
                            }

                            //Console.WriteLine(cursor.Dump());

                            ulong ovpageno = 0;
                            ulong ovlength = 0;

                            if (val.Length > Database.Limits.MaxInlineValueSize((ushort)key.Length))
                            {
                                CreateOverflowPages(ref val, out ovpageno, out ovlength);
                            }

                            var entry = new EntryExtern(key, val, null, 0, 0, ovpageno, ovlength);
                            cursor.InsertKey(ref entry);

                            Version++;

                            KvDebug.ValidateCursor2(this, cursor, treeref, key);
                        }

                        SpillCheck();
                    }
                    catch (KeyValiumException ex)
                    {
                        if (ex.ErrorCode != ErrorCodes.KeyAlreadyExists)
                        {
                            Fail(true);
                        }

                        throw;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the subtree pointed to by treeref. 
        /// Warning: When called with null the whole database will be deleted.
        /// This is an expensive operation.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        public bool DeleteTree(TreeRef treeref)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(true);
                    treeref?.Validate(this);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            cursor.SetPosition(CursorPositions.BeforeFirst);
                            var ret = cursor.DeleteTree();

                            Version++;

                            return ret;
                        }
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the key and the associated value.
        /// If the key has a nonempty subtree only the value will be deleted.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if the key has been deleted. 
        /// False if the key does not exist or has a nonempty subtree.</returns>
        public bool Delete(TreeRef treeref, ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            var ret = false;

            lock (TxLock)
            {
                try
                {
                    Validate(true);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                // save subtree info
                                cursor.GetCurrentEntryInfo(out var subtree, out var tc, out var lc, out var ovpageno);
                                if (ovpageno != 0)
                                {
                                    // mark overflow pages as free
                                    DeleteOverflowPages(ovpageno);
                                }

                                // do not delete keys with subtree
                                if (subtree.HasValue && subtree.Value != 0)
                                {
                                    var valinfo = new ValInfo();

                                    // update with empty value
                                    var entry = new EntryExtern(key, valinfo, subtree, tc, lc, 0, 0);
                                    cursor.UpdateKey(ref entry);
                                }
                                else
                                {
                                    ret = cursor.DeleteKey();
                                }

                                Version++;

                                KvDebug.ValidateCursor2(this, cursor, treeref, key, cursor.DeleteHandling);

                                SpillCheck();
                            }
                        }

                        return ret;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Checks if a key exists.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if the key exists.
        /// False if the key does not exist.
        /// </returns>
        public bool Exists(TreeRef treeref, ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            return cursor.SetPosition(key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to get the first key in a tree.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="first">The first key. Only valid if the method returns true.</param>
        /// <returns>
        /// True if there is a first key.
        /// False if the tree is empty.
        /// </returns>
        public bool TryGetFirst(TreeRef treeref, out ReadOnlySpan<byte> first)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(CursorPositions.BeforeFirst))
                            {
                                if (cursor.MoveToNextKey())
                                {
                                    first = cursor.GetCurrentKeySpan();
                                    return true;
                                }
                            }
                        }

                        first = default;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to get the last key in a tree.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="last">The last key. Only valid if the method returns true.</param>
        /// <returns>
        /// True if there is a last key.
        /// False if the tree is empty.
        /// </returns>
        public bool TryGetLast(TreeRef treeref, out ReadOnlySpan<byte> last)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(CursorPositions.BehindLast))
                            {
                                if (cursor.MoveToPrevKey())
                                {
                                    last = cursor.GetCurrentKeySpan();
                                    return true;
                                }
                            }
                        }

                        last = default;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to get the next key in a tree.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="next">The next key. Only valid if the method returns true.</param>
        /// <returns>
        /// True if there is a next key.
        /// False if there is no next key.
        /// </returns>
        public bool TryGetNext(TreeRef treeref, ReadOnlySpan<byte> key, out ReadOnlySpan<byte> next)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                if (cursor.MoveToNextKey())
                                {
                                    next = cursor.GetCurrentKeySpan();
                                    return true;
                                }
                            }
                            else
                            {
                                // TODO handle EOF
                                // test if on valid key, else move next
                            }
                        }

                        next = default;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Tries to get the previous key in a tree.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="key">The key.</param>
        /// <param name="next">The previous key. Only valid if the method returns true.</param>
        /// <returns>
        /// True if there is a previous key.
        /// False if there is no previous key.
        /// </returns>
        public bool TryGetPrev(TreeRef treeref, ReadOnlySpan<byte> key, out ReadOnlySpan<byte> prev)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);
                    ValidateKey(ref key);

                    try
                    {
                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(key))
                            {
                                if (cursor.MoveToPrevKey())
                                {
                                    prev = cursor.GetCurrentKeySpan();
                                    return true;
                                }
                            }
                            else
                            {
                                // TODO handle BOF
                                // test if on valid key, else move prev

                                if (cursor.MoveToPrevKey())
                                {
                                    prev = cursor.GetCurrentKeySpan();
                                    return true;
                                }

                            }
                        }

                        prev = default;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Fail(true);

                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        #endregion

        #region Overflow Pages

        internal void DeleteOverflowPages(KvPagenumber ovpageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            if (ovpageno == 0)
            {
                return;
            }

            // TODO Optimize calculate Pagecount from OverflowLength without reading the page
            using (var page = GetPage(ovpageno, true, out _))
            {
                var count = page.AsOverflowPage.Header.PageCount;

                var lastpage = ovpageno + count - 1;

                for (var i = ovpageno; i <= lastpage; i++)
                {
                    // TODO optimize
                    AddFreeOrLoosePage(i);
                }
            }
        }

        internal OverflowStream GetOverflowStream(KvPagenumber pageno, ulong length)
        {
            Perf.CallCount();

            Validate(false);

            return new OverflowStream(this, pageno, length);
        }

        /// <summary>
        /// creates and writes to the overflowpage if necessary
        /// caller must check size
        /// </summary>
        /// <param name="entry"></param>
        /// <exception cref="NotImplementedException"></exception>
        private unsafe void CreateOverflowPages(ref ValInfo val, out KvPagenumber ovpageno, out ulong ovlength)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            // get metrics
            GetOverflowMetrics(val.Length, out var pagecount, out var first, out var fullpages, out var last);

            using (var firstpage = AllocateOverflowPages(pagecount, out var range))
            {
                KvDebug.Assert(range.Contains(firstpage.PageNumber), "Pagenumber out of Range!");

                // set overflow page and length
                ovpageno = firstpage.Header.PageNumber;
                ovlength = (ulong)val.Length;

                // set content length in header
                firstpage.Header.ContentLength = ovlength;

                //
                // write first page
                //
                WriteOverflowPage(firstpage, ref val, firstpage.Bytes.Span.Slice(UniversalHeader.HeaderSize, first), false);

                //
                // write full pages
                //
                var pageno = range.First + 1;
                for (var i = 0; i < fullpages; i++, pageno++)
                {
                    KvDebug.Assert(range.Contains(pageno), "Pagenumber out of Range!");

                    using (var page = Allocator.GetPage(pageno, false, null, 0))
                    {
                        WriteOverflowPage(page, ref val, page.Bytes.Span, true);
                    }
                }

                //
                // write last page
                //
                if (last > 0)
                {
                    KvDebug.Assert(range.Contains(pageno), "Pagenumber out of Range!");

                    using (var page = Allocator.GetPage(pageno, false, null, 0))
                    {
                        WriteOverflowPage(page, ref val, page.Bytes.Span.Slice(0, last), true);
                    }
                }
            }
        }

        /// <summary>
        /// returns the metrics for storing data in overflow pages
        /// </summary>
        /// <param name="pagesize">the page size</param>
        /// <param name="firstpage">number of bytes to write in the first overflow page</param>
        /// <param name="fullpages">number of full overflow pages to write</param>
        /// <param name="lastpage">number of bytes to write in the last overflow page</param>
        internal void GetOverflowMetrics(long valuelen, out ulong pagecount, out int firstpage, out long fullpages, out int lastpage)
        {
            // calculate number of pages needed
            pagecount = ((ulong)valuelen + (PageSize - 1) + UniversalHeader.HeaderSize) / PageSize;

            var len = valuelen;

            var firstlen = (int)PageSize - UniversalHeader.HeaderSize;

            firstpage = (int)(len < firstlen ? len : firstlen);
            len -= firstpage;
            fullpages = len / PageSize;
            lastpage = (int)(len % PageSize);
        }

        internal void WriteOverflowPage(AnyPage page, ref ValInfo source, Span<byte> target, bool canspill)
        {
            Perf.CallCount();

            var bytesread = source.CopyTo(target);
            if (bytesread != target.Length)
            {
                var msg = string.Format("Not enough data in the stream (Expected: {0} Actual: {1})", target.Length, bytesread);
                throw new NotSupportedException(msg);
            }

            if (canspill && source.Length > Database.Options.ValueSpillSize)
            {
                // write directly to disk
                Pager.WritePage(this, page);
                Pages.Insert(page.PageNumber, null, PageStates.Spilled);
            }
            else
            {
                // add to dirty pages
                Pages.Insert(page.PageNumber, page, PageStates.Dirty);
            }
        }

        #endregion

        #region Page Management

        ///// <summary>
        ///// state of the pages
        ///// </summary>
        //internal KvDictionary<PageStates> _pagestates = new();

        //_pagestates.ToDictionary(x=>x.Key, x=>x.Value);

        ///// <summary>
        ///// pages that became unused (added to freespace at commit)
        ///// </summary>
        //internal KvHashSet _freepages = new();

        /// <summary>
        /// pages and their state
        /// </summary>
        internal readonly KvPageDictionary Pages = new();

        ///// <summary>
        ///// Pages that are dirty (must be written)
        ///// </summary>
        //internal KvPageDictionary _dirtypages = new(1024);

        ///// <summary>
        ///// Pages from Parent Transactions that are dirty (must be written)
        ///// </summary>
        //internal KvPageDictionary _dirtyparentpages = new(1024);

        /// <summary>
        /// dirty pages that became free, can be reused
        /// </summary>
        //internal PageRangeList _loosepages = new();

        ///// <summary>
        ///// Pages that were temporarily written
        ///// </summary>
        //internal KvHashSet _spilledpages = new();

        private void ClearPages()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            // do not clear loose pages because list is moved to parent transaction
            //_loosepages.Clear();

            // do not clear page states because list is moved to parent transaction
            //_pagestates.Clear();

            // TODO check which pagetypes should be cleared

            // important because of reference counting
            Pages.Clear();
        }

        /// <summary>
        /// if the page is dirty, it is added to loose pages
        /// if the page is not dirty it is added to free pages
        /// </summary>
        /// <param name="page"></param>
        internal void AddFreeOrLoosePage(KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            ref var past = ref Pages.TryGetValueRef(pageno, out var isvalid);
            if (!isvalid)
            {
                Pages.Insert(pageno, null, PageStates.Free);
                Logger.LogDebug(LogTopics.Allocation, Tid, "Clean page marked as free: {0}", pageno);
            }
            else
            {
                switch (past.State)
                {
                    case PageStates.Dirty:
                    case PageStates.DirtyAtParent:  // this happens when overflowpages created by parent transactions are freed                    
                    case PageStates.Spilled:

                        Pages.ChangeState(pageno, ref past, PageStates.Loose);

                        Logger.LogDebug(LogTopics.Allocation, Tid, "{0} page marked as loose: {1}", past.State, pageno);
                        break;

                    case PageStates.Loose:
                    case PageStates.Free:
                        throw new NotSupportedException(string.Format("Page is already {0}!", past.State));

                    default:
                        throw new NotSupportedException("Unhandled page state!");
                }
            }
        }

        /// <summary>
        /// gets the Page with the given number 
        /// the page is returned with incremented refcount
        /// RefCount: caller is responsible for calling Dispose
        /// </summary>
        /// <param name="pagenumber">the number of the requested page</param>
        /// <returns>the requested page</returns>
        internal AnyPage GetPage(KvPagenumber pageno, bool createheader, out PageStates state, bool unspill = true)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Read lock not held!");

            ref var past = ref Pages.TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                state = past.State;

                switch (state)
                {
                    case PageStates.Dirty:
                    case PageStates.DirtyAtParent:
                        return past.Page.AddRef();

                    case PageStates.Spilled:
                        return UnspillPage(pageno, createheader, unspill);

                    case PageStates.Loose:
                    case PageStates.Free:
                        throw new NotSupportedException(string.Format("Trying to read {0} page {1}.", past.State, pageno));
                }
            }
            else
            {
                if (Parent == null)
                {
                    state = PageStates.None;
                    return Pager.ReadPage(this, pageno, createheader);
                }
                else
                {
                    var parentpage = Parent.GetPage(pageno, createheader, out state);
                    if (state != PageStates.None)
                    {
                        if (state == PageStates.Dirty || state == PageStates.DirtyAtParent)
                        {
                            Pages.Insert(pageno, parentpage, PageStates.DirtyAtParent);
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled Pagestate!");
                        }
                    }

                    return parentpage;
                }
            }

            throw new NotSupportedException(string.Format("Trying to read unknown page {0}.", pageno));
        }

        #region Spilling

        internal void SpillCheck()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            // get number of dirty pages in all transactions
            long dirtypagecount = 0;

            var tx = Root;
            while (tx != null)
            {
                dirtypagecount += tx.Pages.DirtyPages.Count;
                tx = tx.Child;
            }

            var dirtysize = dirtypagecount * PageSize;

            // check limit
            if (dirtysize > Database.Options.SpillSize)
            {
                Spill();
            }
        }

        internal void Spill()
        {
            Perf.CallCount();

            try
            {
                // acquire lockfile and verify tx exists in lockfile and is not expired
                Database.LockFile?.LockAndVerify(this);

                // get page numbers that cannot be spilled
                // dont spill pages that are referenced in tracked cursors
                var dontspill = Database.Tracker.GetPageNumbers();

                var tx = Root;
                while (tx != null)
                {
                    // child transactions can have large values already spilled
                    // so take care to not overwrite them with pages from the parent transactions
                    dontspill.AddRange(tx.Pages.SpilledPages);

                    tx = tx.Child;
                }

                tx = Root;
                while (tx != null)
                {
                    // number of pages to spill
                    var pagestospillcount = tx.Pages.DirtyPages.Count / 2;

                    KvHashSet _tospill = new();

                    // determine pages to spill
                    // TODO find better algorithm
                    var pages = tx.Pages.GetDirtyPages(PageTypes.Raw);
                    _tospill.AddRange(pages);

                    if (_tospill.Count < pagestospillcount)
                    {
                        pages = tx.Pages.GetDirtyPages(PageTypes.DataOverflow);
                        _tospill.AddRange(pages);
                    }

                    if (_tospill.Count < pagestospillcount)
                    {
                        pages = tx.Pages.GetDirtyPages(PageTypes.DataLeaf);
                        _tospill.AddRange(pages);
                    }

                    if (_tospill.Count < pagestospillcount)
                    {
                        pages = tx.Pages.GetDirtyPages(PageTypes.DataIndex);
                        _tospill.AddRange(pages);
                    }

                    // remove unspillable pages
                    _tospill.RemoveRange(dontspill);

                    // spill the pages
                    _tospill.ForEach(pageno =>
                    {
                        Pager.WritePage(tx, Pages.GetPage(pageno));
                        tx.Pages.ChangeState(pageno, PageStates.Spilled);
                    });

                    // do not spill the same page again in child transaction
                    // (otherwise rollback of a child transaction may corrupt the database)                
                    dontspill.AddRange(_tospill);

                    tx = tx.Child;
                }

                // flush to disk
                // TODO check if necessary (probably not)
                Pager.Flush();
            }
            finally
            {
                Database.LockFile?.Unlock();
            }
        }

        // TODO what does unspill mean
        internal AnyPage UnspillPage(KvPagenumber pageno, bool createheader, bool unspill)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Read lock not held!");

            KvDebug.Assert(Pages.SpilledPages.Contains(pageno), "Not a spilled page!");

            var page = Pager.ReadPage(this, pageno, createheader, unspill);
            if (unspill)
            {
                Pages.Update(pageno, page, PageStates.Dirty);
            }

            return page;
        }

        #endregion

        /// <summary>
        /// allocates an empty page and adds it to _dirtypages
        /// </summary>
        /// <param name="pagetype"></param>
        /// <returns></returns>
        internal AnyPage AllocatePage(ushort pagetype, AnyPage copyfrom)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            KvPagenumber? pageno;

            // look for loose pages first
            pageno = Pages.TakeLoosePage();

            if (pageno == null)
            {
                // look for free pages
                pageno = GetFreePage(pagetype);
                if (pageno == null)
                {
                    // take from file end
                    pageno = AllocatePage(pagetype);

                    if (pageno == null)
                    {
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, "Could not allocate a page!");
                    }
                    else
                    {
                        Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated page {0} from file end.", pageno.Value);
                    }
                }
                else
                {
                    Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated page {0} from free space.", pageno.Value);
                }
            }
            else
            {
                Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated page {0} from loose pages.", pageno.Value);
            }

            AnyPage page = null;

            try
            {
                if (copyfrom == null)
                {
                    // create page
                    page = Allocator.GetPage(pageno.Value, pagetype != PageTypes.Raw, pagetype, 0);
                    page.InitContentHeader(Tid);

                    KvDebug.Assert(page.PageType == page.Header.PageType, "FAIL");
                }
                else
                {
                    page = Allocator.GetCopy(copyfrom, pageno.Value, Tid);
                }

                // add to dirty pages
                Pages.Insert(pageno.Value, page, PageStates.Dirty);

                return page;
            }
            finally
            {
                page.Dispose();
            }
        }

        /// <summary>
        /// allocates a new page from Fileend
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        internal KvPagenumber? AllocatePage(ushort pagetype)
        {
            Perf.CallCount();

            switch (pagetype)
            {
                case PageTypes.DataIndex:
                case PageTypes.DataLeaf:
                case PageTypes.DataOverflow:
                case PageTypes.FsIndex:
                case PageTypes.FsLeaf:
                case PageTypes.Raw:

                    // TODO check MaxLastPage
                    return ++Meta.LastPage;

                default:
                    throw new NotSupportedException("PageType cannot be allocated within transaction.");
            }
        }

        internal PageRange AllocateOverflowPagesByCount(ulong count)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            var ret = new PageRange(Meta.LastPage + 1, Meta.LastPage + count);

            Meta.LastPage += count;

            // TODO check MaxLastPage

            return ret;
        }

        internal AnyPage AllocateOverflowPages(ulong pagecount, out PageRange range)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            var rangefound = false;

            // look for loose pages first
            if (Parent != null && pagecount * PageSize > (ulong)Database.Options.ValueSpillSize)
            {
                // values larger than ValueSpillSize will be written directly to disk
                // so do not allocate pages that are marked as spilled in any parent transaction
                // (otherwise rollback may corrupt the database)

                KvPagenumber start = 0;

                while (true)
                {
                    rangefound = Pages.TryTakeLooseRange(start, pagecount, out range);

                    // TODO test if check needed
                    if (!rangefound || !ContainsSpilledPages(range))
                    {
                        break;
                    }

                    start = range.Last + 1;
                }
            }
            else
            {
                rangefound = Pages.TryTakeLooseRange(0, pagecount, out range);
            }

            if (!rangefound)
            {
                // look for free pages
                range = GetFreePages(PageTypes.DataOverflow, pagecount);

                if (range.IsEmpty)
                {
                    // take from file end
                    range = AllocateOverflowPagesByCount(pagecount);

                    if (range.IsEmpty)
                    {
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, "Could not allocate pagerange!");
                    }
                    else
                    {
                        Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated pagerange {0} from file end.", range);
                    }
                }
                else
                {
                    Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated pagerange {0} from free pages.", range);
                }
            }
            else
            {
                Logger.LogDebug(LogTopics.Allocation, Tid, "Allocated pagerange {0} from loose pages.", range);
            }

            // allocate first page with header
            var ovpage = Allocator.GetPage(range.First, true, PageTypes.DataOverflow, 0);
            ovpage.InitContentHeader(Tid);

            return ovpage;
        }

        private bool ContainsSpilledPages(PageRange range)
        {
            Perf.CallCount();

            var tx = Parent;
            while (tx != null)
            {
                for (KvPagenumber pageno = range.First; pageno <= range.Last; pageno++)
                {
                    if (tx.Pages.SpilledPages.Contains(pageno))
                    {
                        return true;
                    }
                }

                tx = tx.Parent;
            }

            return false;
        }

        /// <summary>
        /// make sure the given page is dirty
        /// if the page is already dirty the method does nothing
        /// otherwise a copy is made, the original page is added to freepages
        /// if page is already dirty its reference count will be incremented
        /// RefCount: caller is responsible for disposing the returned page
        /// </summary>
        /// <param name="page">the dirty page</param>
        /// <returns></returns>
        internal AnyPage EnsureDirtyPage(Cursor cursor, AnyPage page, bool disposeoriginal)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            AnyPage newpage;

            ref var past = ref Pages.TryGetValueRef(page.PageNumber, out var isvalid);
            if (isvalid)
            {
                switch (past.State)
                {
                    case PageStates.Dirty:

                        // TODO check early return (skip Dispose. does it make sense to dispose the same page? )
                        newpage = page;
                        break;

                    case PageStates.DirtyAtParent:
                        // make a copy of the page with the same pagenumber
                        // if it is from a parent transaction
                        using (newpage = Allocator.GetCopy(page))
                        {
                            Pages.Update(page.PageNumber, newpage, PageStates.Dirty);
                            ATODirtyParentPage(cursor, newpage);
                        }
                        break;

                    default:
                        throw new NotSupportedException(string.Format("A {0} page cannot be made dirty.", past.State));
                }
            }
            else
            {
                // make a copy of the page with a new pagenumber
                newpage = AllocatePage(page.PageType, page);

                ATODirty(cursor, page, newpage);

                KvDebug.Assert(newpage.PageNumber != page.PageNumber, "Same page!");

                // add old page to free pages
                AddFreeOrLoosePage(page.PageNumber);
            }

            if (disposeoriginal)
            {
                page.Dispose();
            }

            return newpage;
        }

        #endregion

        #region Commit, Rollback, Abort and supporting functions

        private bool HasChanges()
        {
            Perf.CallCount();

            return Pages.DirtyPages.Count > 0 || Pages.SpilledPages.Count > 0 || !Pages.LoosePages.IsEmpty ||
                   Pages.FreePages.Count > 0 || !_fstouchedranges.IsEmpty || _fstodelete.Count > 0;
        }

        public void Commit()
        {
            Perf.CallCount();

            lock (TxLock)
            {
                CommitInternal();
            }
        }

        internal void CommitInternal()
        {
            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            Perf.CallCount();

            try
            {
                Logger.LogInfo(LogTopics.Transaction, Tid, "Committing Transaction...");

                Validate(!IsReadOnly);

                if (Parent == null)
                {
                    CommitToDisk();
                }
                else
                {
                    CommitToParent();
                }

                ClearPages();

                State = TransactionStates.Committed;

                Logger.LogInfo(LogTopics.Transaction, Tid, "Transaction committed successfully.");
            }
            catch (Exception ex)
            {
                State = TransactionStates.Failed;

                Logger.LogError(LogTopics.Transaction, Tid, ex);
                throw;
            }
        }

        private void CommitToParent()
        {
            Perf.CallCount();

            if (!IsReadOnly)
            {
                //
                // commit to parent transaction
                //

                // update Meta
                // TODO check if Meta instance can be replaced
                Parent.Meta.DataRootPage = Meta.DataRootPage;
                Parent.Meta.FsRootPage = Meta.FsRootPage;
                Parent.Meta.LastPage = Meta.LastPage;
                Parent.Meta.DataTotalCount = Meta.DataTotalCount;
                Parent.Meta.DataLocalCount = Meta.DataLocalCount;
                Parent.Meta.FsTotalCount = Meta.FsTotalCount;
                Parent.Meta.FsLocalCount = Meta.FsLocalCount;

                Parent.Pages.UpdateWith(Pages);

                UpdateParentFreeSpace();

                Parent.Version++;
            }

            Database.Tracker.OnCommitChildTransaction(this);
        }

        private void CommitToDisk()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            try
            {
                // acquire lockfile and verify tx exists in lockfile and is not expired
                Database.LockFile?.LockAndVerify(this);

                var haschanges = HasChanges();

                if (!IsReadOnly)
                {
                    if (haschanges)
                    {
                        StoreFreeSpace();

                        // set filesize instead of simply appending each page
                        // this will hopefully reduce file fragmentation
                        // TODO do this too when writing spilled pages
                        // check if MaxLastPage must be used (no)
                        if (Meta.LastPage > Meta.SourceLastPage)
                        {
                            Database.SetFilesize((long)(Meta.LastPage + 1) * PageSize);
                        }

                        void WritePage(KvPagenumber pageno)
                        {
                            Pager.WritePage(this, Pages.GetPage(pageno));
                        }

                        Pages.DirtyPages.ForEach(WritePage);

                        // flush unwritten data                        
                        Pager.Flush();

                        // Update Meta and flush database file
                        Database.SaveMeta(this, Meta);

                        // flush unwritten data                        
                        Pager.Flush();

                        // update filesize
                        Database.UpdateFilesize();

                        Pager.CommitWriteCache(Meta.SourceTid, Meta.Tid);
                    }
                }

                Database.Tracker.OnCommitRootTransaction(this, haschanges ? Tid : Meta.SourceTid);
            }
            catch (Exception ex)
            {
                // TODO Error handling
                // rollback in case of error and mark as failed
                throw;
            }
            finally
            {
                // TODO check if self-disposing is needed
                Database.LockFile?.RemoveAndUnlock(this);
            }
        }

        public void Rollback()
        {
            Perf.CallCount();

            lock (TxLock)
            {
                RollbackInternal();
            }
        }

        internal void Fail(bool rollback)
        {
            Perf.CallCount();

            if (rollback)
            {
                RollbackInternal();
            }

            State = TransactionStates.Failed;
        }

        private void RollbackInternal()
        {
            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            Perf.CallCount();

            try
            {
                Logger.LogInfo(LogTopics.Transaction, Tid, "Rolling back Transaction...");

                Validate(!IsReadOnly);

                if (Root == this)
                {
                    // acquire lockfile and verify tx exists in lockfile and is not expired
                    Database.LockFile?.LockAndVerify(this);
                }

                if (Root == this)
                {
                    //
                    // rollback to disk
                    //
                    Database.Tracker.OnRollbackRootTransaction(this);
                }
                else
                {
                    //
                    // rollback to parent transaction
                    //
                    Database.Tracker.OnRollbackChildTransaction(this);
                }

                ClearPages();

                Pager.ClearWriteCache();

                State = TransactionStates.RolledBack;

                Logger.LogInfo(LogTopics.Transaction, Tid, "Transaction rolled back successfully.");
            }
            catch (Exception ex)
            {
                State = TransactionStates.Failed;

                Logger.LogError(LogTopics.Transaction, Tid, ex);
                throw;
            }
            finally
            {
                if (Root == this)
                {
                    Database.LockFile?.RemoveAndUnlock(this);
                }
            }
        }

        #endregion

        #region Tree References

        /// <summary>
        /// Returns a reference to a sub tree. 
        /// Throws an execption if one of the keys does not exist or does not have the subtree-flag.
        /// </summary>
        /// <param name="scope">The tracking scope.</param>
        /// <param name="keys">Path to the sub tree.</param>
        /// <returns>Reference to the sub tree.</returns>
        public TreeRef GetTreeRef(TrackingScope scope, params ReadOnlyMemory<byte>[] keys)
        {
            Perf.CallCount();

            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            lock (TxLock)
            {
                TreeRef treeref = null;

                try
                {
                    Validate(false);

                    foreach (var key in keys)
                    {
                        var keyspan = key.Span;

                        ValidateKey(ref keyspan);

                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(keyspan))
                            {
                                if (cursor.CurrentHasSubtreeFlag())
                                {
                                    treeref ??= new TreeRef(this, scope);
                                    treeref.AddNodes(cursor.CurrentPath);
                                }
                                else
                                {
                                    throw new KeyValiumException(ErrorCodes.KeyNotFound, "The key does not have a subtree.");
                                }
                            }
                            else
                            {
                                throw new KeyValiumException(ErrorCodes.KeyNotFound, "The key does not exist.");
                            }
                        }
                    }

                    treeref.CopyKeys(keys);

                    return treeref;
                }
                catch (Exception ex)
                {
                    treeref?.Dispose();

                    Logger.LogError(LogTopics.Cursor, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns a reference to a sub tree. Nonexisting keys will be created. Missing subtree flags are set.
        /// </summary>
        /// <param name="scope">The tracking scope.</param>
        /// <param name="keys">Path to the sub tree.</param>
        /// <returns>Reference to the sub tree.</returns>
        public TreeRef EnsureTreeRef(TrackingScope scope, params ReadOnlyMemory<byte>[] keys)
        {
            Perf.CallCount();

            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            lock (TxLock)
            {
                TreeRef treeref = null;

                try
                {
                    Validate(true);

                    foreach (var key in keys)
                    {
                        var keyspan = key.Span;

                        ValidateKey(ref keyspan);

                        using (var cursor = GetDataCursor(treeref))
                        {
                            if (cursor.SetPosition(keyspan))
                            {
                                if (!cursor.CurrentHasSubtreeFlag())
                                {
                                    // update subtree-flag
                                    var entry = cursor.GetCurrentLeafEntry();
                                    entry = new EntryExtern(entry.Key, entry.Value, 0, 0, 0, entry.OverflowPageNumber, entry.OverflowLength);

                                    cursor.UpdateKey(ref entry);

                                    Version++;

                                    // TODO Test cursor positioning
                                }
                            }
                            else
                            {
                                var valinfo = new ValInfo();
                                // create Key with subtree 0
                                var entry = new EntryExtern(keyspan, valinfo, 0, 0, 0, 0, 0);
                                cursor.InsertKey(ref entry);

                                Version++;

                                // TODO Test cursor positioning
                            }

                            // key should have subtree flag set here
                            // TODO Test
                            treeref ??= new TreeRef(this, scope);

                            treeref.AddNodes(cursor.CurrentPath);
                        }
                    }

                    treeref.CopyKeys(keys);

                    return treeref;
                }
                catch (Exception ex)
                {
                    treeref?.Dispose();

                    Logger.LogError(LogTopics.Cursor, Tid, ex);
                    throw;
                }
            }
        }

        #endregion

        #region Iterators

        /// <summary>
        /// Iterates over the tree pointed to by treeref. Calls func for every key value pair.
        /// If func returns false the iteration is aborted. Otheriwse it continues.
        /// The function func is always called with the same KvItem instance. Use KvItem.Value to get the current ValueRef.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="func">The function that is called for every item.</param>
        public void ForEach(TreeRef treeref, Func<KvItem, bool> func)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    using (var cursor = GetCursor(treeref, InternalTrackingScope.TransactionChain))
                    {
                        cursor.DeleteHandling = DeleteHandling.MoveToPrevious;

                        var item = new KvItem(cursor);

                        if (cursor.SetPosition(CursorPositions.BeforeFirst))
                        {
                            while (cursor.MoveToNextKey() && func(item))
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Delegate for iteration.
        /// </summary>
        /// <param name="val">The reference to the current entry.</param>
        /// <returns></returns>
        public delegate bool KeyValueIterator(ref ValueRef val);

        /// <summary>
        /// Iterates over the tree pointed to by treeref. Calls func for every key value pair.
        /// If func returns false the iteration is aborted. Otheriwse it continues.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="func">Function that is called for every item.</param>
        public void ForEach(TreeRef treeref, KeyValueIterator func)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    using (var cursor = GetCursor(treeref, InternalTrackingScope.TransactionChain))
                    {
                        cursor.DeleteHandling = DeleteHandling.MoveToPrevious;

                        if (cursor.SetPosition(CursorPositions.BeforeFirst))
                        {
                            while (cursor.MoveToNextKey())
                            {
                                var val = cursor.GetCurrentValue();
                                if (!func(ref val))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns an iterator that can be used in a foreach loop.
        /// </summary>
        /// <param name="treeref">The reference to the subtree or null for the root tree.</param>
        /// <param name="forward">If true iterates forward. Otherwise backwards.</param>
        public KeyIterator GetIterator(TreeRef treeref, bool forward)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                try
                {
                    Validate(false);
                    treeref?.Validate(this);

                    return new KeyIterator(this, treeref, forward);
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogTopics.DataAccess, Tid, ex);
                    throw;
                }
            }
        }

        #endregion

        #region Freespace Management

        /// <summary>
        /// minimum number of free space entries to be reserved in one call
        /// </summary>
        private const int MinReservedFsEntries = 16;

        /// <summary>
        /// maximum number of free space entries to be reserved in one call
        /// </summary>
        public const int MaxReservedFsEntries = 4096;

        /// <summary>
        /// current number of free space entries to be reserved in one call
        /// will be doubled in every call until Maximum is reached
        /// </summary>
        private int _fsentriestoreserve = MinReservedFsEntries;

        /// <summary>
        /// will be set to true if no more freespace entries are available
        /// </summary>
        private bool _fsexhausted = false;

        /// <summary>
        /// untouched freespace entries, ordered by first pagenumber
        /// </summary>
        private SortedList<KvPagenumber, FsEntryExtern> _fsreserved = new(256);

        /// <summary>
        /// touched freespace entries that need to be deleted later
        /// </summary>
        private SortedSet<KvPagenumber> _fstodelete = new();

        /// <summary>
        /// all available freespace 
        /// </summary>
        private PageRangeList _fsreservedranges = new();

        /// <summary>
        /// freespace entries that have not been completely used yet
        /// </summary>
        private PageRangeList _fstouchedranges = new();

        private KvPagenumber? GetFreePage(ushort pagetype)
        {
            Perf.CallCount();

            if (_fsexhausted && _fsreservedranges.RangeCount == 0)
            {
                return null;
            }

            var range = GetFreePages(pagetype, 1);
            if (!range.IsEmpty)
            {
                Logger.LogDebug(LogTopics.Freespace, Tid, "Got FreePage: {0}", range.First);

                return range.First;
            }
            return null;
        }
        private PageRange GetFreePages(ushort pagetype, ulong count)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            var range = GetFreePagesFromRanges(count);

            // Do not scan freespace tree when the requested pagetype is FsIndex. That happens when freespace runs out
            // while touching the freespace cursor. This is necessary to avoid scanning an inconsistent tree.
            // Example: while a free space cursor is touched. Cursors are touched from bottom to top.
            // The leaf page might be dirtied with the last loose page. The index page above is not dirtied yet.
            // Scanning the freespace in this state will result in the error "Trying to read free page." because
            // the index page will point to an already freed page.
            // Example: while a (possibly cascading) split or merge is in progress.
            if (pagetype != PageTypes.FsIndex)
            {
                while (range.IsEmpty && !_fsexhausted)
                {
                    // adjust entrycount according to count
                    while (count > (ulong)_fsentriestoreserve && _fsentriestoreserve < MaxReservedFsEntries)
                    {
                        _fsentriestoreserve <<= 1;
                    }

                    if (ReserveFreespace())
                    {
                        // try again
                        range = GetFreePagesFromRanges(count);
                    }
                }
            }

            return range;
        }

        /// <summary>
        /// the last reserved freespace entry
        /// </summary>
        private FsEntryExtern? _fslastreserved = null;

        /// <summary>
        /// reads up to MaxReservedFreespaceEntries freespace entries and puts them into the reserved list
        /// </summary>
        /// <returns>true, if new freespace has been reserved</returns>
        private bool ReserveFreespace()
        {
            Perf.CallCount();

            if (_fsexhausted) // || _fsreserved.Count > MaxReservedFsEntries >> 1)
            {
                // do nothing if freespace is exhausted 
                return false;
            }

            Logger.LogDebug(LogTopics.Freespace, Tid, "Scanning FsEntries...");

            var found = false;
            FsEntryExtern? last = null;

            using (var cursor = GetFsCursor1())
            {
                var span = _fslastreserved == null ? ReadOnlySpan<byte>.Empty : new EntryExtern(_fslastreserved.Value.FirstPage, _fslastreserved.Value.LastPage, _fslastreserved.Value.Tid).Key;

                var result = _fslastreserved == null ?
                    cursor.SetPosition(CursorPositions.BeforeFirst) :
                    cursor.SetPosition(span);

                if (result)
                {
                    var count = 0;

                    while (cursor.MoveToNextKey() && count < _fsentriestoreserve) // && _fsreserved.Count < _fsentriestoreserve)
                    {
                        var entry = cursor.GetCurrentLeafEntry();
                        if (entry.Tid < Meta.MinTid)
                        {
                            Logger.LogDebug(LogTopics.Freespace, Tid, "FsEntry read: {0}[{1}-{2}]", entry.Tid, entry.FirstPage, entry.LastPage);

                            if (!_fsreserved.ContainsKey(entry.FirstPage) && !_fstodelete.Contains(entry.FirstPage))
                            {
                                found = true;
                                var fsentry = new FsEntryExtern(entry.FirstPage, entry.LastPage, entry.Tid);
                                _fsreserved.Add(entry.FirstPage, fsentry);
                                _fsreservedranges.AddRange(entry.FirstPage, entry.LastPage);

                                last = fsentry;
                                count++;
                            }
                            else
                            {
                                throw new KeyValiumException(ErrorCodes.InternalError, "Freespace entry read twice.");
                            }
                        }
                    }
                }
            }

            if (last != null)
            {
                // make a copy of the last scanned entry because it might be changed
                _fslastreserved = last;
            }

            if (!found)
            {
                Logger.LogDebug(LogTopics.Freespace, Tid, "FreeSpace is exhausted.");
                _fsexhausted = true;
            }

            // double entrycount for next call
            if (_fsentriestoreserve < MaxReservedFsEntries)
            {
                _fsentriestoreserve <<= 1;
            }

            Logger.LogDebug(LogTopics.Freespace, Tid, "Scanning FsEntries done.");

            return found;
        }

        /// <summary>
        /// tries to get a free page from the already touched ranges
        /// </summary>
        /// <returns></returns>
        //private KvPagenumber? GetFreePageFromRanges()
        //{
        //    var range = GetFreePagesFromRanges(1);
        //    if (range != null)
        //    {
        //        return range.First;
        //    }

        //    return null;
        //}

        /// <summary>
        /// tries to get a free page from the already touched ranges
        /// </summary>
        /// <returns></returns>
        private PageRange GetFreePagesFromRanges(ulong count)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            //
            // look in _fsranges
            //
            if (_fsreservedranges.TryTakeRange(0, count, out var ret))
            {
                for (KvPagenumber page = ret.First; page <= ret.Last; page++)
                {
                    // remove from reserved and mark as to be deleted
                    if (_fsreserved.ContainsKey(page))
                    {
                        // move entry to deletees
                        _fsreserved.Remove(page, out var entry);

                        //KvDebug.Assert(entry != null, "Entry not found!");

                        _fstodelete.Add(page);

                        // check if entry is completely used
                        if (entry.LastPage > ret.Last)
                        {
                            // if not then save remaining range
                            _fstouchedranges.AddRange(ret.Last + 1, entry.LastPage);
                        }

                        // TODO test
                        //page += entry.PageCount - 1;
                    }

                    // TODO make faster
                    _fstouchedranges.RemovePageIfExists(page);
                }

                Logger.LogDebug(LogTopics.Freespace, Tid, "Got FreePagesFromRange: [{0}-{1}]", ret.First, ret.Last);
            }

            return ret;
        }

        private void StoreFreeSpace()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            Logger.LogInfo(LogTopics.Freespace, Tid, "Storing free space...");

            //
            // delete freespace at fileend
            //
            if (_fsreservedranges.TryGetLast(out var lastrange) && lastrange.Last == Meta.LastPage)
            {
                _fsreservedranges.RemoveRange(lastrange);

                // TODO unify with same algorithm as GetFreePagesFromRanges()
                for (KvPagenumber page = lastrange.First; page <= lastrange.Last; page++)
                {
                    // remove from reserved and mark as to be deleted
                    if (_fsreserved.ContainsKey(page))
                    {
                        // move entry to deletees
                        _fsreserved.Remove(page);

                        //KvDebug.Assert(entry != null, "Entry not found!");

                        _fstodelete.Add(page);

                        // TODO test
                        //page += entry.PageCount - 1;
                    }

                    // TODO make faster
                    _fstouchedranges.RemovePageIfExists(page);
                }

                Meta.LastPage = lastrange.First - 1;

                // remove pages from cache
                Pager.RemoveCachedPages(this, lastrange);

                Logger.LogInfo(LogTopics.Freespace, Tid, "Deleted free space at file end: {0}", lastrange);
            }

            var cycle = 1;

            //
            // loop until things settle down 
            // adding and removing freespace entries may create additional free and/or loose pages
            //
            while (Pages.FreePages.Count > 0 || !Pages.LoosePages.IsEmpty ||
                   _fstodelete.Count > 0 || !_fstouchedranges.IsEmpty)
            {
                //
                // move relevant entries to a copy to strictly separate cycles and avoid overlap
                //
                var todelete = _fstodelete;
                _fstodelete = new SortedSet<ulong>();
                var touchedranges = _fstouchedranges.ToList();
                _fstouchedranges = new PageRangeList();
                var freeranges = Pages.RemoveFreeAndLoosePages().ToList();

                //
                // remove touched entries from reserved ranges to avoid allocating a page twice
                // (must be done first)
                //
                Logger.LogDebug(LogTopics.Freespace, Tid, "Cycle {0}: Removing touched entries from reserved ranges...", cycle);

                foreach (var range in touchedranges)
                {
                    _fsreservedranges.RemoveRange(range);
                }

                //
                // delete used freespaceentries
                //
                Logger.LogDebug(LogTopics.Freespace, Tid, "Cycle {0}: Deleting used entries...", cycle);

                foreach (var pageno in todelete)
                {
                    // pageno is the key for fsentries, so no need to provide lastpage and tid for deletion
                    var entryex = new EntryExtern(pageno, 0, 0);
                    DeleteFreeSpaceEntry(ref entryex);
                }

                //
                // save touched free space entries
                //
                Logger.LogDebug(LogTopics.Freespace, Tid, "Cycle {0}: Saving touched entries...", cycle);

                foreach (var range in touchedranges)
                {
                    // set Tid to 0 on touched entries
                    StoreFreePages(0, range);
                }

                //
                // save free and loose pages
                //
                Logger.LogDebug(LogTopics.Freespace, Tid, "Cycle {0}: Saving loose and free pages...", cycle);

                foreach (var range in freeranges)
                {
                    // store in freelist
                    StoreFreePages(Tid, range);
                }

                cycle++;
            }

            Logger.LogInfo(LogTopics.Freespace, Tid, "Storing free space finished.");
        }

        private void StoreFreePages(KvTid tid, PageRange range)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            var fsext = new EntryExtern(range.First, range.Last, tid);

            InsertFreeSpaceEntry(ref fsext);
        }

        /// <summary>
        /// Inserts the key. Will fail if key already exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <exception cref="NotSupportedException"></exception>
        private void InsertFreeSpaceEntry(ref EntryExtern fsentry)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            try
            {
                Validate(true);

                //TODO
                //ValidateKeyValue(key, val);

                using (var cursor = GetFsCursor2())
                {
                    Logger.LogDebug(LogTopics.Freespace, Tid, "Inserting FsEntry: [{0}-{1}] {2}", fsentry.FirstPage, fsentry.LastPage, fsentry.Tid);

                    if (cursor.SetPosition(fsentry.Key))
                    {
                        throw new NotSupportedException("FreeSpaceEntry already exists.");
                    }

                    //Console.WriteLine(cursor.Dump());

                    cursor.InsertKey(ref fsentry);

                    KvDebug.ValidateCursor2(this, cursor, null, fsentry.Key, cursor.DeleteHandling);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Freespace, Tid, ex);
                throw;
            }
        }

        private bool DeleteFreeSpaceEntry(ref EntryExtern fsentry)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            try
            {
                Validate(true);

                using (var cursor = GetFsCursor2())
                {
                    Logger.LogDebug(LogTopics.Freespace, Tid, "Deleting FsEntry: [{0}-{1}] {2}", fsentry.FirstPage, fsentry.LastPage, fsentry.Tid);

                    if (cursor.SetPosition(fsentry.Key))
                    {
                        var ret = cursor.DeleteKey();

                        //Logger.LogDebug(LogTopics.Freespace, Tid, "Deleting FsEntry: [{0}-{1}] {2} ({3})", fsentry.FirstPage, fsentry.LastPage, fsentry.Tid, ret);

                        KvDebug.ValidateCursor2(this, cursor, null, fsentry.Key, cursor.DeleteHandling);

                        return ret;
                    }
                    else
                    {
                        var msg = string.Format("Freespace entry to be deleted not found: {0}[{1}-{2}]", fsentry.Tid, fsentry.FirstPage, fsentry.LastPage);
                        throw new ArgumentException(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Freespace, Tid, ex);
                throw;
            }
        }

        #endregion

        #region Adjusting tracked objects

        internal void ATODirty(Cursor excluded, AnyPage oldpage, AnyPage newpage)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATODirty");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustDirty(oldpage, newpage));

            Database.Tracker.ATODirty(this, excluded, oldpage, newpage);
        }

        internal void ATODirtyParentPage(Cursor excluded, AnyPage newpage)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATODirtyParentPage");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustDirtyParentPage(newpage));

            Database.Tracker.ATODirtyParentPage(this, excluded, newpage);
        }

        internal void ATOInsertKey(Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATOInsertKey pageno={0} keyindex={1}", pageno, keyindex);

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustInsertKey(pageno, keyindex));

            Database.Tracker.ATOInsertKey(this, excluded, pageno, keyindex);
        }

        internal void ATOUpdateKey(Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATOUpdateKey");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustUpdateKey(pageno, keyindex));

            Database.Tracker.ATOUpdateKey(this, excluded, pageno, keyindex);
        }

        internal void ATODeleteKey(Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATODeleteKey");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustDeleteKey(pageno, keyindex));

            Database.Tracker.ATODeleteKey(this, excluded, pageno, keyindex);
        }

        internal void ATOSplit(Cursor excluded, KvPagenumber leftpageno, KvPagenumber rightpageno, ushort splitindex)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATOSplit");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustSplit(leftpageno, rightpageno, splitindex));

            Database.Tracker.ATOSplit(this, excluded, leftpageno, rightpageno, splitindex);
        }

        internal void ATOMerge(Cursor excluded, KvPagenumber targetpageno, KvPagenumber mergeepageno, ushort targetkeycountbefore)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATOMerge");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustMerge(targetpageno, mergeepageno, targetkeycountbefore));

            Database.Tracker.ATOMerge(this, excluded, targetpageno, mergeepageno, targetkeycountbefore);
        }

        internal void ATODeletePage(Cursor excluded, KvPagenumber pageno)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATODeletePage");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustDeletePage(pageno));

            Database.Tracker.ATODeletePage(this, excluded, pageno);
        }

        internal void ATODeleteTree(Cursor excluded, KvPagenumber pageno)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATODeleteTree");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustDeleteTree(pageno));

            Database.Tracker.ATODeleteTree(this, excluded, pageno);
        }

        internal void ATOInsertPage(Cursor excluded, KvPagenumber leftpageno, KvPagenumber rightpageno, KvPagenumber newpageno)
        {
            Perf.CallCount();

            Logger.LogDebug(LogTopics.Tracking, Tid, "ATOInsertPage");

            //Database.Tracker.AdjustCursors(this, excluded, x => x.AdjustInsertPage(leftpageno, rightpageno, newpageno));

            Database.Tracker.ATOInsertPage(this, excluded, leftpageno, rightpageno, newpageno);
        }

        #endregion

        #region IDisposable

        private volatile bool _isdisposed;

        private void Dispose(bool disposing, bool removefromdb = true)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(TxLock), "Write lock not held!");

            if (!_isdisposed)
            {
                if (disposing)
                {
                    Child?.DisposeInternal(removefromdb);

                    if (State == TransactionStates.Active)
                    {
                        Rollback();
                    }

                    if (Root == this)
                    {
                        if (removefromdb)
                        {
                            Database.EndTransaction(this);
                        }
                    }
                    else
                    {
                        // remove from Parent
                        Parent.Child = null;
                        Parent = null;
                    }

                    DataCursor.DisposeReal();
                    FsCursor1?.DisposeReal();
                    FsCursor2?.DisposeReal();
                }

                State = TransactionStates.Disposed;
                _isdisposed = true;
            }
        }

        private void DisposeInternal(bool removefromdb)
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true, removefromdb);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool removefromdb)
        {
            Perf.CallCount();

            lock (TxLock)
            {
                DisposeInternal(removefromdb);
            }
        }

        public void Dispose()
        {
            Perf.CallCount();

            Dispose(true);
        }

        #endregion
    }
}

