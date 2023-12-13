using KeyValium.Cursors;
using System.Data;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace KeyValium
{
    internal class Timer
    {
        private static Stopwatch _sw = new();

        private static Dictionary<string, long> _counts = new();

        private static Dictionary<string, long> _ticks = new();

        static Timer()
        {
            //for (int k = 0; k < 10; k++)
            //{
            //    Clear();

            //    for (int i = 0; i < 10000; i++)
            //    {
            //        Start("Init");
            //        Stop("Init");
            //    }               
            //}

            //var count = _counts["Init"];
            //var ticks = _ticks["Init"];

            //var totalseconds = (double)ticks / (double)Stopwatch.Frequency;

            //_overhead = totalseconds* / count;
            //_overhead = _overhead  ;

            //Console.WriteLine("Overhead: {0:#.000}ns", _overhead);

            //Clear();
        }

        private static double _overhead = 0;

        public static void Clear()
        {
            _counts.Clear();
            _ticks.Clear();
            _dict.Clear();
        }

        public static void Dump()
        {
            foreach (var key in _counts.Keys)
            {
                var count = _counts[key];
                var ticks = _ticks[key];

                var totalseconds = (double)ticks / (double)Stopwatch.Frequency - _overhead / 1000000000 * count;

                var nsperitem = totalseconds / count * 1000000000;

                Console.WriteLine("[{0}]: Count: {1}    Total: {2:#.000}ms   Per Item: {3:#.000}ns", key, count, totalseconds * 1000, nsperitem);
            }
        }

        private static Dictionary<string, Stopwatch> _dict = new();

        private string _name;

        private static Stopwatch EnsureStopwatch(string name)
        {
            if (_dict.TryGetValue(name, out var stopwatch))
            {
                return stopwatch;
            }

            var sw = new Stopwatch();

            _dict[name] = sw;

            return sw;
        }

        public static void Start(string name = "<Unnamed>")
        {
            _sw.Restart();
        }

        public static void Stop(string name = "<Unnamed>")
        {
            _sw.Stop();

            Add(name, 1, _sw.ElapsedTicks);
        }

        private static void Add(string name, long count, long ticks)
        {
            if (_counts.ContainsKey(name))
            {
                _counts[name] += count;
            }
            else
            {
                _counts[name] = count;
            }

            if (_ticks.ContainsKey(name))
            {
                _ticks[name] += ticks;
            }
            else
            {
                _ticks[name] = ticks;
            }
        }
    }

    internal class KvDebug
    {
        #region Assertions

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, msg);
            }
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void BreakIf(bool condition)
        {
            if (Debugger.IsAttached && condition)
                Debugger.Break();
        }

        #endregion

        #region Cursor

        internal static void DumpNode(Cursor cursor, StreamWriter writer, KvPagenumber pageno, int level)
        {
            if (pageno == 0)
            {
                return;
            }

            using (var page = cursor.CurrentTransaction.GetPage(pageno, true, out _))
            {

                var sb = new StringBuilder();

                sb.AppendFormat("{0}{1:0000}[{2}]: ", "".PadLeft(level * 4), page.PageNumber, page.PageType);

                switch (page.PageType)
                {
                    case PageTypes.DataIndex:
                    case PageTypes.FsIndex:
                        ref var ipage = ref page.AsContentPage;

                        for (int i = 0; i < ipage.EntryCount; i++)
                        {
                            sb.AppendFormat("[{0:0000}]-{1}-", ipage.GetLeftBranch(i), Util.GetHexString(ipage.GetEntryAt(i).KeyBytes.ToArray()));
                        }

                        if (ipage.EntryCount > 0)
                        {
                            var i = ipage.EntryCount - 1;
                            sb.AppendFormat("[{0:0000}]", ipage.GetRightBranch(i));
                        }
                        break;

                    case PageTypes.DataLeaf:
                    case PageTypes.FsLeaf:
                        ref var leaf = ref page.AsContentPage;

                        var list = new List<string>();
                        for (int i = 0; i < leaf.EntryCount; i++)
                        {
                            var entry = leaf.GetEntryAt(i);
                            list.Add(Util.GetHexString(entry.KeyBytes.ToArray()));
                            if (entry.SubTree.HasValue && entry.SubTree.Value != 0)
                            {
                                DumpNode(cursor, writer, entry.SubTree.Value, level + 1);
                            }
                        }

                        sb.Append(string.Join("-", list));

                        break;

                    default:
                        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
                }

                writer.WriteLine(sb.ToString());

                switch (page.PageType)
                {
                    case PageTypes.DataIndex:
                    case PageTypes.FsIndex:
                        ref var ipage = ref page.AsContentPage;

                        if (ipage.EntryCount > 0)
                        {
                            for (int i = 0; i <= ipage.EntryCount; i++)
                            {
                                DumpNode(cursor, writer, ipage.GetLeftBranch(i), level + 1);
                            }
                        }
                        break;

                    case PageTypes.DataLeaf:
                    case PageTypes.FsLeaf:
                        break;

                    default:
                        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + page.PageType.ToString());
                }
            }
        }

        public static (bool, string) CompareCursors(Cursor self, Cursor other)
        {
            var equal = true;

            var sb = new StringBuilder();

            var leftpos = self.CurrentPath.First;
            var rightpos = other.CurrentPath.First;

            // TODO fix ToList() function

            while (leftpos <= self.CurrentPath.Last || rightpos <= other.CurrentPath.Last)
            {
                var left = "";
                var right = "";

                if (leftpos <= self.CurrentPath.Last)
                {
                    left = self.CurrentPath.GetNode(leftpos).ToString();
                }
                else
                {
                    left = "<none>";
                }

                if (rightpos <= other.CurrentPath.Last)
                {
                    right = other.CurrentPath.GetNode(rightpos).ToString();
                }
                else
                {
                    right = "<none>";
                }

                if (left != right)
                {
                    equal = false;
                }

                sb.AppendFormat("{0} {1} {2}\n", left, left == right ? "==" : "!=", right);

                leftpos++;
                rightpos++;
            }

            return (equal, sb.ToString());
        }

        /// <summary>
        /// Validates the cursors nodepath. Throws if an error is found.
        /// </summary>
        /// <param name="key"></param>
        [Conditional("DEBUG")]
        public static void ValidateCursor2(Transaction tx, Cursor cursor, TreeRef keyref, ReadOnlySpan<byte> key, DeleteHandling? delete = null)
        {
            return;

            lock (tx.TxLock)
            {
                try
                {
                    cursor.SavePagenumber();
                    cursor.CurrentPath.MoveFirst();

                    using (var cursor2 = new Cursor(tx, keyref, InternalTrackingScope.None, cursor.IsFreeSpaceCursor, false))
                    {
                        if (delete.HasValue)
                        {
                            cursor2.DeleteHandling = delete.Value;
                        }

                        if (cursor2.SetPosition(key) || delete.HasValue)
                        {
                            if (delete.HasValue && delete.Value != DeleteHandling.Invalidate)
                            {
                                cursor2.ApplyDeleteHandling();
                            }

                            var (cmpresult, msg) = CompareCursors(cursor, cursor2);
                            if (!cmpresult)
                            {
                                Console.WriteLine(msg);
                                throw new KeyValiumException(ErrorCodes.InvalidCursor, "Cursor mismatch!");
                            }
                            else if (!delete.HasValue)
                            {
                                var key2 = cursor2.GetCurrentKey(); ;

                                var result = UniversalComparer.CompareBytes(key.ToArray(), key2);
                                if (result != 0)
                                {
                                    throw new KeyValiumException(ErrorCodes.KeyMismatch, "Key mismatch!");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Key not found.");
                        }
                    }
                }
                finally
                {
                    cursor.RestorePagenumber();
                }
            }
        }

        /// <summary>
        /// Validates the cursors nodepath. Throws if an error is found.
        /// </summary>
        /// <param name="key"></param>
        [Conditional("DEBUG")]
        public static void ValidateCursor(Cursor cursor, TreeRef keyref, byte[] key)
        {
            lock (cursor.CurrentTransaction.TxLock)
            {
                try
                {
                    // TODO fix

                    //cursor.SavePosition();

                    //cursor.CurrentKeyPath.MoveFirst();

                    //if (cursor.CurrentKeyPath.Current==null)
                    //{
                    //    return;
                    //}

                    //if (cursor.CurrentKeyPath.Current.Page.PageNumber != cursor.CurrentTransaction.Meta.DataRootPage)
                    //{
                    //    throw new KeyValiumException(ErrorCodes.InvalidCursor, "Root Pagenumber mismatch!");
                    //}

                    //while (true)
                    //{
                    //    if (cursor.CurrentKeyPath.Current.Page.PageType == cursor.PageTypeIndex)
                    //    {
                    //        var ipage = cursor.CurrentKeyPath.Current.Page.AsContentPage;
                    //        var pageno = ipage.GetLeftBranch(cursor.CurrentKeyPath.Current.KeyIndex);

                    //        if (cursor.CurrentKeyPath.Current.Next==null)
                    //        {
                    //            throw new KeyValiumException(ErrorCodes.InvalidCursor, "Invalid Nextnode!");
                    //        }

                    //        if (pageno != cursor.CurrentKeyPath.Current.Next.Page.PageNumber)
                    //        {
                    //            throw new KeyValiumException(ErrorCodes.PagenumberMismatch, "Pagenumber mismatch!");
                    //        }
                    //    }
                    //    else if (cursor.CurrentKeyPath.Current.Page.PageType == cursor.PageTypeLeaf)
                    //    {
                    //        var lpage = cursor.CurrentKeyPath.Current.Page.AsContentPage;
                    //        var lkey = lpage.GetKey(cursor.CurrentKeyPath.Current.KeyIndex);

                    //        var result = UniversalComparer.CompareBytes(key, lkey);
                    //        if (result != 0)
                    //        {
                    //            throw new KeyValiumException(ErrorCodes.KeyMismatch, "Key mismatch!");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected pagetype!");
                    //    }

                    //    if (!cursor.CurrentKeyPath.MoveNext())
                    //    {
                    //        return;
                    //    }
                    //}
                }
                finally
                {
                    //cursor.RestorePosition();
                }
            }
        }

        #endregion

        static int _dumptree = 0;

        [Conditional("DEBUG")]
        public static void DumpTree(Transaction tx, string path, bool freespace)
        {
            _dumptree++;

            using (var writer = new StreamWriter(path + "." + _dumptree.ToString("00000"), false, Encoding.UTF8))
            {
                writer.WriteLine(" DataRootPage: {0:0000}", tx.Meta.DataRootPage);
                writer.WriteLine("   FsRootPage: {0:0000}", tx.Meta.FsRootPage);
                writer.WriteLine("  Dirty Pages: {0} - {1}", tx.Pages.DirtyPages.Count, string.Join(", ", tx.Pages.DirtyPages.ToList()));
                writer.WriteLine("  Loose Pages: {0} - {1}", tx.Pages.LoosePages.PageCount, string.Join(", ", tx.Pages.LoosePages.ToList()));
                writer.WriteLine("Spilled Pages: {0} - {1}", tx.Pages.SpilledPages.Count, string.Join(", ", tx.Pages.SpilledPages.ToRangeList()));
                writer.WriteLine("   Free Pages: {0} - {1}", tx.Pages.FreePages.Count, string.Join(", ", tx.Pages.FreePages.ToRangeList()));

                lock (tx.TxLock)
                {
                    if (freespace)
                    {
                        using (var cursor = tx.GetFsCursor1())
                        {
                            DumpNode(cursor, writer, tx.Meta.FsRootPage, 0);
                        }
                    }
                    else
                    {
                        using (var cursor = tx.GetCursor(null, InternalTrackingScope.None))
                        {
                            DumpNode(cursor, writer, tx.Meta.DataRootPage, 0);
                        }
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        public static void DumpPage(ref ContentPage cp, string title)
        {
            Console.WriteLine("PageNumber: {0}", cp.Page.PageNumber);

            var page = cp.Page;

            Console.WriteLine("********************************************************");
            Console.WriteLine("{0} Pagenumber: {1}    PageType: {2}    PageSize: {3}", title, page.PageNumber, page.PageType, page.PageSize);
            Console.WriteLine("Entries: {0}", cp.EntryCount);

            for (int i = 0; i < cp.EntryCount; i++)
            {
                Console.Write("Entry[{0}]: Key={1} Size={2}+{3}+{4} Offset={5}", i,
                    GetHexString(cp.GetEntryAt(i).KeyBytes.ToArray()),
                    cp.GetEntrySize(i), cp.BranchSize, cp.OffsetEntrySize,
                    cp.GetEntryOffset(i));

                if (cp.IsIndexPage)
                {
                    Console.Write(" Left={0} Right={1}", cp.GetLeftBranch(i), cp.GetRightBranch(i));
                }

                Console.WriteLine();
            }


            Console.WriteLine("Used Space: {0}", cp.Header.UsedSpace);
            Console.WriteLine("Free Space: {0}", cp.Header.FreeSpace);

            Console.WriteLine("********************************************************");
        }

        [Conditional("DEBUG")]
        public static void DumpCursor(Cursor cursor, string title)
        {
            var sb = new StringBuilder();

            sb.Append(title);

            sb.Append(GetCursorInfo(cursor));

            // TODO fix

            //for (var index = cursor.CurrentKeyPath.First; index <= cursor.CurrentKeyPath.Last; index++)
            //{
            //    ref var node = ref cursor.CurrentKeyPath.Items[index];

            //    if (index == cursor.CurrentKeyPath.Current)
            //        sb.Append("*");
            //    sb.AppendFormat("[{0}] {1}.{2}", PageTypes.GetName(node.Page.PageType), node.Page.PageNumber, node.KeyIndex);
            //    if (index < cursor.CurrentKeyPath.Last)
            //        sb.Append(" --> ");
            //}

            sb.Append("}");

            Console.WriteLine(sb);
        }

        public static string GetPageInfo(AnyPage page)
        {
            var sb = new StringBuilder();

            // Pagenumber | Pagetype | Count | Low | High | Freespace
            // Index | Offset | Left | Flags | KeyLength | Key | Subtree | ValueLength | Right

            sb.AppendLine(new String('-', 132));

            sb.AppendFormat("|Page {0} Type: {1} ({2}) Size: {3}",
                page.PageNumber, page.PageType, PageTypes.GetName(page.PageType), page.PageSize);

            ref var cp = ref page.AsContentPage;
            if (cp.Content.Length != 0)
            {
                sb.AppendFormat(" Count: {0} Low: {1} High: {2} Used: {3} Free: {4}",
                    cp.Header.KeyCount, cp.Header.Low, cp.Header.High, cp.Header.UsedSpace, cp.Header.FreeSpace);
                sb.AppendLine();

                sb.AppendLine(new String('-', 132));

                var fmt = "|{0,5}|{1,6}|{2,9}|{3,9}|{4,5:x4}|{5,6}|{6,-44}|{7,9}|{8,9}|{9,9}|{10,9}|\n";
                sb.AppendFormat(fmt, "Index", "Offset", "Left", "Right", "Flags", "KeyLen", "Key", "Subtree", "ValueLen", "OvLen", "OvPage");

                sb.AppendLine(new String('-', 132));

                for (int i = 0; i < cp.Header.KeyCount; i++)
                {
                    var entry = cp.GetEntryAt(i);

                    var ovpage = "";
                    if (!cp.IsIndexPage)
                    {
                        ovpage = entry.OverflowPageNumber == 0 ? entry.InlineValueLength.ToString() : string.Format("{0} => {1}", entry.OverflowLength, entry.OverflowPageNumber);
                    }

                    sb.AppendFormat(fmt,
                        i,                                                  // Index
                        cp.GetEntryOffset(i),                               // Offset
                        cp.IsIndexPage ? cp.GetLeftBranch(i) : "",          // Left
                        cp.IsIndexPage ? cp.GetRightBranch(i) : "",         // Right
                        entry.Flags,                                        // Flags
                        entry.KeyLength,                                    // KeyLength
                        Util.GetHexString(cp.GetKey(i)),                    // Key
                        cp.IsIndexPage ? "" : entry.SubTree,                // SubTree
                        cp.IsIndexPage ? "" : entry.InlineValueLength,      // InlineValueLength
                        cp.IsIndexPage ? "" : entry.OverflowLength,         // OverflowLength
                        cp.IsIndexPage ? "" : entry.OverflowPageNumber      // OverflowPage
                        );
                }
            }

            sb.AppendLine(new String('-', 132));

            return sb.ToString();
        }

        public static string GetCursorInfo(Cursor cursor)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("C Oid={0} Tag='{1}'", cursor.Oid, cursor.Tag);
            sb.Append(" {");

            // TODO fix
            //for (var index = cursor.CurrentKeyPath.First; index <= cursor.CurrentKeyPath.Last; index++)
            //{
            //    ref var node = ref cursor.CurrentKeyPath.Items[index];

            //    if (index == cursor.CurrentKeyPath.Current)
            //        sb.Append("*");

            //    string key = null;
            //    //var cp = node.Page.AsContentPage;
            //    //if (cp != null && cp.PageType == PageTypes.DataLeaf)
            //    //{
            //    //    key = Util.GetHexString(cp.GetKey(node.KeyIndex));
            //    //}

            //    sb.AppendFormat("[{0}] {1}.{2} ()",
            //        PageTypes.GetName(node.Page.PageType),
            //        node.Page.PageNumber,
            //        node.KeyIndex,
            //        key);

            //    if (index < cursor.CurrentKeyPath.Last)
            //        sb.Append(" --> ");
            //}

            sb.Append("}");

            return sb.ToString();
        }

        #region Debug Tools

        public void SavePage(AnyPage page)
        {
            var fname = string.Format("Page-{0:0000000000}.dat", page.PageNumber);

            using (var fs = new FileStream(fname, FileMode.Create, FileAccess.ReadWrite))
            {
                fs.Write(page.Bytes.Span);
            }
        }

        #endregion

        private static string GetHexString(byte[] val)
        {
            if (val == null || val.Length == 0)
                return "<null>";

            var sb = new StringBuilder(val.Length * 2);

            for (int i = 0; i < val.Length; i++)
            {
                sb.AppendFormat("{0:x2}", val[i]);
            }

            return sb.ToString();
        }
    }
}
