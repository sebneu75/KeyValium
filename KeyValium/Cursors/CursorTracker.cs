

using KeyValium.Collections;
using System;
using System.Security.Cryptography;

namespace KeyValium.Cursors
{
    internal class CursorTracker : IDisposable
    {
        #region Constructor

        public CursorTracker(Database db)
        {
            Perf.CallCount();

            Database = db;
        }

        #endregion

        #region Variables

        private readonly Database Database;

        /// <summary>
        /// Tracked TreeRefs
        /// </summary>
        private readonly Dictionary<ulong, TrackedCursor> _treerefs = new();

        /// <summary>
        /// Tracked Cursors
        /// </summary>
        private readonly Dictionary<ulong, TrackedCursor> _cursors = new();

        /// <summary>
        /// Suspended Cursors
        /// </summary>
        private readonly Dictionary<ulong, SuspendedTreeRef> _suspended = new();

        private object _trackerlock = new object();

        #endregion

        public CursorTrackerStats GetStats()
        {
            Perf.CallCount();

            return new CursorTrackerStats(_treerefs.Count, _cursors.Count, _suspended.Count);
        }

        #region handling of tracked objects

        internal void Add(TreeRef treeref)
        {
            Perf.CallCount();

            // TODO check if necessary
            KvDebug.Assert(Monitor.IsEntered(treeref.Cursor.CurrentTransaction.TxLock), "Read lock not held!");

            Add(new TrackedCursor(treeref, treeref.Cursor));
        }

        internal void Add(Cursor cursor)
        {
            Perf.CallCount();

            // TODO check if necessary
            KvDebug.Assert(Monitor.IsEntered(cursor.CurrentTransaction.TxLock), "Read lock not held!");

            Add(new TrackedCursor(null, cursor));
        }

        internal void Add(TrackedCursor tc)
        {
            Perf.CallCount();

            lock (_trackerlock)
            {
                _cursors.Add(tc.Cursor.Oid, tc);

                if (tc.TreeRef != null)
                {
                    _treerefs.Add(tc.TreeRef.Oid, tc);
                }
            }
        }

        internal void Remove(TreeRef treeref)
        {
            Perf.CallCount();

            RemoveByTreeRefOid(treeref.Oid);
        }

        internal void Remove(Cursor cursor)
        {
            Perf.CallCount();

            RemoveByCursorOid(cursor.Oid);
        }

