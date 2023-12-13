
using KeyValium.Inspector;
using KeyValium.Pages;

namespace KeyValium.Collections
{
    /// <summary>
    /// Dictionary for pages and their states
    /// </summary>
    internal sealed class KvPageDictionary
    {
        internal KvPageDictionary()
        {
            Perf.CallCount();

            PagesAndState = new HashKeyValueAllocator<PageAndState>();

            DirtyPages = new KvHashSet();
            DirtyParentPages = new KvHashSet();
            SpilledPages = new KvHashSet();
            LoosePages = new PageRangeList();
            FreePages = new KvHashSet();
        }

        #region Variables

        /// <summary>
        /// the actual pages and their state
        /// </summary>
        internal readonly HashKeyValueAllocator<PageAndState> PagesAndState;

        /// <summary>
        /// Pages that are dirty (must be written)
        /// contains only the pagenumbers
        /// </summary>
        internal readonly KvHashSet DirtyPages;

        /// <summary>
        /// Pages from Parent Transactions that are dirty (must be written)
        /// contains only the pagenumbers
        /// </summary>
        internal readonly KvHashSet DirtyParentPages;

        /// <summary>
        /// Pages that were temporarily written
        /// contains only the pagenumbers
        /// </summary>
        internal readonly KvHashSet SpilledPages;

        /// <summary>
        /// dirty pages that became free, can be reused
        /// contains only the pagenumbers
        /// </summary>
        internal PageRangeList LoosePages;

        /// <summary>
        /// pages that became unused (added to freespace at commit)
        /// contains only the pagenumbers
        /// </summary>
        internal readonly KvHashSet FreePages;

        #endregion

        #region public API

        public int Count
        {
            get
            {
                Perf.CallCount();

                return PagesAndState.Count;
            }
        }

        public void Clear()
        {
            Perf.CallCount();

            void ClearPage(KvPagenumber pageno, ref PageAndState item)
            {
                item.Page = null;
            }

            // clear Pages because of reference counting
            PagesAndState.ForEach(ClearPage);
            PagesAndState.Clear();

            DirtyPages.Clear();
            DirtyParentPages.Clear();
            SpilledPages.Clear();
            LoosePages.Clear();
            FreePages.Clear();
        }

        internal void Insert(KvPagenumber pageno, AnyPage page, PageStates state)
        {
            Perf.CallCount();

            if (PagesAndState.Contains(pageno))
            {
                throw new NotSupportedException("Pagenumber already exists!");
            }

            var past = new PageAndState(page, state); // increments refcount of page

            PagesAndState.Add(pageno, ref past);

            ChangeState(pageno, PageStates.None, state);
        }

        internal void ChangeState(ulong pageno, ref PageAndState past, PageStates newstate)
        {
            ChangeState(pageno, past.State, newstate);

            past.State = newstate;

            switch (newstate)
            {
                case PageStates.Loose:
                case PageStates.Spilled:
                case PageStates.Free:
                    past.Page = null;
                    break;
            }
        }

