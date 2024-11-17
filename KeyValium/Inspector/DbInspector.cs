using KeyValium.Collections;
using KeyValium.Memory;
using KeyValium.Options;
using KeyValium.Pages.Headers;

namespace KeyValium.Inspector
{
    public class DbInspector : IDisposable
    {
        internal DbInspector(string filename, string password, string keyfile)
        {
            var options = new DatabaseOptions();
            options.SharingMode = SharingModes.Exclusive;
            options.ReadOnly = true;
            options.Password = password;
            options.KeyFile = keyfile;

            //options.Password = "Hossa";

            _database = Database.Open(filename, options);
        }

        internal DbInspector(Database database)
        {
            _database = database;
        }

        private Database _database;

        private DatabaseProperties _props;

        public DatabaseProperties Properties
        {
            get
            {
                if (_props == null)
                {
                    _props = new DatabaseProperties();

                    _props.Filename = _database.Filename;
                    _props.FileSize = _database.DbFile.Length;
                    _props.FirstMetaPage = Limits.FirstMetaPage;
                    _props.Flags = (ushort)_database.Options.Flags;
                    _props.MaxKeyAndValueSize = _database.Limits.MaximumInlineKeyValueSize;
                    _props.MaxKeySize = _database.Limits.MaximumKeySize;
                    _props.MetaPages = Limits.MetaPages;
                    _props.MinKeysPerIndexPage = Limits.MinKeysPerIndexPage;
                    _props.MinKeysPerLeafPage = Limits.MinKeysPerLeafPage;
                    _props.PageSize = _database.Options.PageSize;
                    _props.Version = _database.Options.Version;
                    _props.InternalTypecode = _database.Options.InternalTypeCode;
                    _props.UserTypecode = _database.Options.UserTypeCode;

                    for (ushort i = 0; i < Properties.MetaPages; i++)
                    {
                        var meta = GetMeta(i);

                        _props.MetaInfos.Add(meta);
                    }
                }

                return _props;
            }
        }

        public MetaInfo GetMeta(ushort index)
        {
            if (index < 0 || index >= Properties.MetaPages)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var page = _database.Pager.ReadPageInspector(Properties.FirstMetaPage + index, Properties.PageSize, true);
            var mp = new MetaPage(page);

            var ret = new MetaInfo();

            ret.Index = index;
            ret.DataRootPage = mp.DataRootPage;
            ret.FsRootPage = mp.FsRootPage;
            ret.PageNumber = Properties.FirstMetaPage + index;
            ret.Tid = mp.Tid;
            ret.HeaderTid = mp.HeaderTid;
            ret.FooterTid = mp.FooterTid;
            ret.LastPage = mp.LastPage;
            ret.DataTotalCount = mp.DataTotalCount;
            ret.DataLocalCount = mp.DataLocalCount;
            ret.FsTotalCount = mp.FsTotalCount;
            ret.FsLocalCount = mp.FsLocalCount;

            page.Dispose();

            return ret;
        }

        public FileMap GetFileMap(IProgress<ulong> progress, CancellationToken canceltoken)
        {
            var ret = new FileMap();
            ret.TotalPageCount = (ulong)Properties.PageCount;

            ret.Add(-1, 0, PageTypesI.FileHeader, 0);

            var mintid = Properties.MetaInfos.Min(x => x.Tid);

            for (short i = 0; i < Properties.MetaInfos.Count; i++)
            {
                ret.Add(-1, Properties.FirstMetaPage + (ulong)i, PageTypesI.Meta, 0);

                var meta = Properties.MetaInfos[i];

                ScanTree(meta.DataRootPage, i, mintid, ret, progress, canceltoken);
                ScanTree(meta.FsRootPage, i, mintid, ret, progress, canceltoken);
            }

            return ret;
        }

        static internal PageRangeList GetPageRange(Database db, KvPagenumber rootpageno)
        {
            using (var dbi = new DbInspector(db))
            {
                var _canceltokensource = new CancellationTokenSource();

                var map = new FileMap();
                dbi.ScanTree(rootpageno, 0, 0, map, null, _canceltokensource.Token);

                // Avoid disposing the given database
                dbi._database = null;

                return map.GetPageList(0);
            }
        }

