using KeyValium.Collections;
using KeyValium.Inspector;
using KeyValium.Memory;
using System.Buffers;
using System.Diagnostics.Tracing;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace KeyValium.Cursors
{
    /// <summary>
    /// represents the path from root to the leafnode
    /// </summary>
    internal sealed class Cursor : IDisposable
    {
        private static ulong OidCounter = 0;

        /// <summary>
        /// called by Transaction.GetCursor()
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="treeref"></param>
        /// <param name="scope"></param>
        /// <param name="freespace"></param>
        /// <param name="isreusable"></param>
        /// <exception cref="NotSupportedException"></exception>
        internal Cursor(Transaction tx, TreeRef treeref, InternalTrackingScope scope, bool freespace, bool isreusable)
        {
            Perf.CallCount();

            Oid = Interlocked.Increment(ref OidCounter);

            if (freespace && treeref != null)
            {
                throw new NotSupportedException("Freespace Cursors do not support TreeRefs.");
            }

            TreeRef = treeref;
            Scope = scope;
            Database = tx.Database;
            PathChain = new NodePathChain(this, tx);

            IsFreeSpaceCursor = freespace;
            IsReusable = isreusable;

            DeleteHandling = DeleteHandling.Invalidate;

            PageTypeIndex = IsFreeSpaceCursor ? PageTypes.FsIndex : PageTypes.DataIndex;
            PageTypeLeaf = IsFreeSpaceCursor ? PageTypes.FsLeaf : PageTypes.DataLeaf;

            Logger.LogInfo(LogTopics.Cursor, "Cursor created {0}.", Oid);

            if (isreusable)
            {
                Pool = Database.Pool;
            }
        }

        #region Variables

        internal readonly ulong Oid;

        internal TreeRef TreeRef;

        internal readonly NodePathChain PathChain;

        internal readonly InternalTrackingScope Scope;

        internal readonly Database Database;

        internal object Tag;

        internal readonly KeyPool Pool;

        // updated by NodePathChain
        internal Transaction CurrentTransaction;

        // updated by NodePathChain
        internal NodePath CurrentPath;

        internal DeleteHandling DeleteHandling;

        internal readonly bool IsFreeSpaceCursor;

        internal readonly bool IsReusable;

        internal readonly ushort PageTypeIndex;

        internal readonly ushort PageTypeLeaf;

        internal KvPagenumber RootPagenumber
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    return CurrentTransaction.Meta.FsRootPage;
                }
                else if (TreeRef == null)
                {
                    return CurrentTransaction.Meta.DataRootPage;
                }
                else
                {
                    return TreeRef.PageNumber;
                }
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating FsRootPage {0} -> {1}", CurrentTransaction.Meta.FsRootPage, value);
                    CurrentTransaction.Meta.FsRootPage = value;
                }
                else if (TreeRef == null)
                {
                    Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating DataRootPage {0} -> {1}", CurrentTransaction.Meta.DataRootPage, value);
                    CurrentTransaction.Meta.DataRootPage = value;
                }
                else
                {
                    Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updated KeyRef-RootPage {0} -> {1}", TreeRef.PageNumber, value);
                    TreeRef.PageNumber = value;
                }
            }
        }

        private ulong TotalCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    return CurrentTransaction.Meta.FsTotalCount;
                }
                else if (TreeRef == null)
                {
                    return CurrentTransaction.Meta.DataTotalCount;
                }
                else
                {
                    return TreeRef.TotalCount;
                }
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating FsRootPage {0} -> {1}", CurrentTransaction.Meta.FsRootPage, value);
                    CurrentTransaction.Meta.FsTotalCount = value;
                }
                else if (TreeRef == null)
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating DataRootPage {0} -> {1}", CurrentTransaction.Meta.DataRootPage, value);
                    CurrentTransaction.Meta.DataTotalCount = value;
                }
                else
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updated KeyRef-RootPage {0} -> {1}", KeyRef.PageNumber, value);
                    TreeRef.TotalCount = value;
                }
            }
        }

        private ulong LocalCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    return CurrentTransaction.Meta.FsLocalCount;
                }
                else if (TreeRef == null)
                {
                    return CurrentTransaction.Meta.DataLocalCount;
                }
                else
                {
                    return TreeRef.LocalCount;
                }
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

                //Validate();

                if (IsFreeSpaceCursor)
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating FsRootPage {0} -> {1}", CurrentTransaction.Meta.FsRootPage, value);
                    CurrentTransaction.Meta.FsLocalCount = value;
                }
                else if (TreeRef == null)
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updating DataRootPage {0} -> {1}", CurrentTransaction.Meta.DataRootPage, value);
                    CurrentTransaction.Meta.DataLocalCount = value;
                }
                else
                {
                    //Logger.LogInfo(LogTopics.Cursor, CurrentTransaction.Tid, "Updated KeyRef-RootPage {0} -> {1}", KeyRef.PageNumber, value);
                    TreeRef.LocalCount = value;
                }
            }
        }

        #endregion

        #region Pagenumber and Position Stacks

        private readonly Stack<KvPagenumber> _pagenumberstack = new();

        /// <summary>
        ///  TODO check if needed
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        internal void SavePagenumber()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            if (!CurrentPath.IsValid || CurrentPath.Current < 0)
            {
                return;
            }

            //Validate();

            _pagenumberstack.Push(CurrentPath.CurrentItem.Page.PageNumber);
        }

        /// <summary>
        ///  TODO check if needed
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        internal void RestorePagenumber()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            if (!CurrentPath.IsValid || CurrentPath.Current < 0)
            {
                return;
            }

            //Validate();

            var pn = _pagenumberstack.Pop();

            for (var i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == pn)
                {
                    CurrentPath.Current = i;
                    return;
                }
            }

            throw new KeyValiumException(ErrorCodes.PageNotFound, "Page not found in cursor stack.");
        }

        //private KeyPointer _savedposition;

        ///// <summary>
        /////  TODO check if needed
        ///// </summary>
        ///// <exception cref="NotSupportedException"></exception>
        //public void SavePosition()
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(CurrentTransaction._lock.IsReadLocked, "Read lock not held!");

        //    //Validate();

        //    if (_savedposition != null)
        //    {
        //        throw new NotSupportedException("Position already saved!");
        //    }

        //    _savedposition = CurrentKeyPath.Current;
        //}

        ///// <summary>
        /////  TODO check if needed
        ///// </summary>
        ///// <exception cref="NotSupportedException"></exception>
        //public void RestorePosition()
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(CurrentTransaction._lock.IsReadLocked, "Read lock not held!");

        //    //Validate();

        //    if (_savedposition == null)
        //    {
        //        throw new NotSupportedException("No saved position!");
        //    }

        //    CurrentKeyPath.Current = _savedposition;
        //    _savedposition = null;
        //}

        #endregion

        #region Cursor Adjustments

        internal bool AdjustDirty(AnyPage oldpage, AnyPage newpage)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == oldpage.PageNumber)
                {
                    node.Page = newpage;
                    changed = true;
                }
            }

            return changed;
        }

        internal bool AdjustDirtyParentPage(AnyPage newpage)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == newpage.PageNumber)
                {
                    node.Page = newpage;
                    changed = true;
                }
            }

            return changed;
        }

        internal bool AdjustDeleteKey(KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == pageno)
                {
                    if (node.Page.PageType == PageTypes.DataIndex || node.Page.PageType == PageTypes.FsIndex)
                    {
                        if (node.KeyIndex > keyindex)
                        {
                            node.KeyIndex--;
                            changed = true;
                        }
                    }
                    else if (node.Page.PageType == PageTypes.DataLeaf || node.Page.PageType == PageTypes.FsLeaf)
                    {
                        if (node.KeyIndex > keyindex)
                        {
                            node.KeyIndex--;
                            changed = true;
                        }
                        else if (node.KeyIndex == keyindex)
                        {
                            ApplyDeleteHandling();
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// manage DeleteHandling
        /// </summary>
        internal void ApplyDeleteHandling()
        {
            Perf.CallCount();

            //Validate();

            if (DeleteHandling == DeleteHandling.Invalidate)
            {
                CurrentPath.Invalidate();
            }
            else if (DeleteHandling == DeleteHandling.MoveToNext)
            {
                ref var node = ref CurrentPath.GetNode(CurrentPath.Last);
                ref var page = ref node.Page.AsContentPage;
                if (node.KeyIndex >= page.EntryCount)
                {
                    if (!MoveToNextKey())
                    {
                        CurrentPath.Invalidate();
                    }
                }
            }
            else if (DeleteHandling == DeleteHandling.MoveToPrevious)
            {
                if (!MoveToPrevKey())
                {
                    CurrentPath.Invalidate();
                }
            }
        }

        internal bool AdjustInsertKey(KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == pageno)
                {
                    if (node.Page.PageType == PageTypes.DataIndex || node.Page.PageType == PageTypes.FsIndex)
                    {
                        if (node.KeyIndex > keyindex) // TODO check > or >=
                        {
                            node.KeyIndex++;
                            changed = true;
                        }
                        else if (node.KeyIndex == keyindex)
                        {
                            ref var ipage = ref node.Page.AsContentPage;
                            ref var next = ref CurrentPath.GetNode(i + 1);
                            if (next.Page.PageNumber == ipage.GetLeftBranch(node.KeyIndex))
                            {
                                // do nothing
                            }
                            else if (next.Page.PageNumber == ipage.GetRightBranch(node.KeyIndex))
                            {
                                node.KeyIndex++;
                                changed = true;
                            }
                            else
                            {
                                Logger.LogDebug(LogTopics.Tracking, "Page not found:\n{0}", KvDebug.GetPageInfo(node.Page));

                                throw new KeyValiumException(ErrorCodes.PageNotFound, "Page not found.");
                            }
                        }
                    }
                    else if (node.Page.PageType == PageTypes.DataLeaf || node.Page.PageType == PageTypes.FsLeaf)
                    {
                        if (node.KeyIndex >= keyindex)
                        {
                            node.KeyIndex++;
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        internal bool AdjustUpdateKey(KvPagenumber pageno, int keyindex)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            // do nothing

            return changed;
        }

        internal bool AdjustSplit(KvPagenumber leftpageno, KvPagenumber rightpageno, ushort splitindex)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageType == PageTypes.DataIndex || node.Page.PageType == PageTypes.FsIndex)
                {
                    if (node.Page.PageNumber == leftpageno)
                    {
                        if (node.KeyIndex > splitindex)
                        {
                            using (var page = CurrentTransaction.GetPage(rightpageno, true, out _))
                            {
                                node.Page = page;
                            }
                            node.KeyIndex -= splitindex + 1;
                            changed = true;
                        }
                    }
                }
                else if (node.Page.PageType == PageTypes.DataLeaf || node.Page.PageType == PageTypes.FsLeaf)
                {
                    if (node.Page.PageNumber == leftpageno)
                    {
                        if (node.KeyIndex >= splitindex)
                        {
                            using (var page = CurrentTransaction.GetPage(rightpageno, true, out _))
                            {
                                node.Page = page;
                            }
                            node.KeyIndex -= splitindex;
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        internal bool AdjustMerge(KvPagenumber targetpageno, KvPagenumber mergeepageno, ushort targetkeycountbefore)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageType == PageTypes.DataIndex || node.Page.PageType == PageTypes.FsIndex)
                {
                    if (node.Page.PageNumber == mergeepageno)
                    {
                        using (var page = CurrentTransaction.GetPage(targetpageno, true, out _))
                        {
                            node.Page = page;
                        }
                        node.KeyIndex += targetkeycountbefore + 1;
                        changed = true;
                    }
                }
                else if (node.Page.PageType == PageTypes.DataLeaf || node.Page.PageType == PageTypes.FsLeaf)
                {
                    if (node.Page.PageNumber == mergeepageno)
                    {
                        using (var page = CurrentTransaction.GetPage(targetpageno, true, out _))
                        {
                            node.Page = page;
                        }
                        node.KeyIndex += targetkeycountbefore;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        internal bool AdjustDeleteTree(KvPagenumber pageno)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == pageno)
                {
                    CurrentPath.Invalidate();
                    changed = true;
                    break;
                }
            }

            return changed;
        }

        internal bool AdjustDeletePage(KvPagenumber pageno)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == pageno)
                {
                    CurrentPath.Current = i;
                    CurrentPath.Remove();
                    changed = true;
                    break;

                    //Path.RemoveNodeAt(i);
                    //i--;
                }
            }

            return changed;
        }

        internal bool AdjustInsertPage(KvPagenumber leftpageno, KvPagenumber rightpageno, KvPagenumber newpageno)
        {
            Perf.CallCount();

            var changed = false;

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            for (int i = CurrentPath.First; i <= CurrentPath.Last; i++)
            {
                ref var node = ref CurrentPath.GetNode(i);

                if (node.Page.PageNumber == leftpageno || node.Page.PageNumber == rightpageno)
                {
                    // save current node
                    var oldcurrent = CurrentPath.Current;

                    // TODO Test
                    CurrentPath.Current = i;
                    using (var page = CurrentTransaction.GetPage(newpageno, true, out _))
                    {
                        CurrentPath.Insert(page, 0);
                    }

                    // restore current node
                    CurrentPath.Current = oldcurrent;

                    if (oldcurrent >= i)
                    {
                        // account for inserted node
                        CurrentPath.Current++;
                    }

                    changed = true;

                    // account for inserted node
                    i++;
                }
            }

            return changed;
        }

        #endregion

        public bool SetPositionEx(CursorPositions pos, ref ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            lock (CurrentTransaction.TxLock)
            {
                if (pos == CursorPositions.Key)
                {
                    return SetPosition(key);
                }
                else
                {
                    return SetPosition(pos);
                }
            }
        }

        [SkipLocalsInit]
        internal bool SetPosition(CursorPositions pos)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            CurrentPath.Initialize(true);

            if (pos == CursorPositions.BeforeFirst)
            {
                IsBOF = true;
                IsEOF = false;

                return true;
            }
            else if (pos == CursorPositions.BehindLast)
            {
                IsBOF = false;
                IsEOF = true;

                return true;
            }

            throw new KeyValiumException(ErrorCodes.InternalError, "Invalid CursorPosition.");
        }

        [SkipLocalsInit]
        internal bool SetPosition(ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            CurrentPath.Initialize(true);

            var root = RootPagenumber;
            if (root == 0)
            {
                // Root does not exist: leave cursor empty
                return false;
            }

            return SeekToKey(root, ref key);

            //Timer.Start("SeekToKey");
            //var ret = SeekToKey(root, key);
            //Timer.Stop("SeekToKey");

            //return ret;
        }

        //internal bool SetPosition(CursorPositions pos, BytePointer key)
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(CurrentTransaction._lock.IsReadLocked, "Read lock not held!");

        //    CurrentKeyPath.Initialize();

        //    if (pos == CursorPositions.BeforeFirst)
        //    {
        //        IsBOF = true;
        //        IsEOF = false;

        //        return true;
        //    }

        //    if (pos == CursorPositions.BehindLast)
        //    {
        //        IsBOF = false;
        //        IsEOF = true;

        //        return true;
        //    }

        //    var root = RootPagenumber;
        //    if (root == 0)
        //    {
        //        // Root does not exist: leave cursor empty
        //        return false;
        //    }


        //    return SeekToKey(root, key);

        //    //Timer.Start("SeekToKey");
        //    //var ret = SeekToKey(root, key);
        //    //Timer.Stop("SeekToKey");

        //    //return ret;
        //}

        /// <summary>
        /// Begin of File
        /// </summary>
        public bool IsBOF
        {
            get;
            private set;
        }

        /// <summary>
        /// End of File
        /// </summary>
        public bool IsEOF
        {
            get;
            private set;
        }

        #region Cursor4Generic


        /// <summary>
        /// Builds the path to the given key starting with pageno
        /// </summary>
        /// <param name="pageno">Page number of the starting page. This should be the root page.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>Returns true when the cursor is positioned on the exact key.</returns>
        [SkipLocalsInit]
        internal bool SeekToKey(KvPagenumber pageno, ref ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            var exact = false;

            while (true)
            {
                using (var page = CurrentTransaction.GetPage(pageno, true, out _))
                {
                    ref var cpage = ref page.AsContentPage;

                    if (cpage.PageType != PageTypeIndex && cpage.PageType != PageTypeLeaf)
                    {
                        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
                    }

                    var index = cpage.GetKeyIndex(ref key, out exact);

                    CurrentPath.Append(page, (ushort)index);

                    if (page.PageType == PageTypeIndex)
                    {
                        pageno = cpage.GetLeftBranch(index);
                    }
                    else
                    {
                        return exact;
                    }
                }
            }
        }

        //private bool SeekToKey(KvPagenumber pageno, BytePointer key)
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(CurrentTransaction._lock.IsReadLocked, "Read lock not held!");

        //    //Validate();

        //    int index;
        //    var currentpage = pageno;
        //    var exact = true;

        //    while (true)
        //    {
        //        //Timer.Start("GetPage");
        //        var page = CurrentTransaction.GetPage(currentpage, true);
        //        //Timer.Stop("GetPage");

        //        var cpage = page.AsContentPage;

        //        if (!(page.PageType == PageTypeIndex || page.PageType == PageTypeLeaf))
        //        {
        //            throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
        //        }

        //        // search the first Keyindex that is greater than the given key
        //        // so we can always take the left branch
        //        index = page.PageType == PageTypeIndex ? cpage.GetKeyIndexGt(key) : cpage.GetKeyIndex(key, out exact);

        //        CurrentKeyPath.Append(page, index);

        //        if (page.PageType == PageTypeIndex)
        //        {
        //            currentpage = cpage.GetLeftBranch(index);
        //        }
        //        else
        //        {
        //            return exact;
        //        }
        //    }
        //}

        private bool SeekToFirst(KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            ushort index = 0;
            var currentpage = pageno;

            while (true)
            {
                using (var page = CurrentTransaction.GetPage(currentpage, true, out _))
                {
                    ref var cpage = ref page.AsContentPage;

                    if (!(page.PageType == PageTypeIndex || page.PageType == PageTypeLeaf))
                    {
                        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
                    }

                    CurrentPath.Append(page, index);

                    if (page.PageType == PageTypeIndex)
                    {
                        currentpage = cpage.GetLeftBranch(index);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        private bool SeekToLast(KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            ushort index;
            var currentpage = pageno;

            while (true)
            {
                using (var page = CurrentTransaction.GetPage(currentpage, true, out _))
                {
                    ref var cpage = ref page.AsContentPage;

                    if (!(page.PageType == PageTypeIndex || page.PageType == PageTypeLeaf))
                    {
                        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
                    }

                    index = page.PageType == PageTypeIndex ? cpage.EntryCount : (ushort)(cpage.EntryCount - 1);

                    CurrentPath.Append(page, index);

                    if (page.PageType == PageTypeIndex)
                    {
                        currentpage = cpage.GetLeftBranch(index);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// moves the cursor to the next key
        /// </summary>
        /// <returns></returns>
        public bool MoveToNextKey()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            if (IsEOF)
            {
                return false;
            }

            if (IsBOF)
            {
                var root = RootPagenumber;
                if (root == 0)
                {
                    IsBOF = true;
                    IsEOF = true;

                    return false;
                }

                // TODO check for MoveToKey after end of iteration
                IsBOF = false;
                return SeekToFirst(root);
            }

            CurrentPath.MoveLast();

            while (true)
            {
                ref var currentnode = ref CurrentPath.CurrentItem;

                if (currentnode.Page.PageType == PageTypeIndex)
                {
                    ref var ipage = ref currentnode.Page.AsContentPage;

                    if (currentnode.KeyIndex < ipage.EntryCount)
                    {
                        currentnode.KeyIndex++;
                        CurrentPath.Cutoff();
                        SeekToFirst(ipage.GetLeftBranch(currentnode.KeyIndex));
                        return true;
                    }

                    if (!CurrentPath.MovePrevious())
                    {
                        IsEOF = true;
                        return false;
                    }
                }
                else if (currentnode.Page.PageType == PageTypeLeaf)
                {
                    ref var leaf = ref currentnode.Page.AsContentPage;
                    if (currentnode.KeyIndex < leaf.EntryCount - 1)
                    {
                        currentnode.KeyIndex++;
                        return true;
                    }

                    if (!CurrentPath.MovePrevious())
                    {
                        IsEOF = true;
                        return false;
                    }
                }
                else
                {
                    throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + currentnode.Page.PageType.ToString());
                }
            }
        }

        /// <summary>
        /// moves the cursor to the previous key
        /// </summary>
        /// <returns></returns>
        public bool MoveToPrevKey()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            if (IsBOF)
            {
                return false;
            }

            if (IsEOF)
            {
                var root = RootPagenumber;
                if (root == 0)
                {
                    IsBOF = true;
                    IsEOF = true;

                    return false;
                }

                // TODO check for MoveToKey after end of iteration
                IsEOF = false;
                return SeekToLast(root);
            }

            CurrentPath.MoveLast();

            while (true)
            {
                ref var currentnode = ref CurrentPath.CurrentItem;

                if (currentnode.Page.PageType == PageTypeIndex)
                {
                    ref var ipage = ref currentnode.Page.AsContentPage;

                    if (currentnode.KeyIndex > 0)
                    {
                        currentnode.KeyIndex--;
                        CurrentPath.Cutoff();
                        SeekToLast(ipage.GetLeftBranch(currentnode.KeyIndex));
                        return true;
                    }

                    if (!CurrentPath.MovePrevious())
                    {
                        IsBOF = true;
                        return false;
                    }
                }
                else if (currentnode.Page.PageType == PageTypeLeaf)
                {
                    //var leaf = CurrentNodeList.Current.Page.AsContentPage;
                    if (currentnode.KeyIndex > 0)
                    {
                        currentnode.KeyIndex--;
                        return true;
                    }
                    if (!CurrentPath.MovePrevious())
                    {
                        IsBOF = true;
                        return false;
                    }
                }
                else
                {
                    throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + currentnode.Page.PageType.ToString());
                }
            }
        }

        public byte[] GetCurrentKey()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            try
            {
                //Validate();

                ref var currentnode = ref CurrentPath.CurrentItem;

                KvDebug.Assert(currentnode.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
                ref var leaf = ref currentnode.Page.AsContentPage;
                return leaf.GetKey(currentnode.KeyIndex);
            }
            finally
            {
            }
        }

        public ReadOnlySpan<byte> GetCurrentKeySpan()
        {
            Perf.CallCount();
            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var currentnode = ref CurrentPath.CurrentItem;
            KvDebug.Assert(currentnode.Page.PageType == PageTypeLeaf, "DataLeaf expected.");

            ref var leaf = ref currentnode.Page.AsContentPage;

            return leaf.GetKeyBytesAt(currentnode.KeyIndex).ReadOnlySpan;
        }

        public EntryExtern GetCurrentLeafEntry()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            try
            {
                //Validate();

                ref var currentnode = ref CurrentPath.CurrentItem;

                KvDebug.Assert(currentnode.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
                ref var leaf = ref currentnode.Page.AsContentPage;
                return leaf.GetEntry(currentnode.KeyIndex);
            }
            finally
            {
            }
        }

        private byte[] GetParentIndexKey(int relkeyoffset)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            // TODO check that prev is an indexpage (treerefs)
            if (CurrentPath.HasPrevItem)
            {
                ref var prevnode = ref CurrentPath.PrevItem;

                KvDebug.Assert(prevnode.Page.PageType == PageTypeIndex, "IndexPage expected.");
                ref var ipage = ref prevnode.Page.AsContentPage;
                return ipage.GetKeyBytesAt(prevnode.KeyIndex + relkeyoffset).ToArray();
            }

            return null;
        }

        private AnyPage GetRightSibling()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            // TODO check that prev is an indexpage (treerefs)
            if (CurrentPath.HasPrevItem)
            {
                ref var prevnode = ref CurrentPath.PrevItem;

                KvDebug.Assert(prevnode.Page.PageType == PageTypeIndex, "IndexPage expected.");
                ref var ipage = ref prevnode.Page.AsContentPage;
                if (prevnode.KeyIndex < ipage.EntryCount)
                {
                    return CurrentTransaction.GetPage(ipage.GetRightBranch(prevnode.KeyIndex), true, out _);
                }
            }

            return null;
        }

        private AnyPage GetLeftSibling()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            // TODO check that prev is an indexpage (treerefs)
            if (CurrentPath.HasPrevItem)
            {
                ref var prevnode = ref CurrentPath.PrevItem;

                KvDebug.Assert(prevnode.Page.PageType == PageTypeIndex, "IndexPage expected.");
                ref var ipage = ref prevnode.Page.AsContentPage;
                if (prevnode.KeyIndex > 0)
                {
                    return CurrentTransaction.GetPage(ipage.GetLeftBranch(prevnode.KeyIndex - 1), true, out _);
                }
            }

            return null;
        }

        internal bool CurrentHasSubtreeFlag()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            ref var node = ref CurrentPath.CurrentItem;

            ref var page = ref node.Page.AsContentPage;

            // TODO refactor
            var entry = page.GetEntryAt(node.KeyIndex);
            return (entry.Flags & EntryFlags.HasSubtree) != 0;
        }

        internal void SetSubTree(ref ContentPage leaf, int keyindex, KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");
            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            //Validate();

            // TODO refactor
            var entry = leaf.GetEntryAt(keyindex);
            entry.SubTree = pageno;
        }

        internal void SetCurrentSubTree(KvPagenumber pageno)
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            entry.SubTree = pageno;
        }

        internal void SetCurrentTotalCount(ulong count)
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            entry.TotalCount = count;
        }

        internal void SetCurrentLocalCount(ulong count)
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            entry.LocalCount = count;
        }

        internal KvPagenumber? GetCurrentSubTree()
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            return entry.SubTree;
        }

        internal ulong GetCurrentTotalCount()
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            return entry.TotalCount;
        }

        internal ulong GetCurrentLocalCount()
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            return entry.LocalCount;
        }

        internal void GetCurrentEntryInfo(out KvPagenumber? pageno, out ulong totalcount, out ulong localcount, out KvPagenumber ovpageno)
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);

            pageno = entry.SubTree;
            totalcount = entry.TotalCount;
            localcount = entry.LocalCount;
            ovpageno = entry.OverflowPageNumber;
        }

        internal KvPagenumber GetCurrentValueOverflow()
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");
            ref var leaf = ref node.Page.AsContentPage;

            KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            var entry = leaf.GetEntryAt(node.KeyIndex);
            return entry.OverflowPageNumber;
        }

        public ValueRef GetCurrentValue()
        {
            Perf.CallCount();

            //Validate();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            ref var node = ref CurrentPath.CurrentItem;

            KvDebug.Assert(node.Page.PageType == PageTypeLeaf, "DataLeaf expected.");

            //var leaf = node.Page.AsContentPage;
            //KvDebug.Assert(leaf.PageType == PageTypes.DataLeaf, "Wrong pagetype!");

            // TODO refactor
            //var entry = leaf.GetEntryAt(node.KeyIndex);


            return new ValueRef(CurrentTransaction, node.Page, node.KeyIndex);
        }

        private void UpdateCount(long totaldelta, long localdelta)
        {
            Perf.CallCount();

            if (IsFreeSpaceCursor)
            {
                CurrentTransaction.Meta.FsTotalCount += (ulong)totaldelta;
                CurrentTransaction.Meta.FsLocalCount += (ulong)localdelta;

                return;
            }

            if (TreeRef != null)
            {
                var kpath = TreeRef.Cursor.CurrentPath;
                for (int i = kpath.Last; i >= kpath.First; i--)
                {
                    ref var node = ref kpath.GetNode(i);

                    if (node.Page.PageType == PageTypeLeaf)
                    {
                        ref var cp = ref node.Page.AsContentPage;
                        var le = cp.GetEntryAt(node.KeyIndex);
                        le.TotalCount += (ulong)totaldelta;
                        le.LocalCount += (ulong)localdelta;

                        // TODO check performance
                        // LocalCount is only updated at the direct parent
                        localdelta = 0;
                    }
                }
            }

            // update Meta in any case
            CurrentTransaction.Meta.DataTotalCount += (ulong)totaldelta;
            CurrentTransaction.Meta.DataLocalCount += (ulong)localdelta;
        }

        #endregion

        public void Validate()
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating Cursor {0}.", Oid);

            if (_isdisposed)
            {
                throw new ObjectDisposedException("Cursor", "Cursor is already disposed.");
            }

            if (!PathChain.Items.HasCurrent)
            {
                throw new KeyValiumException(ErrorCodes.InvalidCursor, "The cursor is invalid.");
            }

            TreeRef?.Cursor.Validate();

            CurrentPath?.Validate();
        }

        public void Invalidate()
        {
            Perf.CallCount();

            while (PathChain.Items.HasCurrent)
            {
                PathChain.Items.CurrentItem.Path.Invalidate();
                PathChain.Items.Remove();
            }
        }

        #region Touch

        /// <summary>
        /// makes copies of the pages referenced by the cursor and adjusts the references to pagenumbers
        /// copies are added to DirtyPages
        /// copied Pages are added to FreePages
        /// assumptions: 1. cursor is not empty
        ///              2. cursor points to a leafnode
        /// </summary>
        internal void Touch(bool forceupdatepagenos)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            //Validate();

            TreeRef?.Touch();

            if (!CurrentPath.HasCurrent)
            {
                // TODO set Rootpagenumber to 0 if cursor is empty
                // TODO CHECK set rootpage to zero if forceupdatepagenos
                if (forceupdatepagenos)
                {
                    RootPagenumber = 0;
                }

                return;
            }

            //KvDebug.Assert(object.ReferenceEquals(CurrentNodeList.Current, CurrentNodeList.Last), "Nodelist not at last!");
            //cursor.Path.MoveToNode(CursorNodeMoves.Last);

            var prevchanged = false;

            // make pages dirty
            for (var i = CurrentPath.Current; i >= CurrentPath.First; i--)
            {
                ref var current = ref CurrentPath.GetNode(i);

                // make a writeable copy of page
                var newpage = CurrentTransaction.EnsureDirtyPage(this, current.Page, false);

                // TODO check if needed
                var changed = current.Page != newpage;

                // update node
                current.Page = newpage;

                KvDebug.Assert(CurrentTransaction.Pages.DirtyPages.Contains(current.Page.PageNumber), "Page is not dirty!");

                if (prevchanged || forceupdatepagenos)
                {
                    // adjust pagenumbers
                    if (i < CurrentPath.Current)
                    {
                        ref var next = ref CurrentPath.GetNode(i + 1);

                        switch (current.Page.PageType)
                        {
                            case PageTypes.DataIndex:
                            case PageTypes.FsIndex:
                                ref var ipage = ref current.Page.AsContentPage;
                                ipage.SetLeftBranch(current.KeyIndex, next.Page.PageNumber);
                                break;

                            case PageTypes.DataLeaf:
                                ref var leaf = ref current.Page.AsContentPage;
                                var le = leaf.GetEntryAt(current.KeyIndex);
                                if ((le.Flags & EntryFlags.HasSubtree) != 0)
                                {
                                    le.SubTree = next.Page.PageNumber;
                                }
                                else
                                {
                                    throw new Exception("KeyFlags not set correctly.");
                                }
                                break;

                            default:
                                throw new NotSupportedException("Cannot touch PageType.");
                        }
                    }
                }

                prevchanged = changed;

                if (prevchanged || forceupdatepagenos)
                {
                    // update RootPagenumber in last iteration
                    if (i == CurrentPath.First)
                    {
                        RootPagenumber = current.Page.PageNumber;
                    }
                }
            }
        }

        #endregion

        #region Content Manipulation (Insertion, Deletion)

        /// <summary>
        /// deletes the whole tree pointed to by treeref recursively
        /// if treeref is null the whole DataTree will be deleted
        /// starting from the root page the pagenumbers are collected and stored as free space
        /// </summary>
        internal bool DeleteTree()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            //Validate();

            var rootpageno = RootPagenumber;

            if (rootpageno != 0)
            {
                // touch KeyRef
                TreeRef?.Touch();

                // collect pagenumbers
                var ranges = CollectPages(rootpageno);

                // Adjust Cursors
                CurrentTransaction.ATODeleteTree(this, rootpageno);

                // clear RootpageNumber
                RootPagenumber = 0;

                // TODO test, remove cast
                // update counts
                UpdateCount(-(long)TotalCount, -(long)LocalCount);

                // add freepages
                foreach (var range in ranges.ToList())  // TODO optimize
                {
                    for (KvPagenumber p = range.First; p <= range.Last; p++)
                    {
                        CurrentTransaction.AddFreeOrLoosePage(p);
                    }
                }

                return true;
            }

            return false;
        }

        private PageRangeList CollectPages(KvPagenumber rootpageno)
        {
            Perf.CallCount();

            var ranges = new PageRangeList();

            if (rootpageno != 0)
            {
                var scanqueue = new Queue<KvPagenumber>();
                scanqueue.Enqueue(rootpageno);

                // TODO switch to PageRangeList
                var pagestoscan = new List<KvPagenumber>();

                // walk the tree *breadth first*
                // if the tree is walked depth first it can take hours to scan
                // an uncached big file on mechanical hard disks
                while (true)
                {
                    while (scanqueue.Count > 0)
                    {
                        var pageno = scanqueue.Dequeue();

                        using (var page = CurrentTransaction.GetPage(pageno, true, out _))
                        {

                            // add current page
                            ranges.AddPage(pageno);

                            // TODO deal with duplicate pages (should not happen)

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

                                case PageTypes.DataOverflow:
                                    ref var ovpage = ref page.AsOverflowPage;
                                    for (KvPagenumber p = pageno + 1; p < pageno + ovpage.Header.PageCount; p++)
                                    {
                                        ranges.AddPage(p);
                                    }
                                    break;

                                case PageTypes.FsLeaf:
                                    // nothing to do here

                                    break;

                                default:
                                    //throw new NotSupportedException("Unexpected Pagetype.");
                                    break;
                            }
                        }
                    }

                    if (pagestoscan.Count > 0)
                    {
                        pagestoscan = pagestoscan.OrderBy(x => x).ToList();
                        pagestoscan.ForEach(x => scanqueue.Enqueue(x));
                        pagestoscan.Clear();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ranges;
        }

        internal bool DeleteKey()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Write lock not held!");

            //Validate();

            var ret = DeleteKey(PageTypeLeaf, PageTypeIndex, true);

            if (ret)
            {
                UpdateCount(-1, -1);
            }

            return ret;
        }

        /// <summary>
        /// deletes a key and a value at cursor position
        /// assumptions: 1. cursor points to the correct location
        ///              2. key does exist
        /// </summary>        
        private bool DeleteKey(ushort pagetype, ushort pagetypeparent, bool touch,
                               bool nomerge = false, bool isupdate = false)
        {
            Perf.CallCount();

            if (CurrentPath.Current < 0)
            {
                // tree is empty
                return false;
            }

            if (touch)
            {
                // make a copy of all pages
                Touch(false);
            }

            ref var currentnode = ref CurrentPath.CurrentItem;

            KvDebug.Assert(currentnode.Page.PageType == pagetype, string.Format("Cursor not pointing to {0}.", pagetype));

            var merged = false;

            KvDebug.Assert(currentnode.Page.PageType == pagetype, string.Format("{0} expected.", pagetype));
            ref var page = ref currentnode.Page.AsContentPage;

            var ret = page.DeleteEntry(currentnode.KeyIndex);

            Logger.LogInfo(LogTopics.Delete, CurrentTransaction.Tid, "Deleted entry in page {0}.", page.Header.PageNumber);

            // TODO adjust this cursor
            // adjust other cursors
            if (!isupdate)
            {
                CurrentTransaction.ATODeleteKey(this, currentnode.Page.PageNumber, currentnode.KeyIndex);
            }

            // page.RawPage.Save(key[0]);

            if (nomerge)
            {
                return ret;
            }

            // if at least half of the space is free
            if (page.Header.FreeSpace >= page.Header.ContentSize >> 1)
            {
                // merge if possible
                if (CurrentPath.HasPrevOfType(pagetypeparent))
                {
                    byte[] indexkey;

                    if (!merged)
                    {
                        var rightsibling = GetRightSibling();

                        // prefer rightsibling
                        if (rightsibling != null)
                        {
                            ref var rightpage = ref rightsibling.AsContentPage;

                            EntryExtern entry = default;
                            var hasentry = false;
                            if (page.IsIndexPage)
                            {
                                indexkey = GetParentIndexKey(0);
                                entry = new EntryExtern(indexkey);
                                hasentry = true;
                            }

                            // check size of merged pages
                            var size = page.Header.UsedSpace + rightpage.Header.UsedSpace + (hasentry ? (entry.EntrySize + page.OffsetEntrySize) : 0);
                            if (size <= page.Header.ContentSize)
                            {
                                var leftkeycount = page.Header.KeyCount;

                                page.Merge(ref rightpage, hasentry, ref entry);
                                merged = true;

                                Logger.LogInfo(LogTopics.Merge, CurrentTransaction.Tid, "Merged page {0} into page {1}.", rightpage.Header.PageNumber, page.Header.PageNumber);

                                // adjust other cursors
                                CurrentTransaction.ATOMerge(this, page.Header.PageNumber, rightpage.Header.PageNumber, leftkeycount);

                                // mark mergee as free
                                CurrentTransaction.AddFreeOrLoosePage(rightsibling.PageNumber);
                            }

                            rightsibling.Dispose();
                        }
                    }

                    if (!merged)
                    {
                        var leftsibling = GetLeftSibling();

                        if (leftsibling != null)
                        {
                            ref var leftpage = ref leftsibling.AsContentPage;
                            var oldkeycount = leftpage.Header.KeyCount;

                            EntryExtern entry = default;
                            var hasentry = false;
                            if (page.IsIndexPage)
                            {
                                indexkey = GetParentIndexKey(-1);
                                entry = new EntryExtern(indexkey);
                                hasentry = true;
                            }

                            // check size of merged pages
                            var size = page.Header.UsedSpace + leftpage.Header.UsedSpace + (hasentry ? (entry.EntrySize + page.OffsetEntrySize) : 0);
                            if (size <= page.Header.ContentSize)
                            {
                                // TODO check "true" on call check what happens when page already dirty and gets disposed
                                leftsibling = CurrentTransaction.EnsureDirtyPage(this, leftsibling, true);
                                leftpage = ref leftsibling.AsContentPage;

                                leftpage.Merge(ref page, hasentry, ref entry);
                                merged = true;

                                Logger.LogInfo(LogTopics.Merge, CurrentTransaction.Tid, "Merged page {0} into page {1}.", page.Header.PageNumber, leftpage.Header.PageNumber);

                                // adjust other cursors
                                CurrentTransaction.ATOMerge(this, leftpage.Header.PageNumber, page.Header.PageNumber, oldkeycount);

                                // adjust cursor
                                var originalpageno = currentnode.Page.PageNumber;
                                currentnode.Page = leftsibling;
                                currentnode.KeyIndex += oldkeycount;
                                if (page.IsIndexPage)
                                {
                                    // account for pulled down indexkey
                                    currentnode.KeyIndex++;
                                }

                                // adjust index pointer
                                ref var prevnode = ref CurrentPath.PrevItem;
                                prevnode.KeyIndex--;

                                Touch(true);

                                // mark mergee as free
                                CurrentTransaction.AddFreeOrLoosePage(originalpageno);
                            }
                            else
                            {
                                leftsibling.Dispose();
                            }
                        }
                    }

                    // if merge happened then update index
                    if (merged)
                    {
                        CurrentPath.MovePrevious();
                        //var indexentry = page.Descriptor.CreateExternalEntry(indexkey);
                        DeleteKey(pagetypeparent, pagetypeparent, false);
                    }
                }
            }

            // check for empty node
            if (!merged && page.Header.KeyCount == 0)
            {
                // TODO check
                if (!CurrentPath.HasPrevItem || CurrentPath.HasPrevOfType(PageTypeLeaf))
                {
                    // update parent Pointers
                    //if (!CurrentKeyPath.HasPrevious())
                    //{
                    //    // TODO get rid of real rootpage is assigned in Touch(true)
                    //    RootPagenumber = 0;
                    //}
                    //else if (CurrentKeyPath.HasPreviousOfType(PageTypeLeaf))
                    //{
                    //    // TODO get rid of (should never execute)
                    //    ref var prevnode = ref CurrentKeyPath.PreviousPointer;
                    //    var leaf = prevnode.Page.AsContentPage;
                    //    SetSubTree(leaf, prevnode.KeyIndex, 0);
                    //}

                    var deletedpageno = CurrentPath.Remove2();

                    // update other cursors
                    CurrentTransaction.ATODeletePage(this, deletedpageno);

                    // update references
                    Touch(true);
                    CurrentTransaction.AddFreeOrLoosePage(deletedpageno);
                }
                else if (CurrentPath.HasCurrentOfType(PageTypeIndex) && CurrentPath.HasPrevOfType(PageTypeIndex))
                {
                    if (!VirtualMergeSplit(ref page, merged))
                    {
                        throw new KeyValiumException(ErrorCodes.InternalError, "VirtualMergeSplit failed!");
                    }
                }
                else
                {
                    throw new KeyValiumException(ErrorCodes.UnhandledEmptyPage, "Unhandled empty page.");
                }
            }

            return ret;
        }

        private bool VirtualMergeSplit(ref ContentPage page, bool merged)
        {
            Perf.CallCount();

            // do the Virtual-Merge-Split-Maneuver (currently only for one key)
            // TODO optimize for more keys

            ref var prevnode = ref CurrentPath.PrevItem;
            ref var nextnode = ref CurrentPath.NextItem;

            ref var parentpage = ref prevnode.Page.AsContentPage;

            // prefer rightsibling
            if (!merged)
            {
                var rightsibling = GetRightSibling();

                if (rightsibling != null)
                {
                    Logger.LogInfo(LogTopics.Merge | LogTopics.Split, CurrentTransaction.Tid, "Virtual-Merge-Split: Starting...");

                    // extract parentkey
                    var parententry = parentpage.GetEntryAt(prevnode.KeyIndex).ToEntryExtern();

                    // make dirty
                    var leftpage = page;
                    // TODO check "true" on call check what happens when page already dirty and gets disposed
                    rightsibling = CurrentTransaction.EnsureDirtyPage(this, rightsibling, true);
                    ref var rightpage = ref rightsibling.AsContentPage;

                    //Debugging.Helpers.DumpPage(parentpage, "Before Parent");
                    //Debugging.Helpers.DumpPage(leftpage, "Before Left");
                    //Debugging.Helpers.DumpPage(rightpage, "Before Right");
                    //Debugging.Helpers.DumpCursor(this, "Cursor Before");

                    KvDebug.Assert(rightpage.EntryCount > 2, "Not enough entries.");

                    // TODO extract key only
                    var rightentry = rightpage.GetEntryAt(0).ToEntryExtern();
                    var rightentryRightPage = rightpage.GetRightBranch(0);

                    var newleftentry = new EntryExtern(parententry.Key);
                    var newleftentryLeftPage = nextnode.Page.PageNumber;
                    var newleftentryRightPage = rightpage.GetLeftBranch(0);

                    var newparententry = new EntryExtern(rightentry.Key);
                    var newparententryLeftPage = leftpage.Header.PageNumber;
                    var newparententryRightPage = rightpage.Header.PageNumber;

                    // remove first entry from rightpage
                    rightpage.DeleteEntry(0);
                    // correct left branch
                    rightpage.SetLeftBranch(0, rightentryRightPage);

                    // add new entry to left page
                    leftpage.InsertEntry(0, ref newleftentry, newleftentryLeftPage, newleftentryRightPage);

                    // virtual merge
                    CurrentTransaction.ATOMerge(this, leftpage.Header.PageNumber, rightpage.Header.PageNumber, 0);

                    // update parent key
                    SavePagenumber();
                    if (CurrentPath.MovePrevious())
                    {
                        DeleteKey(PageTypeIndex, PageTypeIndex, false, true);
                    }
                    else
                    {
                        throw new KeyValiumException(ErrorCodes.VMSFailed, "Virtual-Merge-Split-Maneuver failed. Parent node is gone.");
                    }
                    RestorePagenumber();

                    // virtual split
                    CurrentTransaction.ATOSplit(this, leftpage.Header.PageNumber, rightpage.Header.PageNumber, 1);

                    SavePagenumber();
                    if (CurrentPath.MovePrevious())
                    {
                        InsertKey(ref newparententry, PageTypeIndex, PageTypeIndex, false, newparententryLeftPage, newparententryRightPage);
                    }
                    else
                    {
                        throw new KeyValiumException(ErrorCodes.VMSFailed, "Virtual-Merge-Split-Maneuver failed. Parent node is gone.");
                    }
                    RestorePagenumber();

                    merged = true;

                    //Debugging.Helpers.DumpPage(parentpage, "After Parent");
                    //Debugging.Helpers.DumpPage(leftpage, "After Left");
                    //Debugging.Helpers.DumpPage(rightpage, "After Right");
                    //Debugging.Helpers.DumpCursor(this, "Cursor After");

                    Logger.LogInfo(LogTopics.Merge | LogTopics.Split, CurrentTransaction.Tid, "Virtual-Merge-Split: Moved first key from page {0} to page {1}.", rightpage.Header.PageNumber, leftpage.Header.PageNumber);
                }
            }

            if (!merged)
            {
                var leftsibling = GetLeftSibling();

                if (leftsibling != null)
                {
                    Logger.LogInfo(LogTopics.Merge | LogTopics.Split, CurrentTransaction.Tid, "Virtual-Merge-Split: Starting...");

                    //CurrentTransaction.DumpTree(@"d:\!keyvalium\before.dump", false);

                    // move cursor to left parentnode
                    prevnode.KeyIndex--;

                    // extract parentkey
                    var parententry = parentpage.GetEntryAt(prevnode.KeyIndex).ToEntryExtern();

                    // make dirty
                    var rightpage = page;
                    // TODO check "true" on call check what happens when page already dirty and gets disposed
                    leftsibling = CurrentTransaction.EnsureDirtyPage(this, leftsibling, true);
                    ref var leftpage = ref leftsibling.AsContentPage;
                    ushort keycountbefore = (ushort)leftpage.EntryCount;

                    //Debugging.Helpers.DumpPage(parentpage, "Before Parent");
                    //Debugging.Helpers.DumpPage(leftpage, "Before Left");
                    //Debugging.Helpers.DumpPage(rightpage, "Before Right");
                    //Debugging.Helpers.DumpCursor(this, "Cursor Before");

                    KvDebug.Assert(leftpage.EntryCount > 2, "Not enough entries.");

                    // TODO extract key only
                    var leftentry = leftpage.GetEntryAt(leftpage.EntryCount - 1).ToEntryExtern();
                    var leftentryRightPage = leftpage.GetRightBranch(leftpage.EntryCount - 1);

                    var newrightentry = new EntryExtern(parententry.Key);
                    var newrightentryLeftPage = leftentryRightPage;
                    var newrightentryRightPage = nextnode.Page.PageNumber;

                    var newparententry = new EntryExtern(leftentry.Key);
                    var newparententryLeftPage = leftpage.Header.PageNumber;
                    var newparententryRightPage = rightpage.Header.PageNumber;

                    // remove last entry from leftpage
                    leftpage.DeleteEntry(leftpage.EntryCount - 1);

                    // add new entry to right page
                    rightpage.InsertEntry(0, ref newrightentry, newrightentryLeftPage, newrightentryRightPage);

                    // adjust cursor
                    // TODO check if needed
                    ref var currentnode2 = ref CurrentPath.CurrentItem;
                    currentnode2.KeyIndex++;

                    // virtual merge
                    CurrentTransaction.ATOMerge(this, leftpage.Header.PageNumber, rightpage.Header.PageNumber, keycountbefore);

                    // update parent key
                    SavePagenumber();
                    if (CurrentPath.MovePrevious())
                    {
                        DeleteKey(PageTypeIndex, PageTypeIndex, false, true);
                    }
                    else
                    {
                        throw new KeyValiumException(ErrorCodes.VMSFailed, "Virtual-Merge-Split-Maneuver failed. Parent node is gone.");
                    }
                    RestorePagenumber();

                    // virtual split
                    CurrentTransaction.ATOSplit(this, leftpage.Header.PageNumber, rightpage.Header.PageNumber, (ushort)(keycountbefore - 1));

                    SavePagenumber();
                    if (CurrentPath.MovePrevious())
                    {
                        InsertKey(ref newparententry, PageTypeIndex, PageTypeIndex, false, newparententryLeftPage, newparententryRightPage);
                    }
                    else
                    {
                        throw new KeyValiumException(ErrorCodes.VMSFailed, "Virtual-Merge-Split-Maneuver failed. Parent node is gone.");
                    }
                    RestorePagenumber();

                    //Debugging.Helpers.DumpCursor(this, "Cursor After before touch");

                    // update references
                    // TODO fix
                    //Touch(true);

                    merged = true;

                    //Debugging.Helpers.DumpPage(parentpage, "After Parent");
                    //Debugging.Helpers.DumpPage(leftpage, "After Left");
                    //Debugging.Helpers.DumpPage(rightpage, "After Right");
                    //Debugging.Helpers.DumpCursor(this, "Cursor After");

                    //CurrentTransaction.DumpTree(@"d:\!keyvalium\after.dump", false);

                    Logger.LogInfo(LogTopics.Merge | LogTopics.Split, CurrentTransaction.Tid, "Virtual-Merge-Split: Moved last key from page {0} to page {1}.", leftpage.Header.PageNumber, rightpage.Header.PageNumber);
                }
            }

            return merged;
        }

        internal void InsertKey(ref EntryExtern item)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            InsertKey(ref item, PageTypeLeaf, PageTypeIndex, true);

            UpdateCount(+1, +1);
        }

        /// <summary>
        /// inserts a key and a value at cursor position
        /// assumptions: 1. cursor points to the correct location
        ///              2. key does not exist
        ///              3. key and value have correct size (check is done in transaction)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private void InsertKey(ref EntryExtern newentry, ushort pagetype, ushort pagetypeparent, bool touch,
                               KvPagenumber leftbranch = default, KvPagenumber rightbranch = default, bool isupdate = false)
        {
            Perf.CallCount();

            if (!CurrentPath.HasCurrent)
            {
                // create first LeafPage
                var page = CurrentTransaction.AllocatePage(pagetype, null);

                // update Cursor
                CurrentPath.Insert(page, 0);

                // update references
                Touch(true);
            }
            else if (touch)
            {
                KvDebug.Assert(CurrentPath.HasCurrentOfType(pagetype), string.Format("Cursor not pointing to {0}.", pagetype));

                // make a copy of all pages
                Touch(false);
            }

            KvDebug.Assert(CurrentPath.HasCurrentOfType(pagetype), string.Format("{0} expected", pagetype));

            ref var currentnode = ref CurrentPath.CurrentItem;

            var leftany = currentnode.Page;
            ref var leftpage = ref leftany.AsContentPage;
            var isleft = true;

            AnyPage rightany = null;
            KeyFromPool? splitkey = null;

            // check free space
            if (leftpage.Header.FreeSpace < (newentry.EntrySize + leftpage.BranchSize + leftpage.OffsetEntrySize))
            {
                //if (leftpage.Header.PageType==PageTypes.FsLeaf)
                //{
                //    Console.WriteLine();
                //}

                // allocate empty page
                rightany = CurrentTransaction.AllocatePage(pagetype, null);

                // split content
                (isleft, var newkeyindex, var splitindex, splitkey) = leftpage.Split(Pool, ref rightany.AsContentPage, currentnode.KeyIndex, CurrentTransaction.AppendMode);

                Logger.LogInfo(LogTopics.Split, CurrentTransaction.Tid, "Splitted page {0} into page {1}.", leftany.PageNumber, rightany.PageNumber);

                if (isupdate && isleft)
                {
                    // if update and the key is inserted left then account for the already deleted key
                    splitindex++;
                }

                // adjust other cursors
                CurrentTransaction.ATOSplit(this, leftany.PageNumber, rightany.PageNumber, splitindex);

                // update cursor
                if (!isleft)
                {
                    currentnode.Page = rightany;
                }

                currentnode.KeyIndex = newkeyindex;
            }

            // update insertpage
            ref var insertpage = ref leftpage;
            if (!isleft)
            {
                insertpage = ref rightany.AsContentPage;
            }

            insertpage.InsertEntry(currentnode.KeyIndex, ref newentry, leftbranch, rightbranch);

            Logger.LogInfo(LogTopics.Insert, CurrentTransaction.Tid, "Inserted entry in page {0}.", GetInsertInfo(ref insertpage, currentnode.KeyIndex, newentry, leftbranch, rightbranch));

            //CurrentTransaction.DumpTree(@"d:\!keyvalium\after insert.dump", false);

            // adjust other cursors
            if (isupdate)
            {
                CurrentTransaction.ATOUpdateKey(this, insertpage.Header.PageNumber, currentnode.KeyIndex);
            }
            else
            {
                CurrentTransaction.ATOInsertKey(this, insertpage.Header.PageNumber, currentnode.KeyIndex);
            }

            if (pagetype == PageTypeIndex)
            {
                ref var nextnode = ref CurrentPath.NextItem;
                if (nextnode.Page.PageNumber == rightbranch)
                {
                    // update KeyIndex, cursors always point to the next greater index node
                    currentnode.KeyIndex++;
                }
            }

            // if split happened, update index
            if (rightany != null)
            {
                ReadOnlySpan<byte> indexkey = default;

                // only Indexpages return splitkey
                if (!splitkey.HasValue)
                {
                    // TODO optimize
                    indexkey = rightany.AsContentPage.GetKeyBytesAt(0).ReadOnlySpan;
                }

                //ContentPage indexpage;

                // update or create parent IndexPage
                if (!CurrentPath.HasPrevOfType(pagetypeparent))
                {
                    // create new IndexPage
                    var page = CurrentTransaction.AllocatePage(pagetypeparent, null);

                    // update Cursor
                    CurrentPath.Insert(page, 0);

                    //indexpage = page.AsContentPage;

                    // update other cursors
                    CurrentTransaction.ATOInsertPage(this, leftany.PageNumber, rightany.PageNumber, page.PageNumber);

                    //// update references
                    //Touch(true);

                    // update parent Pointers
                    if (!CurrentPath.HasPrevItem)
                    {
                        RootPagenumber = page.PageNumber;
                    }
                    else if (CurrentPath.HasPrevOfType(PageTypeLeaf))
                    {
                        ref var prev = ref CurrentPath.PrevItem;

                        ref var leaf = ref prev.Page.AsContentPage;
                        SetSubTree(ref leaf, prev.KeyIndex, page.PageNumber);
                    }
                }
                else
                {
                    CurrentPath.MovePrevious();
                    //indexpage = CurrentNodeList.Current.Page.AsContentPage;
                }

                //KvDebug.Assert(splitkey != null, "SplitKey is null!");
                var indexentry = new EntryExtern(splitkey.HasValue ? splitkey.Value.Span : indexkey);
                InsertKey(ref indexentry, pagetypeparent, pagetypeparent, false, leftany.PageNumber, rightany.PageNumber);

                Pool.Return(splitkey);
            }
        }

        private static string GetInsertInfo(ref ContentPage cp, int keyindex, EntryExtern entry, KvPagenumber leftbranch, KvPagenumber rightbranch)
        {
            var ret = string.Format("Page: {0} Type: {1} KeyIndex: {2} Key: {3} Left: {4} Right: {5}",
                cp.Page.PageNumber,
                cp.Page.PageType,
                keyindex,
                Util.GetHexString(entry.Key),
                leftbranch, rightbranch);

            return ret;
        }

        internal void UpdateKey(ref EntryExtern item)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(CurrentTransaction.TxLock), "Read lock not held!");

            //Validate();

            if (!UpdateKey(ref item, PageTypeLeaf))
            {
                DeleteKey(PageTypeLeaf, PageTypeIndex, false, true, true);
                InsertKey(ref item, PageTypeLeaf, PageTypeIndex, false, default, default, true);
            }
        }

        /// <summary>
        /// updates a key and a value at cursor position
        /// assumptions: 1. cursor points to the correct location
        ///              2. key does exist
        ///              3. key and value have correct size (check is done in transaction)
        /// </summary>
        /// <returns>true if the update completed, false if the update failed (not enough freespace in page - delete-insert needs to be done)</returns>
        private bool UpdateKey(ref EntryExtern newentry, ushort pagetype)
        {
            Perf.CallCount();

            ref var currentnode = ref CurrentPath.CurrentItem;

            KvDebug.Assert(currentnode.Page.PageType == pagetype, string.Format("Cursor not pointing to {0}.", pagetype));

            // make a copy of all pages
            Touch(false);

            KvDebug.Assert(currentnode.Page.PageType == pagetype, string.Format("{0} expected", pagetype));

            var updateany = currentnode.Page;
            ref var updatepage = ref updateany.AsContentPage;

            var oldentrysize = updatepage.GetEntrySize(currentnode.KeyIndex);

            // check free space
            if ((updatepage.Header.FreeSpace + oldentrysize) < (newentry.EntrySize))
            {
                // not enough free space for update
                return false;
            }

            updatepage.UpdateEntry(currentnode.KeyIndex, ref newentry);

            // adjust other cursors
            CurrentTransaction.ATOUpdateKey(this, updateany.PageNumber, currentnode.KeyIndex);

            Logger.LogInfo(LogTopics.Update, CurrentTransaction.Tid, "Updated entry in page {0}.", updatepage.Header.PageNumber);

            return true;
        }

        #endregion

        #region IDisposable implementation

        private bool _isdisposed;

        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    if (Scope != InternalTrackingScope.None)
                    {
                        Database.Tracker.Remove(this);
                    }

                    // must be called last
                    Invalidate();
                }

                _isdisposed = true;
            }
        }

        public void Dispose()
        {
            Perf.CallCount();

            if (IsReusable)
            {
                return;
            }

            DisposeReal();
        }

        //~Cursor()
        //{
        //    if (!_isdisposed)
        //    {
        //        throw new NotSupportedException("Cursor not disposed!");
        //    }
        //}

        public void DisposeReal()
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