        internal void ChangeState(ulong pageno, PageStates newstate)
        {
            ref var past = ref TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                ChangeState(pageno, ref past, newstate);
            }
            else
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "Page not found!");
            }
        }

        //internal Action<KvPagenumber>[] _statechanges = new Action<KvPagenumber>[6 * 6]
        //{
        //    // oldstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Dirty
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Free

        //    // oldstate = Dirty
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { ; }, // newstate = Dirty
        //    x=> { DirtyPages.Remove(pageno); }, // newstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Free

        //    // oldstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Dirty
        //    x=> { ; }, // newstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Free

        //    // oldstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Dirty
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = DirtyAtParent
        //    x=> { ; }, // newstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Free

        //    // oldstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Dirty
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Loose
        //    x=> { ; }, // newstate = Spilled
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Free

        //    // oldstate = Free
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = None
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Dirty
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = DirtyAtParent
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Loose
        //    x=> { throw new KeyValiumException(ErrorCodes.InternalError, "Cannot change state to None."); }, // newstate = Spilled
        //    x=> { ; }, // newstate = Free
        //};

        /// <summary>
        /// removes and inserts pageno from/to the appropriate collection
        /// </summary>
        /// <param name="pageno"></param>
        /// <param name="oldstate"></param>
        /// <param name="newstate"></param>
        /// <exception cref="KeyValiumException"></exception>
        private void ChangeState(ulong pageno, PageStates oldstate, PageStates newstate)
        {
            Perf.CallCount();

            if (oldstate == newstate)
            {
                return;
            }

            //var index = (int)oldstate * 6 + (int)newstate;
            //_statechanges[index].Invoke(pageno);

            switch (oldstate)
            {
                case PageStates.None:
                    // do nothing
                    break;
                case PageStates.Dirty:
                    DirtyPages.Remove(pageno);
                    break;
                case PageStates.DirtyAtParent:
                    DirtyParentPages.Remove(pageno);
                    break;
                case PageStates.Loose:
                    LoosePages.RemovePage(pageno);
                    break;
                case PageStates.Spilled:
                    SpilledPages.Remove(pageno);
                    break;
                case PageStates.Free:
                    throw new KeyValiumException(ErrorCodes.InternalError, "Free pages cannot change state.");
                default:
                    throw new KeyValiumException(ErrorCodes.InternalError, "Unhandled page state.");
            }

            switch (newstate)
            {
                case PageStates.None:
                    // do nothing
                    break;
                case PageStates.Dirty:
                    DirtyPages.Add(pageno);
                    break;
                case PageStates.DirtyAtParent:
                    DirtyParentPages.Add(pageno);
                    break;
                case PageStates.Loose:
                    LoosePages.AddPage(pageno);
                    break;
                case PageStates.Spilled:
                    SpilledPages.Add(pageno);
                    break;
                case PageStates.Free:
                    FreePages.Add(pageno);
                    break;
                default:
                    throw new KeyValiumException(ErrorCodes.InternalError, "Unhandled page state.");
            }
        }

        internal void Update(ulong pageno, AnyPage newpage, PageStates newstate)
        {
            Perf.CallCount();

            ref var past = ref TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                ChangeState(pageno, past.State, newstate);
                // set fields individually to make sure setter is called because of refcounting
                past.Page = newpage;
                past.State = newstate;
            }
            else
            {
                throw new NotSupportedException("Pagenumber does not exist!");
            }
        }

        private void Upsert(KvPagenumber pageno, ref PageAndState item)
        {
            Perf.CallCount();

            ref var past = ref TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                // Update
                ChangeState(pageno, past.State, item.State);

                // set fields individually to make sure setter is called because of refcounting
                past.Page = item.Page;
                past.State = item.State;
            }
            else
            {
                // Insert
                item.Page?.AddRef();
                PagesAndState.Add(pageno, ref item);

                ChangeState(pageno, PageStates.None, item.State);
            }
        }

        internal bool RemoveWithoutCleanup(KvPagenumber pageno)
        {
            Perf.CallCount();

            return PagesAndState.Remove(pageno);
        }

        internal bool RemoveWithoutCleanup(PageRange range)
        {
            Perf.CallCount();

            var ret = true;

            for (var pageno = range.First; pageno <= range.Last; pageno++)
            {
                ret &= PagesAndState.Remove(pageno);
            }

            return ret;
        }

        //public bool Remove(KvPagenumber pageno)
        //{
        //    Perf.CallCount();

        //    void Cleanup(KvPagenumber pageno, ref PageAndState past)
        //    {
        //        ChangeState(pageno, past.State, PageStates.None);
        //        past.Page = null;
        //    }

        //    return PagesAndState.Remove(pageno, Cleanup);
        //}

        public bool Contains(KvPagenumber pageno)
        {
            Perf.CallCount();

            return PagesAndState.Contains(pageno);
        }

        internal ref PageAndState TryGetValueRef(KvPagenumber pageno, out bool isvalid)
        {
            Perf.CallCount();

            return ref PagesAndState.TryGetValueRef(pageno, out isvalid);
        }

        internal AnyPage GetPage(KvPagenumber pageno)
        {
            Perf.CallCount();

            ref var past = ref PagesAndState.TryGetValueRef(pageno, out var isvalid);
            if (isvalid)
            {
                return past.Page;
            }

            return null;
        }

        internal List<KeyValuePair<KvPagenumber, PageAndState>> ToList()
        {
            Perf.CallCount();

            var _list = new List<KeyValuePair<KvPagenumber, PageAndState>>();

            void AddToList(KvPagenumber pageno, ref PageAndState item)
            {
                Perf.CallCount();

                _list.Add(new KeyValuePair<KvPagenumber, PageAndState>(pageno, item));
            }

            _list = new List<KeyValuePair<KvPagenumber, PageAndState>>();

            PagesAndState.ForEach(AddToList);

            var ret = _list;
            _list = null;

            return ret;
        }

        internal KvPagenumber? TakeLoosePage()
        {
            var ret = LoosePages.TakePage();
            if (ret.HasValue)
            {
                RemoveWithoutCleanup(ret.Value);
            }

            return ret;
        }

        internal bool TryTakeLooseRange(ulong start, ulong count, out PageRange range)
        {
            var ret = LoosePages.TryTakeRange(start, count, out range);
            if (ret)
            {
                RemoveWithoutCleanup(range);
            }

            return ret;
        }

        internal void ClearLoosePages()
        {
            var list = LoosePages.ToList();
            foreach (var range in list)
            {
                for (var pageno = range.First; pageno <= range.Last; pageno++)
                {
                    RemoveWithoutCleanup(pageno);
                }
            }

            LoosePages.Clear();
        }

        internal void ClearFreePages()
        {
            var list = FreePages.ToList();
            foreach (var pageno in list)
            {
                RemoveWithoutCleanup(pageno);
            }

            FreePages.Clear();
        }

        internal PageRangeList RemoveFreeAndLoosePages()
        {
            var ranges = LoosePages.Copy();

            ranges.AddRanges(FreePages.ToRangeList());

            ClearLoosePages();
            ClearFreePages();

            return ranges;
        }

        internal void CopyLoosePages(KvPageDictionary source)
        {
            if (LoosePages.RangeCount > 0)
            {
                throw new InvalidOperationException("LoosePages must be empty!");
            }

            LoosePages = source.LoosePages.Copy();

            var past = new PageAndState(null, PageStates.Loose);

            // TODO optimize
            foreach (var range in LoosePages.ToList())
            {
                for (var pageno = range.First; pageno <= range.Last; pageno++)
                {
                    PagesAndState.Add(pageno, ref past);
                }
            }
        }

        internal void UpdateWith(KvPageDictionary source)
        {
            // move loose pages
            //LoosePages = source.LoosePages;

            void Iterator(KvPagenumber pageno, ref PageAndState past)
            {
                if (past.State == PageStates.None)
                {
                    throw new InvalidOperationException("PageState must not be None!");
                }

                if (past.State != PageStates.DirtyAtParent)
                {
                    this.Upsert(pageno, ref past);
                }
            }

            source.PagesAndState.ForEach(Iterator);
        }

        internal List<KvPagenumber> GetDirtyPages(uint pagetype)
        {
            var list = new List<KvPagenumber>();

            void Filter(KvPagenumber pageno)
            {
                var page = GetPage(pageno);
                if (page.PageType == pagetype)
                {
                    list.Add(pageno);
                }
            }

            DirtyPages.ForEach(Filter);

            return list;
        }

        /// <summary>
        /// verify pages and their state
        /// check that all pages are in the correct collections
        /// </summary>
        internal void Validate()
        {
            var errors = new List<string>();

            void CheckPagesAndState(KvPagenumber pageno, ref PageAndState past)
            {
                switch (past.State)
                {
                    case PageStates.None:
                        errors.Add(string.Format("Page {0}: None is an invalid state.", pageno));
                        break;

                    case PageStates.Dirty:
                        if (past.Page == null)
                        {
                            errors.Add(string.Format("Page {0}: Dirty page is null.)", pageno));
                        }
                        if (!DirtyPages.Contains(pageno))
                        {
                            errors.Add(string.Format("Page {0}: Dirty page not in {1}.)", pageno, nameof(DirtyPages)));
                        }
                        break;

                    case PageStates.DirtyAtParent:
                        if (past.Page == null)
                        {
                            errors.Add(string.Format("Page {0}: Dirty parent page is null.)", pageno));
                        }
                        if (!DirtyParentPages.Contains(pageno))
                        {
                            errors.Add(string.Format("Page {0}: Dirty parent page not in {1}.)", pageno, nameof(DirtyParentPages)));
                        }
                        break;

                    case PageStates.Loose:
                        if (past.Page != null)
                        {
                            errors.Add(string.Format("Page {0}: Loose page is not null.)", pageno));
                        }
                        if (!LoosePages.Contains(pageno))
                        {
                            errors.Add(string.Format("Page {0}: Loose page not in {1}.)", pageno, nameof(LoosePages)));
                        }
                        break;

                    case PageStates.Spilled:
                        if (past.Page != null)
                        {
                            errors.Add(string.Format("Page {0}: Spilled page is not null.)", pageno));
                        }
                        if (!SpilledPages.Contains(pageno))
                        {
                            errors.Add(string.Format("Page {0}: Spilled page not in {1}.)", pageno, nameof(SpilledPages)));
                        }
                        break;

                    case PageStates.Free:
                        if (past.Page != null)
                        {
                            errors.Add(string.Format("Page {0}: Free page is not null.)", pageno));
                        }
                        if (!FreePages.Contains(pageno))
                        {
                            errors.Add(string.Format("Page {0}: Free page not in {1}.)", pageno, nameof(FreePages)));
                        }
                        break;
                }
            }

            PagesAndState.ForEach(CheckPagesAndState);

            CheckHashSet(DirtyPages, PageStates.Dirty, errors);
            CheckHashSet(DirtyParentPages, PageStates.DirtyAtParent, errors);
            CheckPageRangeList(LoosePages, PageStates.Loose, errors);
            CheckHashSet(SpilledPages, PageStates.Spilled, errors);
            CheckHashSet(FreePages, PageStates.Free, errors);

            if (errors.Count > 0)
            {
                var msg = string.Join("\n", errors);
                throw new KeyValiumException(ErrorCodes.InternalError, msg);
            }
        }

        private void CheckHashSet(KvHashSet hashset, PageStates expected, List<string> errors)
        {
            void Check(KvPagenumber pageno)
            {
                ref var past = ref TryGetValueRef(pageno, out var isvalid);
                if (!isvalid)
                {
                    errors.Add(string.Format("Page {0}: {1} page not in {2}.)", pageno, expected, nameof(PagesAndState)));
                }
                else
                {
                    if (past.State != expected)
                    {
                        errors.Add(string.Format("Page {0}: State mismatch. Expected: {1}. Actual: {2}.)", pageno, expected, past.State));
                    }
                }
            }

            hashset.ForEach(Check);
        }

        private void CheckPageRangeList(PageRangeList list, PageStates expected, List<string> errors)
        {
            void Check(KvPagenumber pageno)
            {
                ref var past = ref TryGetValueRef(pageno, out var isvalid);
                if (!isvalid)
                {
                    errors.Add(string.Format("Page {0}: {1} page not in {2}.)", pageno, expected, nameof(PagesAndState)));
                }
                else
                {
                    if (past.State != expected)
                    {
                        errors.Add(string.Format("Page {0}: State mismatch. Expected: {1}. Actual: {2}.)", pageno, expected, past.State));
                    }
                }
            }

            foreach (var range in list.ToList())
            {
                for (var pageno = range.First; pageno <= range.Last; pageno++)
                {
                    Check(pageno);
                }
            }
        }

        #endregion
    }
}