        /// <summary>
        /// Scans the tree and extracts different kinds of information
        /// </summary>
        /// <param name="rootpageno">Pagenumber of the starting page</param>
        /// <param name="metaindex"></param>
        /// <param name="mintid">Minimum Tid</param>
        /// <param name="map"></param>
        /// <param name="progress"></param>
        /// <param name="canceltoken"></param>
        private void ScanTree(KvPagenumber rootpageno, short metaindex, KvTid mintid, FileMap map,
                              IProgress<ulong> progress, CancellationToken canceltoken)
        {
            if (rootpageno == 0)
            {
                return;
            }

            var level = 0;      // for debugging purposes

            var scanqueue = new PageRangeList();
            scanqueue.AddPage(rootpageno);


            var pagestoscan = new List<KvPagenumber>();

            // walk the tree *breadth first*
            // if the tree is walked depth first it can take hours to scan
            // an uncached big file on mechanical hard disks
            while (true)
            {
                while (!scanqueue.IsEmpty)
                {
                    if (!scanqueue.TryTakePage(out var pageno))
                    {
                        throw new Exception("Unexpected empty queue!");
                    }

                    _database.Pager.ReadAheadInspector(pageno);
                    var page = _database.Pager.ReadPageInspector(pageno, Properties.PageSize, true);

                    // add current page
                    if (page.PageType != PageTypes.DataOverflow)
                    {
                        if (!map.Add(metaindex, pageno, GetPageTypeI(page), GetFreeSpace(page)))
                        {
                            // TODO deal with duplicate pages
                            continue;
                        }
                    }

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

                            map.Add(metaindex, pageno, PageTypesI.DataOverflow, 0,
                                    GetRangeKind(pageno, ovpage.Header.PageNumber, ovpage.Header.PageCount));

                            for (KvPagenumber p = pageno + 1; p < pageno + ovpage.Header.PageCount; p++)
                            {
                                map.Add(metaindex, p, PageTypesI.DataOverflowCont, 0,
                                        GetRangeKind(p, ovpage.Header.PageNumber, ovpage.Header.PageCount));
                            }

                            break;

                        case PageTypes.FsLeaf:

                            lpage = ref page.AsContentPage;
                            for (int i = 0; i < lpage.EntryCount; i++)
                            {
                                var entry = lpage.GetEntryAt(i);

                                for (KvPagenumber p = entry.FirstPage; p <= entry.LastPage; p++)
                                {
                                    map.Add(metaindex, p, entry.Tid <= mintid ? PageTypesI.FreeSpace : PageTypesI.FreeSpaceInUse, 0,
                                            GetRangeKind(p, entry.FirstPage, entry.PageCount));
                                }

                                if (true) // TODO make optional
                                {
                                    var fsentry = new FsEntry()
                                    {
                                        Pagenumber = lpage.Page.PageNumber,
                                        Index = (ushort)i,
                                        Tid = entry.Tid,
                                        FirstPage = entry.FirstPage,
                                        LastPage = entry.LastPage,
                                    };

                                    map.Add(metaindex, fsentry);
                                }
                            }
                            break;

                        default:
                            //throw new NotSupportedException("Unexpected Pagetype.");
                            break;
                    }

                    page.Dispose();

                    canceltoken.ThrowIfCancellationRequested();
                    progress?.Report(map.UniquePageCount);
                }

                if (pagestoscan.Count > 0)
                {
                    pagestoscan = pagestoscan.OrderBy(x => x).ToList();
                    pagestoscan.ForEach(x => scanqueue.AddPage(x));
                    pagestoscan.Clear();
                    level++;
                }
                else
                {
                    break;
                }
            }
        }