        private void RemoveByCursorOid(ulong oid)
        {
            Perf.CallCount();

            lock (_trackerlock)
            {
                if (_cursors.ContainsKey(oid))
                {
                    var tc = _cursors[oid];
                    tc.Cursor = null;

                    if (tc.TreeRef != null && !_suspended.ContainsKey(tc.TreeRef.Oid))
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, "TreeRef is not null and not suspended on cursor remove!");
                    }

                    _cursors.Remove(oid);
                }
            }
        }

        private void RemoveByTreeRefOid(ulong oid)
        {
            Perf.CallCount();

            lock (_trackerlock)
            {
                var tc = _treerefs[oid];

                if (tc.Cursor != null)
                {
                    _cursors.Remove(tc.Cursor.Oid);
                }

                _treerefs.Remove(oid);
                _suspended.Remove(oid);
            }
        }

        #endregion

        #region Queries

        /// <summary>
        /// returns all Cursors with the current transaction tx
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        private List<TrackedCursor> GetTrackedCursors(Transaction tx, Cursor excluded, bool validonly = true)
        {
            Perf.CallCount();

            lock (_trackerlock)
            {
                if (_cursors.Count > 0)
                {
                    var ret = new List<TrackedCursor>(_cursors.Count);

                    foreach (var item in _cursors.Values)
                    {
                        // only adjust cursors for this transaction
                        if (item.Cursor != excluded && item.Cursor.CurrentTransaction.Oid == tx.Oid)
                        {
                            if (validonly && !item.Cursor.CurrentPath.IsValid)
                            {
                                // do not include invalid cursors if only valid cursors where requested
                                continue;
                            }

                            ret.Add(item);
                        }
                    }

                    return ret;
                }
            }

            return null;
        }

        /// <summary>
        /// returns a list of all pagenumbers that are currently used by tracked cursors regardless of transaction level
        /// </summary>
        /// <returns></returns>
        internal KvHashSet GetPageNumbers()
        {
            Perf.CallCount();

            var ret = new KvHashSet();

            lock (_trackerlock)
            {
                foreach (var tc in _cursors.Values)
                {
                    for (int p = 0; p <= tc.Cursor.PathChain.Items._allocator.Last; p++)
                    {
                        ref var item = ref tc.Cursor.PathChain.Items._allocator.GetRef(p);
                        for (int i = item.Path.First; i <= item.Path.Last; i++)
                        {
                            ref var node = ref item.Path.GetNode(i);
                            ret.Add(node.Page.PageNumber);
                        }
                    }
                }
            }

            return ret;
        }

        #endregion

        #region Suspension and Resurrection

        private void SuspendTreeRef(Transaction tx, Cursor cursor, KvTid etid)
        {
            Perf.CallCount();

            // TODO check locks
            lock (_trackerlock)
            {
                lock (tx.TxLock)
                {
                    // caller is responsible for Validation of Cursor

                    var tc = _cursors[cursor.Oid];
                    var treerefoid = tc.TreeRef.Oid;

                    var susp = new SuspendedTreeRef(etid, treerefoid);

                    for (int i = cursor.CurrentPath.First; i <= cursor.CurrentPath.Last; i++)
                    {
                        ref var node = ref cursor.CurrentPath.GetNode(i);

                        susp.KeyPointers.Add(new SuspendedKeyPointer(node.Page.PageNumber, node.KeyIndex));
                    }

                    _suspended[treerefoid] = susp;

                    // dispose the cursor (removes it from the tracker)
                    tc.TreeRef.Cursor.Dispose();
                    tc.TreeRef.Cursor = null;
                }
            }
        }

        private void ResurrectTreeRef(Transaction tx, SuspendedTreeRef susp)
        {
            Perf.CallCount();

            // TODO check locks
            lock (_trackerlock)
            {
                lock (tx.TxLock)
                {
                    var tc = _treerefs[susp.TreeRefOid];

                    if (tc.Cursor != null)
                    {
                        throw new Exception("Cursor is not null on Resurrection!");
                    }

                    if (susp.Tid == tx.Meta.SourceTid)
                    {
                        // rebuild cursor
                        var cursor = tx.GetCursor(null, (InternalTrackingScope)tc.TreeRef.Scope, false);
                        foreach (var skp in susp.KeyPointers)
                        {
                            using (var page = tx.GetPage(skp.PageNumber, true, out _))
                            {
                                cursor.CurrentPath.Append(page, skp.KeyIndex);
                            }
                        }

                        tc.Cursor = cursor;
                        tc.TreeRef.Cursor = cursor;

                        // tc.TreeRef._istouched = false;

                        _cursors.Add(cursor.Oid, tc);
                    }
                    else
                    {
                        // recreate from stored keys

                        tc.TreeRef.RestoreCursor(tx);

                        if (tc.TreeRef.Cursor != null)
                        {
                            tc.Cursor = tc.TreeRef.Cursor;
                            _cursors.Add(tc.Cursor.Oid, tc);
                        }
                        else
                        {
                            // TreeRef cannot be resurrected
                            // TODO ???
                        }
                    }
                }
            }
        }

        #endregion

        #region Transaction handling

        internal void OnBeginRootTransaction(Transaction tx)
        {
            Perf.CallCount();

            foreach (var susp in _suspended)
            {
                ResurrectTreeRef(tx, susp.Value);
            }
        }

        internal void OnCommitRootTransaction(Transaction tx, KvTid etid)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, null, false);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    if (tc.Cursor.CurrentPath == null || !tc.Cursor.CurrentPath.IsValid)
                    {
                        if (tc.TreeRef != null)
                        {
                            _suspended.Remove(tc.TreeRef.Oid);
                        }

                        tc.Dispose();
                    }
                    else if (tc.Cursor.Scope == InternalTrackingScope.Database)
                    {
                        SuspendTreeRef(tx, tc.Cursor, etid);
                        // cursor must have KeyRef for suspension
                        // TODO suspend valid KeyRefs for later resurrection
                        // TODO do not dispose KeyRefs
                        // dispose cursors without keyref

                        // done by SuspendCursor()
                        //_cursors.Remove(cursor.Oid);
                        //CursorToKeyRef.Remove(cursor.Oid);
                        //// dispose Database-scoped cursors
                        //cursor.Dispose();
                    }
                    else if (tc.Cursor.Scope == InternalTrackingScope.TransactionChain)
                    {
                        // declare cursors invalid
                        tc.Dispose();
                        //Remove(cursor);
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled CursorScope!");
                    }
                }
            }
        }

        internal void OnRollbackRootTransaction(Transaction tx)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, null, false);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    if (tc.Cursor.Scope == InternalTrackingScope.Database)
                    {
                        // TODO dispose keyrefs that are not in the resurrection list
                        // dispose cursors

                        // dispose Database-scoped cursors
                        //DisposeCursors(CursorScope.Database);

                        if (_suspended.ContainsKey(tc.TreeRef.Oid))
                        {
                            // if TreeRef exists in Suspended state it may become valid again
                            // so only dispose the cursor
                            
                            tc.TreeRef.Cursor.Dispose();
                            tc.TreeRef.Cursor = null;                            
                            tc.Cursor = null;

                            //var temp = tc.TreeRef;
                            //tc.TreeRef = null;
                            //tc.Dispose();
                            //tc.TreeRef = temp;
                        }
                        else
                        {
                            tc.Dispose();
                        }

                        //Remove(cursor);
                    }
                    else if (tc.Cursor.Scope == InternalTrackingScope.TransactionChain)
                    {
                        // declare cursors invalid
                        tc.Dispose();
                        //Remove(cursor);
                    }
                    else
                    {
                        throw new NotSupportedException("Unhandled CursorScope!");
                    }
                }
            }
        }

        internal void OnBeginChildTransaction(Transaction parent, Transaction child)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(parent.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(parent, null, false);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    // TODO untouch keyref?
                    tc.Cursor.PathChain.AppendCopy(child);
                }
            }
        }

        internal void OnCommitChildTransaction(Transaction tx)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, null, false);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    tc.Cursor.PathChain.CommitToParent();
                }
            }
        }

        internal void OnRollbackChildTransaction(Transaction tx)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, null, false);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    tc.Cursor.PathChain.RollbackToParent();

                    if (tc.Cursor.CurrentPath == null)
                    {
                        // remove invalid cursors

                        tc.Dispose();

                        // TODO Test should be removed by Dispose()
                        //Remove(cursor);
                    }
                }
            }
        }

        #endregion

        #region Cursor Adjustment

        internal void ATOMoveKey(Transaction tx, Cursor excluded, KvPagenumber oldpageno, int keyindex, TreeRef newtreeref, Cursor newcursor)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustMoveKey(oldpageno, keyindex, newtreeref, newcursor);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATODirty(Transaction tx, Cursor excluded, AnyPage oldpage, AnyPage newpage)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustDirty(oldpage, newpage);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATODirtyParentPage(Transaction tx, Cursor excluded, AnyPage newpage)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustDirtyParentPage(newpage);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATOInsertKey(Transaction tx, Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustInsertKey(pageno, keyindex);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATOUpdateKey(Transaction tx, Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustUpdateKey(pageno, keyindex);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATODeleteKey(Transaction tx, Cursor excluded, KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustDeleteKey(pageno, keyindex);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATOSplit(Transaction tx, Cursor excluded, KvPagenumber leftpageno, KvPagenumber rightpageno, ushort splitindex)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustSplit(leftpageno, rightpageno, splitindex);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATOMerge(Transaction tx, Cursor excluded, KvPagenumber targetpageno, KvPagenumber mergeepageno, ushort targetkeycountbefore)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustMerge(targetpageno, mergeepageno, targetkeycountbefore);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATODeletePage(Transaction tx, Cursor excluded, KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustDeletePage(pageno);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATODeleteTree(Transaction tx, Cursor excluded, KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustDeleteTree(pageno);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        internal void ATOInsertPage(Transaction tx, Cursor excluded, KvPagenumber leftpageno, KvPagenumber rightpageno, KvPagenumber newpageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(tx.TxLock), "Write lock not held!");

            var tcs = GetTrackedCursors(tx, excluded);
            if (tcs != null)
            {
                foreach (var tc in tcs)
                {
                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  Before: {0}", KvDebug.GetCursorInfo(tc.Cursor));

                    var ret = tc.Cursor.AdjustInsertPage(leftpageno, rightpageno, newpageno);

                    Logger.LogDebug(LogTopics.Tracking, tx.Tid, "  After{0}: {1}", ret ? "*" : " ", KvDebug.GetCursorInfo(tc.Cursor));
                }
            }
        }

        #endregion

        #region IDisposable

        private bool _isdisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isdisposed)
            {
                if (disposing)
                {
                    lock (_trackerlock)
                    {
                        foreach (var item in _cursors.Values)
                        {
                            item.Dispose();
                        }
                    }
                }

                _isdisposed = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
