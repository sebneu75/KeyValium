using KeyValium.Collections;
using System.Linq;

namespace KeyValium.Inspector
{
    public class FileMap
    {
        internal FileMap()
        {
        }

        #region Freespace list

        // TODO put in separate file 
        private SortedDictionary<short, List<FsEntry>> _fslist = new();

        internal bool Add(short metaindex, FsEntry entry)
        {
            if (!_fslist.ContainsKey(metaindex))
            {
                _fslist.Add(metaindex, new List<FsEntry>());
            }

            _fslist[metaindex].Add(entry);

            return true;
        }

        public IReadOnlyList<FsEntry> GetFreespaceList(short metaindex)
        {
            if (_fslist.ContainsKey(metaindex))
            {
                return _fslist[metaindex];
            }

            return new List<FsEntry>();
        }

        #endregion

        public ulong TotalPageCount
        {
            get;
            internal set;
        }

        public ulong UniquePageCount
        {
            get;
            private set;
        }

        // TODO one dictionary per PageType
        private SortedDictionary<short, SortedDictionary<KvPagenumber, PageInfo>> _map = new();

        private SortedDictionary<short, Dictionary<PageTypesI, ulong>> _pagecounts = new();

        //private SortedDictionary<short, Dictionary<PageTypesI, ulong>> _unusedspace = new();

        internal PageRangeList GetPageList(short metaindex)
        {
            var ret = new PageRangeList();

            if (_map.ContainsKey(metaindex))
            {
                foreach (var pair in _map[metaindex])
                {
                    ret.AddPage(pair.Key);
                }
            }

            return ret;
        }

        internal bool Add(short metaindex, KvPagenumber pageno, PageTypesI pagetype, int unusedspace)
        {
            if (!ExistsPageNumber(pageno))
            {
                UniquePageCount++;
            }

            if (!_map.ContainsKey(metaindex))
            {
                _map.Add(metaindex, new SortedDictionary<KvPagenumber, PageInfo>());
            }

            if (_map[metaindex].ContainsKey(pageno))
            {
                // page already exists
                return false;
            }

            _map[metaindex].Add(pageno, new PageInfo() { PageNumber = pageno, PageType = pagetype, UnusedSpace = unusedspace });

            if (!_pagecounts.ContainsKey(metaindex))
            {
                _pagecounts.Add(metaindex, new Dictionary<PageTypesI, ulong>());
            }

            if (!_pagecounts[metaindex].ContainsKey(pagetype))
            {
                _pagecounts[metaindex].Add(pagetype, 0);
            }

            _pagecounts[metaindex][pagetype]++;

            // unused space
            //if (!_unusedspace.ContainsKey(metaindex))
            //{
            //    _unusedspace.Add(metaindex, new Dictionary<PageTypesI, ulong>());
            //}

            //if (!_unusedspace[metaindex].ContainsKey(pagetype))
            //{
            //    _unusedspace[metaindex].Add(pagetype, 0);
            //}

            //_unusedspace[metaindex][pagetype] += (ulong)unusedspace;

            return true;
        }

        private bool ExistsPageNumber(KvPagenumber pageno)
        {
            foreach (var metaindex in _map.Keys)
            {
                if (_map[metaindex].ContainsKey(pageno))
                {
                    return true;
                }
            }

            return false;
        }

        public ulong GetPageCount(short metaindex)
        {
            if (_map.ContainsKey(metaindex))
            {
                return (ulong)_map[metaindex].Count;
            }

            return 0;
        }

        public ulong GetPageCount(short metaindex, PageTypesI pagetype)
        {
            if (_pagecounts.ContainsKey(metaindex))
            {
                if (_pagecounts[metaindex].ContainsKey(pagetype))
                {
                    return _pagecounts[metaindex][pagetype];
                }
            }

            return 0;
        }

        public PageTypesI GetPageType(short metaindex, KvPagenumber pageno)
        {
            SortedDictionary<KvPagenumber, PageInfo> maindict = null;
            SortedDictionary<KvPagenumber, PageInfo> metadict = null;

            if (_map.ContainsKey(-1))
            {
                maindict = _map[-1];
            }

            if (_map.ContainsKey(metaindex))
            {
                metadict = _map[metaindex];
            }

            if (metadict != null && metadict.ContainsKey(pageno))
            {
                return metadict[pageno].PageType;
            }

            if (maindict != null && maindict.ContainsKey(pageno))
            {
                return maindict[pageno].PageType;
            }

            return PageTypesI.Unknown;
        }

        internal ulong GetUnusedSpace(short metaindex)
        {
            if (_map.ContainsKey(metaindex))
            {
                return (ulong)_map[metaindex].Values.Sum(x => x.UnusedSpace);
            }

            return 0;
        }

        public ulong GetUnusedSpace(short metaindex, PageTypesI pagetype)
        {
            if (_map.ContainsKey(metaindex))
            {
                return (ulong)_map[metaindex].Values.Where(x => x.PageType == pagetype).Sum(x => x.UnusedSpace);
            }

            return 0;
        }

        internal ulong GetUnreferencedCount(short metaindex, DatabaseProperties props)
        {
            if (_map.ContainsKey(metaindex))
            {
                var ranges = new PageRangeList();
                ranges.AddRange(Limits.MetaPages + 1, props.MetaInfos[metaindex].LastPage);
                _map[metaindex].Values.ToList().ForEach(x => ranges.RemovePage(x.PageNumber));

                return ranges.PageCount;
            }

            return 0;
        }
    }
}