        private RangeKind GetRangeKind(ulong pageno, ulong firstpage, ulong pagecount)
        {
            var lastpage = firstpage + pagecount - 1;

            if (firstpage == lastpage)
            {
                return RangeKind.FullBlock;
            }
            else if (pageno == firstpage)
            {
                return RangeKind.Start;
            }
            else if (pageno == lastpage)
            {
                return RangeKind.End;
            }

            return RangeKind.Center;
        }

        private int GetFreeSpace(AnyPage page)
        {
            switch (page.PageType)
            {
                case PageTypes.DataIndex:
                case PageTypes.DataLeaf:
                case PageTypes.FsIndex:
                case PageTypes.FsLeaf:
                    return page.AsContentPage.Header.FreeSpace;
            }

            return 0;
        }

        private PageTypesI GetPageTypeI(AnyPage page)
        {
            switch (page.PageType)
            {
                case PageTypes.Raw:
                    return PageTypesI.Unknown;
                case PageTypes.FileHeader:
                    return PageTypesI.FileHeader;
                case PageTypes.Meta:
                    return PageTypesI.Meta;
                case PageTypes.FsIndex:
                    return PageTypesI.FsIndex;
                case PageTypes.FsLeaf:
                    return PageTypesI.FsLeaf;
                case PageTypes.DataIndex:
                    return PageTypesI.DataIndex;
                case PageTypes.DataLeaf:
                    return PageTypesI.DataLeaf;
                case PageTypes.DataOverflow:
                    return PageTypesI.DataOverflow;
                default:
                    return PageTypesI.Unknown;
            }
        }

