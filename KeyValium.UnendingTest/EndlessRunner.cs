using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Cursors;
using KeyValium.Inspector;
using KeyValium.Iterators;
using KeyValium.Pages;
using KeyValium.TestBench;
using KeyValium.TestBench.ActionProviders;
using KeyValium.TestBench.Helpers;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace KeyValium.UnendingTest
{
    internal class EndlessRunner
    {
        //const bool DoVerifyShadow = false;
        //const bool DoVerifyCursors = false;
        //const bool DoVerifyDatabase = false;
        //const bool DoDeleteOnSuccess = false;

        const bool DoVerifyShadow = true;
        const bool DoVerifyCursors = true;
        const bool DoVerifyDatabase = true;
        const bool DoDeleteOnSuccess = false;
        const bool DoVerifyCache = true;
        const bool DoVerifyRefCount = true;
        const bool DoVerifyPageStates = true;
        const bool DoShowAdditionalStats = true;

        public EndlessRunner()
        {
        }

        #region Properties

        private Stats Stats;

        private ShadowTree Shadow;

        //private Random _rnd;

        private LogFile ActionLog;

        private TestDescription Description;

        #endregion

        #region Run

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Run(ActionProvider provider)
        {
            Description = provider.Description;
            ActionLog = provider is LogActionProvider ? null : new LogFile(Description.DbFilename + ".actions");
            Stats = new Stats();

            var showstatsevery = 10;

            //Shadow = new ShadowDict();
            Shadow = new ShadowTree();

            var haserrors = false;

            DeleteFiles();

            using (var db = Database.Open(Description.DbFilename, Description.Options))
            {
                Transaction roottx = null;
                Transaction currenttx = null;

                try
                {
                    foreach (var action in provider.GetActions())
                    {
                        OnBeforeAction(action, currenttx);

                        switch (action.Type)
                        {
                            case ActionType.BeginTx:
                                ActionLog?.LogAction(ActionType.BeginTx, null, null, null);
                                roottx = db.BeginWriteTransaction();
                                currenttx = roottx;
                                Stats.TxCount++;
                                if (Stats.TxCount % showstatsevery == 0)
                                {
                                    ShowStats(db, "After BeginTx");
                                }

                                if (DoVerifyCache)
                                {
                                    DoTimed(nameof(VerifyCacheAllPages), () => VerifyCacheAllPages(db, roottx));
                                }

                                if (DoVerifyDatabase)
                                {
                                    DoTimed(nameof(VerifyDatabase), () => VerifyDatabase(db, roottx));
                                }

                                if (DoVerifyRefCount)
                                {
                                    DoTimed(nameof(VerifyRefCount), () => VerifyRefCount(db, roottx));
                                }

                                break;

                            case ActionType.CommitTx:

                                //if (Stats.TxCount % showstatsevery == 0)
                                //{
                                //    ShowStats(db, "Before CommitTx");
                                //}

                                ActionLog?.LogAction(ActionType.CommitTx, currenttx.Tid, null, null);
                                if (currenttx != roottx)
                                {
                                    throw new NotSupportedException("Transaction mismatch!");
                                }

                                if (DoVerifyShadow)
                                {
                                    try
                                    {
                                        DoTimed(nameof(VerifyShadow), () => VerifyShadow(currenttx, Shadow.Current));
                                    }
                                    catch (Exception ex)
                                    {
                                        DumpPages(currenttx);
                                        // commit transaction in case of errors to examine it
                                        currenttx.Commit();
                                        throw;
                                    }
                                }

                                if (DoVerifyPageStates)
                                {
                                    DoTimed(nameof(VerifyPageStates), () => VerifyPageStates(currenttx));
                                }

                                var tid = currenttx.Tid;

                                currenttx.Commit();
                                currenttx.Dispose();
                                currenttx = null;
                                roottx = null;
                                Shadow.ClearCursors();

                                BackupDatabase(db, tid, 16, 1);

                                break;

                            case ActionType.Delete:
                                DeleteNumber(currenttx, action.Key);
                                break;

                            case ActionType.Exists:
                                ExistsNumber(currenttx, action.Key);
                                break;

                            case ActionType.Get:
                                GetNumber(currenttx, action.Key);
                                break;

                            case ActionType.GetNext:
                                //GetNextNumber(stats, currenttx,keynumber);
                                break;

                            case ActionType.GetPrevious:
                                //GetPreviousNumber(stats, currenttx,keynumber);
                                break;

                            case ActionType.Insert:
                                InsertNumber(currenttx, action.Key, action.Entry);
                                break;

                            case ActionType.Update:
                                UpdateNumber(currenttx, action.Key, action.Entry);
                                break;

                            case ActionType.Upsert:
                                UpsertNumber(currenttx, action.Key, action.Entry);
                                break;

                            case ActionType.IterateForward:
                                Iterate(currenttx, action.Key, action.Entry, true);
                                break;

                            case ActionType.IterateBackward:
                                Iterate(currenttx, action.Key, action.Entry, false);
                                break;

                            case ActionType.BeginChildTx:
                                currenttx = BeginChildTx(currenttx);
                                break;

                            case ActionType.CommitChildTx:
                                currenttx = CommitChildTx(currenttx);
                                break;

                            case ActionType.RollbackChildTx:
                                currenttx = RollbackChildTx(currenttx);
                                break;

                            case ActionType.CreateCursor:
                                CreateCursor(currenttx, action.Key);
                                break;

                            case ActionType.DeleteCursor:
                                VerifyCursorDelete(currenttx, action.Key);
                                break;

                            default:
                                throw new NotSupportedException("Unknown ActionType!");
                        }

                        if (currenttx != null)
                        {
                            if (DoVerifyCursors)
                            {
                                VerifyCursors(currenttx);
                            }
                        }

                        OnAfterAction(action, currenttx);
                    }
                }
                catch (Exception ex)
                {
                    ActionLog?.LogError(ex);

                    Tools.WriteError(ex, ex.Message);
                    Stats.Errors++;
                    haserrors = true;
                }
            }

            if (Stats.Errors > 0)
            {
                if (!(provider is LogActionProvider))
                {
                    SaveFiles();
                }
            }
            else
            {
                if (DoDeleteOnSuccess)
                {
                    DeleteFiles();
                }
            }

            return !haserrors;
        }


        private void DumpPages(Transaction currenttx)
        {
            // Cache
            var pp = currenttx.Database.Pager as ExclusivePageProvider;
            if (pp != null)
            {
                void Iterate(ulong pageno, ref PageRef item)
                {
                    var msg = string.Format("{0}: {1} = {2}", pageno, item.PageNumber, Tools.GetHexString(item.Page?.Bytes.Span.ToArray()));
                    ActionLog?.Log("Cache", msg);
                }

                pp.Cache._pages.ForEach(Iterate);
            }

            var tx = currenttx;
            while (tx != null)
            {
                var msg = string.Format("Tid={0} Oid={1} ParentOid={2}", tx.Tid, tx.Oid, tx.Parent?.Oid);
                ActionLog?.Log("Transaction", msg);

                void Iterate(ulong pageno, ref PageAndState item)
                {
                    if (item.Page == null)
                    {
                        msg = string.Format("{0}: {1}", pageno, item.State);
                    }
                    else
                    {
                        msg = string.Format("{0}: {1} = {2}", pageno, item.State, Tools.GetHexString(item.Page.Bytes.Span.ToArray()));
                    }

                    ActionLog?.Log("PageAndState", msg);
                }

                tx = tx.Parent;
            }
        }

        /// <summary>
        /// makes a backup of the database
        /// </summary>
        /// <param name="tid">transaction id</param>
        /// <param name="mod">mod if tid % mod == 0 the database will be backed up</param>
        /// <param name="keep">number pf copies to keep</param>
        /// <exception cref="NotImplementedException"></exception>
        private void BackupDatabase(Database db, ulong tid, int mod, int keep)
        {
            var tidl = (long)tid;

            if (tidl % mod != 0)
            {
                return;
            }

            var backupname = GetBackupName(tid);
            db.CopyTo(backupname);

            if (tidl > keep * mod)
            {
                var start = tidl - keep * mod;

                while (true)
                {
                    var fname = GetBackupName((ulong)start);
                    if (File.Exists(fname))
                    {
                        File.Delete(fname);
                    }
                    else
                    {
                        break;
                    }

                    start -= mod;
                }
            }
        }


        private string GetBackupName(ulong tid)
        {
            var fullname = Description.DbFilename;
            var path = Path.GetDirectoryName(fullname);
            var fname = Path.GetFileNameWithoutExtension(fullname);
            var fext = Path.GetExtension(fullname);

            var targetname = Path.Combine(path, string.Format("{0}.{1:0000000}{2}", fname, tid, fext));

            return targetname;
        }

        private void OnBeforeAction(ActionEntry action, Transaction currenttx)
        {
            //if (currenttx != null && currenttx.Tid == 919)
            //{
            //    var path = new PathToKey();
            //    path.Path.Add(12);
            //    path.Path.Add(13);

            //    GetNumber(currenttx, path);
            //}
        }

        private void OnAfterAction(ActionEntry action, Transaction currenttx)
        {
        }

        #endregion

        private static object _lock = new object();

        private void ShowStats(Database db, string title)
        {
            lock (_lock)
            {
                Tools.WriteColor(ConsoleColor.Gray, "------- {0} -------------------------------------------", title);
                Tools.WriteColor(ConsoleColor.Gray, "Number: {0}", Description.Token);
                Tools.WriteColor(ConsoleColor.Gray, "Tx Count: {0}", Stats.TxCount);
                Tools.WriteColor(ConsoleColor.Gray, "Deleted: {0}", Stats.Deleted);
                Tools.WriteColor(ConsoleColor.Gray, "Existing: {0}", Stats.Existing);
                Tools.WriteColor(ConsoleColor.Gray, "Got: {0}", Stats.Got);
                Tools.WriteColor(ConsoleColor.Gray, "Inserted: {0}", Stats.Inserted);
                Tools.WriteColor(ConsoleColor.Gray, "Updated: {0}", Stats.Updated);
                Tools.WriteColor(ConsoleColor.Gray, "Upserted: {0}", Stats.Upserted);
                Tools.WriteColor(ConsoleColor.Gray, "ShadowSize: {0}", Shadow.Current.TotalCount);

                if (DoShowAdditionalStats)
                {
                    Tools.WriteColor(ConsoleColor.Gray, db.Allocator.GetStats().ToString());

                    var ep = db.Pager as ExclusivePageProvider;
                    if (ep != null)
                    {
                        Tools.WriteColor(ConsoleColor.Gray, ep.Cache.GetStats().ToString());
                    }

                    Tools.WriteColor(ConsoleColor.Gray, db.Tracker.GetStats().ToString());
                }

                Tools.WriteColor(Stats.Errors == 0 ? ConsoleColor.Green : ConsoleColor.Red, "Errors: {0}", Stats.Errors);
                Tools.WriteColor(ConsoleColor.Gray, "--------------------------------------------------");
            }
        }

        #region FileHandling

        private void DeleteFiles()
        {
            var files = new List<string>() { Description.DbFilename, Description.DbFilename + ".log", ActionLog?.FullPath };
            files.AddRange(GetBackups());

            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error deleting {0}: ", file, ex.Message);
                    }
                }
            }
        }

        private List<string> GetBackups()
        {
            var path = Path.GetDirectoryName(Description.DbFilename);
            var fname = Path.GetFileNameWithoutExtension(Description.DbFilename);
            var ext = Path.GetExtension(Description.DbFilename);

            var backups = Directory.GetFiles(path, fname + ".*" + ext);

            return backups.ToList();
        }

        private void SaveFiles()
        {
            var files = new List<string>() { Description.DbFilename, Description.DbFilename + ".log", ActionLog?.FullPath };
            files.AddRange(GetBackups());

            var targetpath = TestDescription.ErrorPath;
            Directory.CreateDirectory(targetpath);

            foreach (var file in files)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                    {
                        var newfile = Path.Combine(targetpath, Path.GetFileName(file));
                        File.Move(file, newfile);
                    }
                }
                catch (Exception ex)
                {
                    Tools.WriteError(ex, "Error copying file '{0}'", file);
                }
            }

            try
            {
                // save testdescription
                Description.Save(TestDescription.ErrorPath);
            }
            catch (Exception ex)
            {
                Tools.WriteError(ex, "Error saving TestDescription.");
            }
        }

        #endregion

        private void DoTimed(string name, Action action)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Restart();
                action.Invoke();
            }
            finally
            {
                sw.Stop();
                Tools.WriteColor(ConsoleColor.Cyan, string.Format("{0} - {1}: {2}ms", Description.Token, name, sw.ElapsedMilliseconds), null);
            }
        }

        #region Validations

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyPageStates(Transaction tx)
        {
            tx.Pages.Validate();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyDatabase(Database db, Transaction tx)
        {
            var all = DbInspector.GetPageRange(db, tx.Meta.DataRootPage);
            var fp = DbInspector.GetPageRange(db, tx.Meta.FsRootPage);

            all.AddRanges(fp);

            // add range of fileheader and metapages
            all.AddRange(0, Limits.MetaPages);

            var pages = string.Join(", ", all.ToList());

            if (all.RangeCount > 1)
            {
                throw new ArgumentException(string.Format("Memory leak in Database! (Gaps) - {0}: {1}", tx.Tid, pages));
            }

            var range = all.ToList().First();
            if (range.Last != tx.Meta.LastPage)
            {
                throw new ArgumentException(string.Format("Memory leak in Database! (Gap at end) - {0}: {1}", tx.Tid, pages));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyCursors(Transaction tx)
        {
            foreach (var item in Shadow.Cursors.Values)
            {
                var key = item.Key;
                var cursor = item.Cursor;

                if (cursor.CurrentPath.IsValid)
                {
                    var entry = Shadow.Current.GetEntry(key);

                    // cursor.ValidateCursor(entry.Key);
                    VerifyCursor(tx, cursor, key, entry.Key);
                }
                else
                {
                    throw new ArgumentException("Cursor is invalid!");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyCursor(Transaction tx, Cursor cursor, PathToKey keypath, byte[] key)
        {
            using (var keyref = GetKeyRef(tx, keypath, false))
            using (var correct = tx.GetCursor(keyref, InternalTrackingScope.None))
            {
                var keyspan = new ReadOnlySpan<byte>(key);
                correct.SetPositionEx(CursorPositions.Key, ref keyspan);

                var (equal, text) = KvDebug.CompareCursors(correct, cursor);

                if (!equal)
                {
                    var msg = string.Format("Cursor mismatch Tag: {0} Keynumber: {1}\nCorrect     Actual\n{2}", cursor.Tag, keypath, text);
                    throw new ArgumentException(msg);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyCursorDelete(Transaction tx, PathToKey key)
        {
            if (Shadow.Cursors.ContainsKey(key.ToString()))
            {
                ActionLog?.LogAction(ActionType.DeleteCursor, tx.Tid, key, null);

                var cursorentry = Shadow.Cursors[key.ToString()];
                if (cursorentry.Cursor.CurrentPath.IsValid)
                {
                    // cursor should be invalid so raise error
                    throw new ArgumentException("Cursor should be invalid!");
                }

                Shadow.RemoveCursor(new KeyValuePair<string, CursorEntry>(key.ToString(), cursorentry));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyShadow(Transaction tx, TreeNode node)
        {
            if (node.Children.Count > 0)
            {
                using (var keyref = GetKeyRef(tx, node.Children.First().Path, false))
                {
                    foreach (var child in node.Children)
                    {
                        if (!tx.Exists(keyref, child.Entry.Key))
                        {
                            throw new ArgumentException(string.Format("VERIFY: Key not found! Keynumber={0} Key={1}", child.Path, Tools.GetHexString(child.Entry.Key)));
                        }

                        var val = tx.Get(keyref, child.Entry.Key);
                        var bytes = val.ValueSpan;

                        if (!MemoryExtensions.SequenceEqual<byte>(child.Entry.Value, bytes))
                        {
                            throw new Exception(string.Format("VERIFY: Value not equal! Keynumber={0} Key={1}\nValue={2}\nExpected={3}\nValueRef=({4})",
                                child.Path, Tools.GetHexString(child.Entry.Key),
                                Tools.GetHexString(bytes.ToArray()),
                                Tools.GetHexString(child.Entry.Value),
                                val.GetDebugInfo()));
                        }
                    }
                }

                foreach (var child in node.Children)
                {
                    VerifyShadow(tx, child);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyCacheSinglePage(Database db, Transaction roottx)
        {
            var pp = roottx.Database.Pager as ExclusivePageProvider;
            if (pp != null)
            {
                var pagesize = (int)roottx.Database.Options.PageSize;
                var pagecount = roottx.Meta.LastPage + 1;

                //if (pagecount != (long)roottx.Meta.MaxLastPage + 1)
                //{
                //    throw new Exception(string.Format("VERIFY: FileSize mismatch! MaxLastPage={0} ActualLastPage={1}"
                //}

                var buffer = new byte[pagesize];

                for (ulong pageno = 0; pageno < pagecount; pageno++)
                {
                    roottx.Database.DbFile.Seek((long)pageno * pagesize, SeekOrigin.Begin);
                    var bytesread = roottx.Database.DbFile.Read(buffer);
                    if (bytesread != pagesize)
                    {
                        throw new Exception("Not enough bytes read!");
                    }

                    ref var pageref = ref pp.Cache.GetPage((ulong)pageno, out var _);
                    if (pageref.Page != null)
                    {
                        if (!MemoryExtensions.SequenceEqual<byte>(buffer, pageref.Page.Bytes.Span))
                        {
                            throw new Exception(string.Format("VERIFY: Cache does not match file! Pagenumber={0}/{1}\nFileValue={2}\nCachedValue={3}\nFileLength={4}",
                                pageno, pagecount, Tools.GetHexString(buffer),
                                Tools.GetHexString(pageref.Page.Bytes.Span.ToArray()),
                                roottx.Database.DbFile.Length));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void VerifyCacheAllPages(Database db, Transaction roottx)
        {
            var pp = roottx.Database.Pager as ExclusivePageProvider;
            if (pp != null)
            {
                var pagesize = (int)roottx.Database.Options.PageSize;
                var pagecount = roottx.Meta.LastPage + 1;

                //if (pagecount != (long)roottx.Meta.MaxLastPage + 1)
                //{
                //    throw new Exception(string.Format("VERIFY: FileSize mismatch! MaxLastPage={0} ActualLastPage={1}"
                //}

                var size = pagesize * (int)pagecount;
                var buffer = new byte[size];
                roottx.Database.DbFile.Seek(0, SeekOrigin.Begin);
                var bytesread = roottx.Database.DbFile.Read(buffer);
                if (bytesread != size)
                {
                    throw new Exception("Not enough bytes read!");
                }

                for (ulong pageno = 0; pageno < pagecount; pageno++)
                {
                    ref var pageref = ref pp.Cache.GetPage((ulong)pageno, out var _);
                    if (pageref.Page != null)
                    {
                        var span = buffer.AsSpan().Slice(pagesize * (int)pageno, pagesize);
                        
                        // Decrypt page
                        db.Encryptor.Decrypt(span, pageno);

                        if (!MemoryExtensions.SequenceEqual<byte>(span, pageref.Page.Bytes.Span))
                        {
                            throw new Exception(string.Format("VERIFY: Cache does not match file! Pagenumber={0}/{1}\nFileValue={2}\nCachedValue={3}\nFileLength={4}",
                                pageno, pagecount, Tools.GetHexString(buffer),
                                Tools.GetHexString(pageref.Page.Bytes.Span.ToArray()),
                                roottx.Database.DbFile.Length));
                        }
                    }
                }
            }
        }

        private void VerifyRefCount(Database db, Transaction roottx)
        {
            var allocatorstats = db.Allocator.GetStats();

            // there should only pages with refcount 1 (the cached pages)
            if (allocatorstats.HasRefCountsGT(1))
            {
                var msg = string.Format("RefCounts greater 1 found!\n{0}", allocatorstats.ToString());
                throw new Exception(msg);
            }

            // the number of cached pages should equal the number of pages with refcount 1
            var ep = db.Pager as ExclusivePageProvider;
            if (ep != null)
            {
                var cachestats = ep.Cache.GetStats();
                if (cachestats.PageCount != allocatorstats.ItemsWithRefCount(1))
                {
                    var items = allocatorstats.RefCounts.First().Select(x => x.PageNumber).ToList();
                    foreach (var range in cachestats.Ranges.ToList())
                    {
                        for (ulong pageno = range.First; pageno <= range.Last; pageno++)
                        {
                            items.Remove(pageno);
                        }
                    }

                    var result = string.Join(",", items);

                    var msg = string.Format("The number of cached pages ({0}) does not equal the number of pages with refcount 1 ({1})!\n{2}\nSuperflous pages: {3}",
                                            cachestats.PageCount,
                                            allocatorstats.ItemsWithRefCount(1),
                                            allocatorstats.ToString(),
                                            result);
                    throw new Exception(msg);
                }
            }
        }

        #endregion

        #region Actions

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void CreateCursor(Transaction tx, PathToKey key)
        {
            var entry = Shadow.Current.GetEntry(key);

            // TODO support multiple cursors per key
            if (entry != null && !Shadow.Cursors.ContainsKey(key.ToString()))
            {
                ActionLog?.LogAction(ActionType.CreateCursor, tx.Tid, key, null);

                var c = tx.GetCursorEx(GetKeyRef(tx, key, false), InternalTrackingScope.TransactionChain);

                var keyspan = new ReadOnlySpan<byte>(entry.Key);
                c.SetPositionEx(CursorPositions.Key, ref keyspan);
                c.Tag = key.ToString();

                Shadow.Cursors.Add(key.ToString(), new CursorEntry() { Cursor = c, Key = key });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Transaction RollbackChildTx(Transaction tx)
        {
            ActionLog?.LogAction(ActionType.RollbackChildTx, tx.Tid, null, null);

            Shadow.Rollback();
            var parent = tx.Parent;
            tx.Rollback();
            tx.Dispose();

            // remove cursors invalidated by rollback
            var deletees = Shadow.Cursors.Where(x => x.Value.Cursor.CurrentPath == null).ToList();
            deletees.ForEach(x => Shadow.RemoveCursor(x));

            return parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Transaction CommitChildTx(Transaction tx)
        {
            ActionLog?.LogAction(ActionType.CommitChildTx, tx.Tid, null, null);

            Shadow.Commit();
            var parent = tx.Parent;
            tx.Commit();
            tx.Dispose();

            return parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Transaction BeginChildTx(Transaction tx)
        {
            ActionLog?.LogAction(ActionType.BeginChildTx, tx.Tid, null, null);

            Shadow.BeginTransaction();
            var child = tx.BeginChildTransaction();

            return child;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void UpsertNumber(Transaction tx, PathToKey key, KVEntry newentry = null)
        {
            var dump = false;

            //if (key.ToString() == "1494")
            //{
            //    dump = true;
            //}

            if (!Shadow.Current.ExistsParent(key))
            {
                UpsertNumber(tx, key.Parent, null);
            }

            var entry = Shadow.Current.GetEntry(key);

            if (dump)
            {
                KvDebug.DumpTree(tx, Program.DUMPFILE, false);
            }

            if (entry != null)
            {
                if (newentry != null)
                {
                    entry = newentry;
                }
                else
                {
                    SetRandomValue(entry);
                }

                ActionLog?.LogAction(ActionType.Upsert, tx.Tid, key, entry);

                Shadow.Current.UpdateEntry(key, entry);

                using (var keyref = GetKeyRef(tx, key, false))
                {
                    VerifyCursors(tx);

                    tx.Upsert(keyref, entry.Key, entry.Value);
                }
            }
            else
            {
                if (newentry != null)
                {
                    entry = newentry;
                }
                else
                {
                    entry = GenerateKeyValue(key);
                }

                ActionLog?.LogAction(ActionType.Upsert, tx.Tid, key, entry);

                Shadow.Current.InsertEntry(key, entry);

                using (var keyref = GetKeyRef(tx, key, true))
                {
                    VerifyCursors(tx);

                    tx.Upsert(keyref, entry.Key, entry.Value);
                }
            }

            if (dump)
            {
                KvDebug.DumpTree(tx, Program.DUMPFILE, false);
            }

            Stats.Upserted++;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void UpdateNumber(Transaction tx, PathToKey key, KVEntry newentry = null)
        {
            var entry = Shadow.Current.GetEntry(key);

            if (entry != null)
            {
                if (newentry != null)
                {
                    entry = newentry;
                }
                else
                {
                    SetRandomValue(entry);
                }

                ActionLog?.LogAction(ActionType.Update, tx.Tid, key, entry);

                Shadow.Current.UpdateEntry(key, entry);

                using (var keyref = GetKeyRef(tx, key, false))
                {
                    tx.Update(keyref, entry.Key, entry.Value);
                }

                Stats.Updated++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void InsertNumber(Transaction tx, PathToKey key, KVEntry newentry = null)
        {
            if (!Shadow.Current.ExistsParent(key))
            {
                InsertNumber(tx, key.Parent, null);
            }

            var entry = Shadow.Current.GetEntry(key);

            if (entry == null)
            {
                if (newentry != null)
                {
                    entry = newentry;
                }
                else
                {
                    entry = GenerateKeyValue(key);
                }

                ActionLog?.LogAction(ActionType.Insert, tx.Tid, key, entry);

                Shadow.Current.InsertEntry(key, entry);

                using (var keyref = GetKeyRef(tx, key, true))
                {
                    tx.Insert(keyref, entry.Key, entry.Value);
                }

                Stats.Inserted++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private KVEntry GenerateKeyValue(PathToKey key)
        {
            var ret = new KVEntry();
            ret.KeyLength = KeyValueGenerator.GetRandomLength(Description.MinKeySize, Description.MaxKeySize);
            ret.Key = KeyValueGenerator.GetBytes(KeyGenStrategy.Sequential, key.Path.Last(), ret.KeyLength, ret.KeyLength);

            SetRandomValue(ret);

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public KVEntry SetRandomValue(KVEntry entry)
        {
            entry.ValueLength = KeyValueGenerator.GetRandomLength(Description.MinValueSize, Description.MaxValueSize);
            entry.ValueSeed = KeyValueGenerator.GetRandomSeed();
            entry.Value = KeyValueGenerator.GetSeededBytes(entry.ValueSeed, entry.ValueLength);

            return entry;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GetNumber(Transaction tx, PathToKey key)
        {
            var entry = Shadow.Current.GetEntry(key);

            if (entry != null)
            {
                ActionLog?.LogAction(ActionType.Get, tx.Tid, key, entry);

                using (var keyref = GetKeyRef(tx, key, false))
                {
                    var val = tx.Get(keyref, entry.Key);

                    if (!MemoryExtensions.SequenceEqual<byte>(entry.Value, val.ValueSpan))
                    {
                        throw new Exception(string.Format("GET: Value not equal! Keynumber={0} Key={1} Value={2} Expected={3}",
                            key,
                            Tools.GetHexString(entry.Key),
                            Tools.GetHexString(val.ValueSpan),
                            Tools.GetHexString(entry.Value)));
                    }
                    else
                    {
                        Stats.Got++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void ExistsNumber(Transaction tx, PathToKey key)
        {
            var entry = Shadow.Current.GetEntry(key);

            var ex1 = entry != null;
            if (ex1)
            {
                ActionLog?.LogAction(ActionType.Exists, tx.Tid, key, entry);

                using (var keyref = GetKeyRef(tx, key, false))
                {
                    var ex2 = tx.Exists(keyref, entry.Key);

                    if (ex1 != ex2)
                    {
                        throw new Exception(string.Format("EXISTS: Key not found! Keynumber={0} Key={1}", key, Tools.GetHexString(entry.Key)));
                    }
                    else
                    {
                        Stats.Existing++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DeleteNumber(Transaction tx, PathToKey key)
        {
            var entry = Shadow.Current.GetEntry(key);

            if (entry != null)
            {
                ActionLog?.LogAction(ActionType.Delete, tx.Tid, key, entry);

                Shadow.Current.Remove(key);

                using (var keyref = GetKeyRef(tx, key, false))
                {
                    if (tx.Delete(keyref, entry.Key))
                    {
                        Stats.Deleted++;

                        VerifyCursorDelete(tx, key);
                    }
                    else
                    {
                        var node = Shadow.Current.GetNode(key);
                        if (node == null)
                        {
                            throw new Exception(string.Format("DELETE: Key not found! Keynumber={0} Key={1}", key, Tools.GetHexString(entry.Key)));
                        }

                        if (node.Children.Count == 0)
                        {
                            throw new Exception(string.Format("DELETE: Node without children not deleted! Keynumber={0} Key={1}", key, Tools.GetHexString(entry.Key)));
                        }
                    }
                }
            }
        }

        private void Iterate(Transaction tx, PathToKey path, KVEntry e, bool forward)
        {
            var root = Shadow.Current.GetParentNode(path);
            if (root != null)
            {
                // get all entries
                var items = root.Children.Select(x => x.Entry).ToList();
                if (items.Count > 0)
                {
                    ActionLog?.LogAction(forward ? ActionType.IterateForward : ActionType.IterateBackward, tx.Tid, path, e);

                    //DumpEntries(items);

                    using (var keyref = GetKeyRef(tx, path, false))
                    {
                        using (var iter = tx.GetIterator(keyref, forward))
                        {
                            //DumpIterator(iter);
                        }

                        using (var iter = tx.GetIterator(keyref, forward))
                        {
                            foreach (var item in iter)
                            {
                                var val = item.Value;
                                val.Validate();

                                var entry = RemoveEntry(items, val.Key);
                                var bytes = val.ValueSpan;

                                if (!MemoryExtensions.SequenceEqual<byte>(entry.Value, bytes))
                                {
                                    throw new Exception(string.Format("ITERATE: Value not equal! Keynumber={0} Key={1}\nValue={2}\nExpected={3}\nValueRef=({4})",
                                        path, Tools.GetHexString(entry.Key),
                                        Tools.GetHexString(bytes.ToArray()),
                                        Tools.GetHexString(entry.Value),
                                        val.GetDebugInfo()));
                                }
                            }
                        }
                    }

                    if (items.Count > 0)
                    {
                        throw new Exception(string.Format("ITERATE: Not all keys have been found by the iterator! ({0} keys missing.)", items.Count));
                    }
                }
            }
        }

        private void DumpEntries(List<KVEntry> items)
        {
            Console.WriteLine("-----------------------");
            foreach (var item in items)
            {
                Console.WriteLine("Entries: {0} = {1}", Tools.GetHexString(item.Key), item.ValueLength);
            }
        }

        private void DumpIterator(KeyIterator iter)
        {
            foreach (var item in iter)
            {
                Console.WriteLine("Iter: {0} = {1}", Tools.GetHexString(item.Value.Key), item.Value.Length);
            }
        }

        /// <summary>
        /// removes the item with the given key from the list and returns it
        /// </summary>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private KVEntry RemoveEntry(List<KVEntry> items, ReadOnlySpan<byte> key)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (MemoryExtensions.SequenceEqual<byte>(item.Key, key))
                {
                    items.RemoveAt(i);
                    return item;
                }
            }

            throw new Exception(string.Format("ITERATE: Key not found! Key={0}", Tools.GetHexString(key)));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private TreeRef GetKeyRef(Transaction tx, PathToKey key, bool ensure)
        {
            if (key == null || key.Path == null || key.Path.Count <= 1)
            {
                return null;
            }

            var list = Shadow.Current.GetParentKeys(key);

            if (list != null)
            {
                var keys = list.Select(x => new ReadOnlyMemory<byte>(x)).ToArray();
                var keyref = ensure ? tx.EnsureTreeRef(TrackingScope.TransactionChain, keys) : tx.GetTreeRef(TrackingScope.TransactionChain, keys);

                if (keyref != null)
                {
                    return keyref;
                }
            }

            throw new Exception(string.Format("KeyRef not found! Keynumber = {0}", key));
        }

        #endregion
    }
}