        public PageMap GetPageMap(KvPagenumber pageno, PageTypesI pagetype)
        {
            var ret = new PageMap();
            ret.PageNumber = pageno;
            ret.PageType = pagetype;

            using (var page = GetPage(pageno, pagetype))
            {
                ret.PageSize = page.PageSize;

                ret.Bytes = page.Bytes.Span.ToArray();

                switch (pagetype)
                {
                    case PageTypesI.FileHeader:
                        var hp = new FileHeaderPage(page);

                        ret.Map = new ByteRange("Page", 0, (int)hp.Page.PageSize, -1, null, null, null);
                        var header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(hp.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddDefaultHeader(ret, header, hp.Header);
                        BrAddFileHeader(ret, header, hp.Header);

                        break;

                    case PageTypesI.Meta:
                        var mp = new MetaPage(page);

                        ret.Map = new ByteRange("Page", 0, (int)mp.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        var content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(mp.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddDefaultHeader(ret, header, mp.Header);
                        BrAddMetaHeader(ret, header, mp.Header);
                        BrAddMetaContent(ret, content, mp);

                        break;

                    case PageTypesI.FsIndex:
                        ref var fsindex = ref page.AsContentPage;
                        ret.Map = new ByteRange("Page", 0, (int)fsindex.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(fsindex.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddContentHeader(ret, header, fsindex.Header);
                        BrAddFsIndexContent(ret, content, ref fsindex);

                        break;

                    case PageTypesI.FsLeaf:

                        ref var fsleaf = ref page.AsContentPage;

                        ret.Map = new ByteRange("Page", 0, (int)fsleaf.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(fsleaf.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddContentHeader(ret, header, fsleaf.Header);
                        BrAddFsLeafContent(ret, content, ref fsleaf);

                        break;

                    case PageTypesI.FreeSpace:
                        ret.Map = new ByteRange("Page", 0, (int)page.PageSize, -1, null, null, null);
                        break;

                    case PageTypesI.FreeSpaceInUse:
                        ret.Map = new ByteRange("Page", 0, (int)page.PageSize, -1, null, null, null);
                        break;

                    case PageTypesI.Unknown:
                        ret.Map = new ByteRange("Page", 0, (int)page.PageSize, -1, null, null, null);
                        break;

                    case PageTypesI.DataIndex:
                        ref var dindex = ref page.AsContentPage;

                        ret.Map = new ByteRange("Page", 0, (int)dindex.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(dindex.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddContentHeader(ret, header, dindex.Header);
                        BrAddDataIndexContent(ret, content, ref dindex);

                        break;

                    case PageTypesI.DataLeaf:
                        ref var dleaf = ref page.AsContentPage;

                        ret.Map = new ByteRange("Page", 0, (int)dleaf.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(dleaf.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddContentHeader(ret, header, dleaf.Header);
                        BrAddDataLeafContent(ret, content, ref dleaf);

                        break;

                    case PageTypesI.DataOverflow:
                        ref var doverflow = ref page.AsOverflowPage;
                        ret.Map = new ByteRange("Page", 0, (int)doverflow.Page.PageSize, -1, null, null, null);
                        header = ret.Map.AddChild("Header", 0, UniversalHeader.HeaderSize, null, null, null);
                        content = ret.Map.AddChild("Content", UniversalHeader.HeaderSize, (int)(doverflow.Page.PageSize - UniversalHeader.HeaderSize), null, null, null);

                        BrAddOverflowHeader(ret, header, doverflow.Header);

                        break;

                    case PageTypesI.DataOverflowCont:
                        ret.Map = new ByteRange("Page", 0, (int)page.PageSize, -1, null, null, null);
                        content = ret.Map.AddChild("Content", 0, (int)(page.PageSize), null, null, null);
                        break;

                    default:
                        break;
                }
            }

            return ret;
        }

        private unsafe void BrAddFsIndexContent(PageMap map, ByteRange range, ref ContentPage page)
        {
            //var used = range.AddChild("UsedSpace", 0, page.Header.UsedSpace, null, null, null);
            var free = range.AddChild("FreeSpace", page.Header.Low, page.Header.FreeSpace, null, null, null);

            map.ContentSize = page.Header.ContentSize;
            map.FreeSpace = page.Header.FreeSpace;
            map.UsedSpace = page.Header.UsedSpace;

            for (int i = 0; i < page.EntryCount; i++)
            {
                var entry = page.GetEntryAt(i);

                if (i == 0)
                {
                    var leftbranch = page.GetLeftBranch(i);
                    var leftchild = range.AddChild("Branch", (int)(entry.Entry.Pointer - page.Content.Pointer - page.BranchSize),
                        page.BranchSize, i, typeof(ulong), leftbranch, leftbranch.ToString("N0"));
                }

                var child = range.AddChild("Entry", (int)(entry.Entry.Pointer - page.Content.Pointer), entry.EntrySize, i, null, null, null);

                child.AddChild(nameof(entry.Flags), 0x00, 2, typeof(ushort), entry.Flags, entry.Flags.ToString("X4"));
                child.AddChild(nameof(entry.KeyLength), 0x02, 2, typeof(ushort), entry.KeyLength, entry.KeyLength.ToString("N0"));
                child.AddChild(nameof(entry.FirstPage), 0x04, 8, typeof(ulong), entry.FirstPage, entry.FirstPage.ToString("N0"));

                var rightbranch = page.GetRightBranch(i);
                var rightchild = range.AddChild("Branch", (int)(entry.Entry.Pointer + entry.EntrySize - page.Content.Pointer),
                    page.BranchSize, i + 1, typeof(ulong), rightbranch, rightbranch.ToString("N0"));

                var ei = new EntryInfo();
                ei.Index = i;
                ei.Offset = page.GetEntryOffset(i);
                ei.Flags = entry.Flags;
                ei.KeyLength = entry.KeyLength;
                ei.Key = entry.KeyBytes.ToArray();
                ei.FirstPage = entry.FirstPage;
                ei.LeftBranch = page.GetLeftBranch(i);
                ei.RightBranch = page.GetRightBranch(i);
                map.Entries.Add(ei);
            }
        }

        private unsafe void BrAddFsLeafContent(PageMap map, ByteRange range, ref ContentPage page)
        {
            //var used = range.AddChild("UsedSpace", 0, page.Header.UsedSpace, null, null, null);
            var free = range.AddChild("FreeSpace", page.Header.Low, page.Header.FreeSpace, null, null, null);

            map.ContentSize = page.Header.ContentSize;
            map.FreeSpace = page.Header.FreeSpace;
            map.UsedSpace = page.Header.UsedSpace;

            for (int i = 0; i < page.EntryCount; i++)
            {
                var entry = page.GetEntryAt(i);

                var child = range.AddChild("Entry", (int)(entry.Entry.Pointer - page.Content.Pointer), entry.EntrySize, i, null, null, null);

                child.AddChild(nameof(entry.Flags), 0x00, 2, typeof(ushort), entry.Flags, entry.Flags.ToString("X4"));
                child.AddChild(nameof(entry.KeyLength), 0x02, 2, typeof(ushort), entry.KeyLength, entry.KeyLength.ToString("N0"));
                child.AddChild(nameof(entry.FirstPage), 0x04, 8, typeof(ulong), entry.FirstPage, entry.FirstPage.ToString("N0"));
                child.AddChild(nameof(entry.LastPage), 0x0c, 8, typeof(ulong), entry.LastPage, entry.LastPage.ToString("N0"));
                child.AddChild(nameof(entry.Tid), 0x14, 8, typeof(ulong), entry.Tid, entry.Tid.ToString("N0"));

                var ei = new EntryInfo();
                ei.Index = i;
                ei.Offset = page.GetEntryOffset(i);
                ei.Flags = entry.Flags;
                ei.KeyLength = entry.KeyLength;
                ei.Key = entry.KeyBytes.ToArray();
                ei.Tid = entry.Tid;
                ei.FirstPage = entry.FirstPage;
                ei.LastPage = entry.LastPage;
                map.Entries.Add(ei);
            }
        }

        private unsafe void BrAddDataIndexContent(PageMap map, ByteRange range, ref ContentPage page)
        {
            //var used = range.AddChild("UsedSpace", 0, page.Header.UsedSpace, null, null, null);
            var free = range.AddChild("FreeSpace", page.Header.Low, page.Header.FreeSpace, null, null, null);

            map.ContentSize = page.Header.ContentSize;
            map.FreeSpace = page.Header.FreeSpace;
            map.UsedSpace = page.Header.UsedSpace;

            var offsets = range.AddChild("EntryOffsets", (int)((byte*)(page.EntryOffsetArray - page.EntryCount + 1) - page.Content.Pointer), page.Header.KeyCount * page.OffsetEntrySize, null, null, null);

            for (int i = 0; i < page.EntryCount; i++)
            {
                var entry = page.GetEntryAt(i);

                if (i == 0)
                {
                    var leftbranch = page.GetLeftBranch(i);
                    var leftchild = range.AddChild("Branch", (int)(entry.Entry.Pointer - page.Content.Pointer - page.BranchSize),
                        page.BranchSize, i, typeof(ulong), leftbranch, leftbranch.ToString("N0"));
                }

                var child = range.AddChild("Entry", (int)(entry.Entry.Pointer - page.Content.Pointer), entry.EntrySize, i, null, null, null);

                child.AddChild(nameof(entry.Flags), 0x00, 2, typeof(ushort), entry.Flags, entry.Flags.ToString("X4"));
                child.AddChild(nameof(entry.KeyLength), 0x02, 2, typeof(ushort), entry.KeyLength, entry.KeyLength.ToString("N0"));
                child.AddChild(nameof(entry.KeyBytes), 0x04, entry.KeyLength, typeof(byte[]), entry.KeyBytes.ToArray(), Util.GetHexString(entry.KeyBytes, 64));

                offsets.AddChild("EntryOffset", (int)((byte*)(page.EntryOffsetArray - i) - page.Content.Pointer - offsets.Offset), page.OffsetEntrySize, i, typeof(ushort), page.GetEntryOffset(i), page.GetEntryOffset(i).ToString("N0"));

                var rightbranch = page.GetRightBranch(i);
                var rightchild = range.AddChild("Branch", (int)(entry.Entry.Pointer + entry.EntrySize - page.Content.Pointer),
                    page.BranchSize, i + 1, typeof(ulong), rightbranch, rightbranch.ToString("N0"));

                var ei = new EntryInfo();
                ei.Index = i;
                ei.Offset = page.GetEntryOffset(i);
                ei.Flags = entry.Flags;
                ei.KeyLength = entry.KeyLength;
                ei.Key = entry.KeyBytes.ToArray();
                ei.LeftBranch = page.GetLeftBranch(i);
                ei.RightBranch = page.GetRightBranch(i);
                map.Entries.Add(ei);
            }
        }

        private unsafe void BrAddDataLeafContent(PageMap map, ByteRange range, ref ContentPage page)
        {
            //var used = range.AddChild("UsedSpace", 0, page.Header.UsedSpace, null, null, null);
            var free = range.AddChild("FreeSpace", page.Header.Low, page.Header.FreeSpace, null, null, null);

            map.ContentSize = page.Header.ContentSize;
            map.FreeSpace = page.Header.FreeSpace;
            map.UsedSpace = page.Header.UsedSpace;

            var offsets = range.AddChild("EntryOffsets", (int)((byte*)(page.EntryOffsetArray - page.EntryCount + 1) - page.Content.Pointer), page.Header.KeyCount * page.OffsetEntrySize, -1, null, null, null);

            for (int i = 0; i < page.EntryCount; i++)
            {
                var entry = page.GetEntryAt(i);

                var child = range.AddChild("Entry", (int)(entry.Entry.Pointer - page.Content.Pointer), entry.EntrySize, i, null, null, null);

                child.AddChild(nameof(entry.Flags), 0x00, 2, typeof(ushort), entry.Flags, entry.Flags.ToString("X4"));
                child.AddChild(nameof(entry.KeyLength), 0x02, 2, typeof(ushort), entry.KeyLength, entry.KeyLength.ToString("N0"));

                var offset = 4;

                if (entry.KeyLength > 0)
                {
                    child.AddChild(nameof(entry.KeyBytes), offset, entry.KeyLength, typeof(byte[]), entry.KeyBytes.ToArray(), Util.GetHexString(entry.KeyBytes, 64));
                    offset += entry.KeyLength;
                }

                if ((entry.Flags & EntryFlags.HasSubtree) != 0)
                {
                    child.AddChild(nameof(entry.SubTree), offset, 8, typeof(ulong), entry.SubTree, entry.SubTree.HasValue ? entry.SubTree.Value.ToString("N0") : entry.SubTree.ToString());
                    offset += 8;
                    child.AddChild(nameof(entry.TotalCount), offset, 8, typeof(ulong), entry.TotalCount, entry.TotalCount.ToString("N0"));
                    offset += 8;
                    child.AddChild(nameof(entry.LocalCount), offset, 8, typeof(ulong), entry.LocalCount, entry.LocalCount.ToString("N0"));
                    offset += 8;
                }

                if ((entry.Flags & EntryFlags.HasValue) != 0)
                {
                    if ((entry.Flags & EntryFlags.IsOverflow) != 0)
                    {
                        child.AddChild(nameof(entry.OverflowPageNumber), offset, 8, typeof(ulong), entry.OverflowPageNumber, entry.OverflowPageNumber.ToString("N0"));
                        offset += 8;
                        child.AddChild(nameof(entry.OverflowLength), offset, 8, typeof(ulong), entry.OverflowLength, entry.OverflowLength.ToString("N0"));
                    }
                    else
                    {
                        child.AddChild(nameof(entry.InlineValueLength), offset, 2, typeof(ushort), entry.InlineValueLength, entry.InlineValueLength.ToString("N0"));
                        offset += 2;
                        child.AddChild("Value", offset, entry.InlineValueLength, typeof(byte[]), entry.InlineValueBytes.ToArray(), Util.GetHexString(entry.InlineValueBytes, 64));
                    }
                }

                offsets.AddChild("EntryOffset", (int)((byte*)(page.EntryOffsetArray - i) - page.Content.Pointer - offsets.Offset), page.OffsetEntrySize, i, typeof(ushort), page.GetEntryOffset(i), page.GetEntryOffset(i).ToString("N0"));

                var ei = new EntryInfo();
                ei.Index = i;
                ei.Offset = page.GetEntryOffset(i);
                ei.Flags = entry.Flags;
                ei.KeyLength = entry.KeyLength;
                ei.Key = entry.KeyBytes.ToArray();

                if ((entry.Flags & EntryFlags.HasSubtree) != 0)
                {
                    ei.SubTree = entry.SubTree;
                    ei.TotalCount = entry.TotalCount;
                    ei.LocalCount = entry.LocalCount;
                }

                if ((entry.Flags & EntryFlags.HasValue) != 0)
                {
                    if ((entry.Flags & EntryFlags.IsOverflow) != 0)
                    {
                        ei.OverflowPage = entry.OverflowPageNumber;
                        ei.OverflowLength = entry.OverflowLength;
                        ei.OverflowValue = GetOverflowBytes(entry);
                    }
                    else
                    {
                        ei.InlineValueLength = entry.InlineValueLength;
                        ei.InlineValue = entry.InlineValueBytes.ToArray();
                    }
                }

                map.Entries.Add(ei);
            }
        }


        private void BrAddOverflowHeader(PageMap map, ByteRange range, UniversalHeader header)
        {
            BrAddDefaultHeader(map, range, header);

            range.AddChild(nameof(header.Tid), 0x10, 8, typeof(ulong), header.Tid, header.Tid.ToString("N0"));
            range.AddChild(nameof(header.ContentLength), 0x18, 8, typeof(ulong), header.ContentLength, header.ContentLength.ToString("N0"));

            map.Tid = header.Tid;
            map.ContentLength = header.ContentLength;
        }

        private void BrAddContentHeader(PageMap map, ByteRange range, UniversalHeader header)
        {
            BrAddDefaultHeader(map, range, header);

            range.AddChild(nameof(header.Tid), 0x10, 8, typeof(ulong), header.Tid, header.Tid.ToString("N0"));
            range.AddChild(nameof(header.Low), 0x18, 2, typeof(ushort), header.Low, header.Low.ToString("N0"));
            range.AddChild(nameof(header.High), 0x1a, 2, typeof(ushort), header.High, header.High.ToString("N0"));
            range.AddChild(nameof(header.KeyCount), 0x1c, 2, typeof(ushort), header.KeyCount, header.KeyCount.ToString("N0"));
            range.AddChild("Unused", 0x1e, 2, typeof(ushort), header.Unused2, header.Unused2.ToString("N0"));

            map.Tid = header.Tid;
            map.Low = header.Low;
            map.High = header.High;
            map.KeyCount = header.KeyCount;
        }

        private void BrAddMetaHeader(PageMap map, ByteRange range, UniversalHeader header)
        {
            range.AddChild(nameof(header.Tid), 0x10, sizeof(KvTid), typeof(KvTid), header.Tid, header.Tid.ToString("N0"));
        }

        private void BrAddMetaContent(PageMap map, ByteRange range, MetaPage mp)
        {
            range.AddChild(nameof(mp.Tid), 0x00, 8, typeof(ulong), mp.Tid, mp.Tid.ToString("N0"));
            range.AddChild(nameof(mp.FsRootPage), 0x08, 8, typeof(ulong), mp.FsRootPage, mp.FsRootPage.ToString("N0"));
            range.AddChild(nameof(mp.DataRootPage), 0x10, 8, typeof(ulong), mp.DataRootPage, mp.DataRootPage.ToString("N0"));
            range.AddChild(nameof(mp.LastPage), 0x18, 8, typeof(ulong), mp.LastPage, mp.LastPage.ToString("N0"));
            range.AddChild(nameof(mp.DataTotalCount), 0x20, 8, typeof(ulong), mp.DataTotalCount, mp.DataTotalCount.ToString("N0"));
            range.AddChild(nameof(mp.DataLocalCount), 0x28, 8, typeof(ulong), mp.DataLocalCount, mp.DataLocalCount.ToString("N0"));
            range.AddChild(nameof(mp.FsTotalCount), 0x30, 8, typeof(ulong), mp.FsTotalCount, mp.FsTotalCount.ToString("N0"));
            range.AddChild(nameof(mp.FsLocalCount), 0x38, 8, typeof(ulong), mp.FsLocalCount, mp.FsLocalCount.ToString("N0"));
            range.AddChild(nameof(mp.FooterTid), mp.Content.Length - sizeof(KvTid), 8, typeof(ulong), mp.FooterTid, mp.FooterTid.ToString("N0"));
        }

        private void BrAddFileHeader(PageMap map, ByteRange range, UniversalHeader header)
        {
            range.AddChild(nameof(header.Version), 0x10, 2, typeof(ushort), header.Version, header.Version.ToString());
            range.AddChild(nameof(header.PageSizeExponent), 0x12, 2, typeof(ushort), header.PageSizeExponent, header.PageSizeExponent.ToString());
            range.AddChild(nameof(header.Flags), 0x14, 2, typeof(ushort), header.Flags, header.Flags.ToString());
            range.AddChild("Unused", 0x16, 2, typeof(ushort), header.Unused3, header.Unused3.ToString("N0"));
            range.AddChild(nameof(header.InternalTypeCode), 0x18, 4, typeof(uint), header.InternalTypeCode, header.InternalTypeCode.ToString("N0"));
            range.AddChild(nameof(header.UserTypeCode), 0x1c, 4, typeof(uint), header.UserTypeCode, header.UserTypeCode.ToString("N0"));
        }

        private void BrAddDefaultHeader(PageMap map, ByteRange range, UniversalHeader header)
        {
            range.AddChild(nameof(header.Magic), 0x00, 4, typeof(uint), header.Magic, header.Magic.ToString("x8"));
            range.AddChild(nameof(header.PageType), 0x04, 2, typeof(ushort), header.PageType, header.PageType.ToString());
            range.AddChild("Unused", 0x06, 2, typeof(ushort), header.Unused1, header.Unused1.ToString());
            range.AddChild(nameof(header.PageNumber), 0x08, 8, typeof(ulong), header.PageNumber, header.PageNumber.ToString("N0"));

            map.Magic = header.Magic;
            map.PageTypeReal = header.PageType;
        }

        private byte[] GetOverflowBytes(EntryInline entry)
        {
            var len = Math.Min(entry.OverflowLength, 65536);
            var ret = new byte[len];
            var pageno = entry.OverflowPageNumber;

            var pos = 0;

            ref var page = ref GetPage(pageno, PageTypesI.DataOverflow).AsOverflowPage;
            var pagelen = (int)Math.Min(len, (ulong)page.Content.Length);
            var target = ret.AsSpan<byte>().Slice(pos, pagelen);
            page.Content.Span.Slice(0, pagelen).CopyTo(target);

            pos += pagelen;
            len -= (uint)pagelen;

            while (len > 0)
            {
                pageno++;
                var opage = GetPage(pageno, PageTypesI.DataOverflowCont);
                pagelen = (int)Math.Min(len, (ulong)opage.Bytes.Length);
                target = ret.AsSpan<byte>().Slice(pos, pagelen);
                opage.Bytes.Span.Slice(0, pagelen).CopyTo(target);

                pos += pagelen;
                len -= (uint)pagelen;
            }

            return ret;
        }

        private AnyPage GetPage(KvPagenumber pageno, PageTypesI pagetype)
        {
            var createheader = true;

            switch (pagetype)
            {
                case PageTypesI.DataOverflowCont:
                case PageTypesI.FreeSpace:
                case PageTypesI.FreeSpaceInUse:
                case PageTypesI.Unknown:
                    createheader = false;
                    break;
            }

            return _database.Pager.ReadPageInspector(pageno, Properties.PageSize, createheader);
        }

        public void Dispose()
        {
            _database?.Dispose();
            _database = null;
            _props = null;
        }
    }
}
