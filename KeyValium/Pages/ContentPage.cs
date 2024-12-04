using KeyValium.Cache;
using KeyValium.Cursors;
using KeyValium.Inspector;
using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System.Buffers;
using System.Runtime.InteropServices;
using static KeyValium.Pages.Entries.EntryExtern;

namespace KeyValium.Pages
{
    [StructLayout(LayoutKind.Auto)]
    internal unsafe struct ContentPage
    {
        #region Constructor

        internal ContentPage(AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page != null, "page cannot be null.");

            Page = page;
            Header = Page.Header;
            Content = Page.Bytes.Slice(UniversalHeader.HeaderSize, Header.ContentSize);

            PageType = Page.PageType;

            // read page properties
            ref var props = ref PageTypeProperties.Props[PageType];
            BranchSize = props.BranchSize;
            OffsetEntrySize = props.OffsetEntrySize;
            IndexOffset = props.IndexOffset;
            IsIndexPage = props.IsIndexPage;
            IsFreespacePage = props.IsFsPage;

            EntryOffsetArray = IsFreespacePage ? null : (ushort*)(Content.Pointer + Content.Length - Limits.OffsetEntrySize);
        }

        #endregion

        #region Properties

        internal readonly AnyPage Page;

        /// <summary>
        /// pointer to last ushort of entry offset at the end of the page
        /// </summary>
        internal readonly ushort* EntryOffsetArray;

        /// <summary>
        /// pointer to first byte of content (first byte after header)
        /// </summary>
        internal ByteSpan Content;
        
        internal readonly ushort PageType;

        internal readonly ushort BranchSize;

        internal readonly ushort OffsetEntrySize;

        internal readonly ushort IndexOffset;

        internal UniversalHeader Header;

        internal readonly bool IsIndexPage;

        internal readonly bool IsFreespacePage;

        #endregion

        #region Entries

        internal ushort EntryCount
        {
            get
            {
                Perf.CallCount();

                return Header.KeyCount;
            }
        }

        /// <summary>
        /// Returns the entry size. Size of OffsetEntry and Branchsize is not included
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal ushort GetEntrySize(int index)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

            if (IsFreespacePage)
            {
                return IsIndexPage ? Limits.FreespaceIndexEntrySize : Limits.FreespaceLeafEntrySize;
            }

            if (index == Header.KeyCount - 1)
            {
                return (ushort)(Header.Low - Content.ReadUShort(EntryOffsetArray - index) - BranchSize);
            }

            return (ushort)(Content.ReadUShort(EntryOffsetArray - index - 1) - Content.ReadUShort(EntryOffsetArray - index) - BranchSize);
        }

        /// <summary>
        /// Returns the entry at index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal EntryInline GetEntryAt(int index)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            return new EntryInline(Content.Slice(GetEntryOffset(index), GetEntrySize(index)), PageType);
        }

        /// <summary>
        /// Returns the entry at index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal ByteSpan GetKeyBytesAt(int index)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            //var entry = new EntryInline(Content.Slice(GetEntryOffset(index), GetEntrySize(index)), PageType);

            var offset = GetEntryOffset(index);

            return Content.Slice(offset + Limits.OffsetKeyBytes, Content.ReadUShort(offset + Limits.OffsetKeyLength));
        }

        private void SetEntryOffset(int index, ushort offset)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount + 1, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            Content.WriteUShort(EntryOffsetArray - index, offset);
        }

        //private void UpdateEntryOffset(int index, int delta)
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

        //    // TODO asserts keycount, pagetype, index

        //    var val = Content.ReadUShort(EntryOffsetArray - index);
        //    Content.WriteUShort(EntryOffsetArray - index, (ushort)(val + delta));
        //}

        /// <summary>
        /// updates all entryoffsets from startindex to EntryCount-1
        /// </summary>
        /// <param name="startindex"></param>
        /// <param name="delta"></param>
        private void UpdateEntryOffsets(int startindex, int delta)
        {
            Perf.CallCount();

            KvDebug.Assert(startindex >= 0 && startindex <= Header.KeyCount, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            while (startindex < EntryCount)
            {
                var val = Content.ReadUShort(EntryOffsetArray - startindex);
                Content.WriteUShort(EntryOffsetArray - startindex, (ushort)(val + delta));
                startindex++;
            }
        }

        private byte* GetEntryPointer(int index)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            return Content.Pointer + GetEntryOffset(index);
        }

        internal ushort GetEntryOffset(int index)
        {
            Perf.CallCount();

            KvDebug.Assert(index >= 0 && index < Header.KeyCount, "Index out of range!");

            // TODO asserts keycount, pagetype, index

            if (IsFreespacePage)
            {
                return (ushort)(BranchSize + ((IsIndexPage ? Limits.FreespaceIndexEntrySize : Limits.FreespaceLeafEntrySize) + BranchSize) * index);
            }

            return Content.ReadUShort(EntryOffsetArray - index);
        }

        #endregion

        #region Insert / Update / Delete Entries

        //public bool InsertEntry(ref EntryExtern item)
        //{
        //    Perf.Count();

        //    //KvDebug.Assert(item.Key != null && item.Key.Length > 0, "Key cannot be null or empty.");

        //    // check free space
        //    var needed = (ushort)(item.EntrySize + BranchSize + OffsetEntrySize);
        //    if (Header.FreeSpace < needed)
        //    {
        //        throw new NotSupportedException("Node is full.");
        //    }

        //    var keyspan = item.Key;

        //    // get insert position
        //    var index = GetKeyIndex(ref keyspan, out var exact);
        //    if (exact)
        //    {
        //        throw new NotSupportedException("Key already exists.");
        //    }

        //    return InsertEntry(index, ref item);
        //}

        public bool InsertEntry(int index, ref EntryExtern item, KvPagenumber leftbranch = default, KvPagenumber rightbranch = default)
        {
            Perf.CallCount();

            ValidateIndex(index, true);

            // check free space
            var newentrylength = (ushort)(item.EntrySize + BranchSize);

            if (Header.FreeSpace < newentrylength + OffsetEntrySize)
            {
                throw new NotSupportedException("Node is full.");
            }

            //
            // Insert KeyEntry (KeyEntries are sorted)
            //
            ByteSpan newentry;

            if (index == EntryCount)
            {
                // append at Low end
                newentry = Content.Slice(Header.Low, newentrylength);
            }
            else
            {
                // Move keys to the right
                var ksource = GetEntryPointer(index);
                var ktarget = ksource + newentrylength;
                var klength = Content.Pointer + Header.Low - ksource;
                Content.MoveBytes(ktarget, ksource, (int)klength);

                newentry = Content.Slice(ksource, newentrylength);
            }

            item.WriteEntry(ref newentry);

            //
            // Insert Entry into OffsetArray
            //
            if (OffsetEntrySize > 0)
            {
                ushort newkeyoffset = (ushort)(newentry.Pointer - Content.Pointer);

                if (index == EntryCount)
                {
                    SetEntryOffset(index, newkeyoffset);
                }
                else
                {
                    // Adjust EntryOffsets
                    UpdateEntryOffsets(index, newentrylength);

                    // Move offsets to the left
                    var osource = Content.Pointer + Header.High + 1;
                    var otarget = osource - OffsetEntrySize;
                    var olength = (EntryCount - index) * OffsetEntrySize;
                    Content.MoveBytes(otarget, osource, olength);

                    // set entryoffset
                    SetEntryOffset(index, newkeyoffset);
                }

                // Adjust High
                Header.High -= OffsetEntrySize;
            }

            // Adjust Low 
            Header.Low += newentrylength;

            // Adjust KeyCount
            Header.KeyCount++;

            KvDebug.Assert(item.EntrySize == GetEntrySize(index), "Item size mismatch!");

            // update branches
            if (IsIndexPage)
            {
                if (leftbranch != 0)
                {
                    Content.WriteULong(GetEntryPointer(index) - BranchSize, leftbranch);
                }

                if (rightbranch != 0)
                {
                    Content.WriteULong(GetEntryPointer(index) + GetEntrySize(index), rightbranch);
                }
            }

            ValidateEntries();

            return true;
        }

        //internal bool UpdateEntry(EntryExtern item)
        //{
        //    Perf.Count();

        //    //KvDebug.Assert(item.Key != null && item.Key.Length > 0, "Key cannot be null or empty.");

        //    var span = item.Key;
        //    // get insert position
        //    var index = GetKeyIndex(ref span, out var exact);

        //    if (!exact)
        //    {
        //        throw new NotSupportedException("Key does not exist.");
        //    }

        //    //var oldsize = GetEntryAt(index).EntrySize;
        //    var oldsize = GetEntrySize(index);

        //    if ((Header.FreeSpace + oldsize) < (item.EntrySize + BranchSize))
        //    {
        //        throw new NotSupportedException("Node is full.");
        //    }

        //    return UpdateEntry(index, item);
        //}

        internal bool UpdateEntry(int index, ref EntryExtern item)
        {
            Perf.CallCount();

            ValidateIndex(index, true);

            // check free space
            var newentrysize = item.EntrySize;
            //var oldentrysize = GetEntryAt(index).EntrySize;
            var oldentrysize = GetEntrySize(index);

            if ((Header.FreeSpace + oldentrysize) < newentrysize)
            {
                throw new NotSupportedException("Node is full.");
            }

            //
            // Update KeyEntry (KeyEntries are sorted)
            //
            var newkey = Content.Slice(GetEntryOffset(index), newentrysize);

            if (index == EntryCount - 1)
            {
                // overwrite last entry
                // no Memory move necessary
            }
            else if (newentrysize != oldentrysize)
            {
                // Move right keys                 
                var ksource = GetEntryPointer(index + 1); // move entries to the right
                var ktarget = ksource - oldentrysize + newentrysize;
                var klength = Content.Pointer + Header.Low - ksource;
                Content.MoveBytes(ktarget, ksource, (int)klength);

                if (OffsetEntrySize > 0)
                {
                    // Adjust EntryOffsets
                    UpdateEntryOffsets(index + 1, newentrysize - oldentrysize);
                }
            }

            item.WriteEntry(ref newkey);

            // Adjust Low
            Header.Low += (ushort)(newentrysize - oldentrysize);

            //KvDebug.Assert(item.EntrySize == GetEntryAt(index).EntrySize, "Item size mismatch!");
            KvDebug.Assert(item.EntrySize == GetEntrySize(index), "Item size mismatch!");

            ValidateEntries();

            return true;
        }

        //public bool DeleteEntry(ByteStream key)
        //{
        //    Perf.Count();

        //    KvDebug.Assert(key != null && key.Length > 0, "Key cannot be null or empty.");

        //    var keyspan = key.AsSpan;

        //    // get delete position
        //    var index = GetKeyIndex(ref keyspan, out var exact);
        //    if (!exact)
        //    {
        //        throw new NotSupportedException("Key not found.");
        //    }

        //    return DeleteEntry(index);
        //}

        public bool DeleteEntry(int index)
        {
            Perf.CallCount();

            ValidateIndex(index, false);

            // Save old values
            //var oldentrylength = (ushort)(GetEntryAt(index).EntrySize + BranchSize);
            var oldentrylength = (ushort)(GetEntrySize(index) + BranchSize);

            //
            // Delete KeyEntry
            //
            if (index == EntryCount - 1)
            {
                // if last entry, no move is needed
            }
            else
            {
                // Move keys to the left
                var ksource = GetEntryPointer(index + 1);
                var ktarget = ksource - oldentrylength;
                var klength = Content.Pointer + Header.Low - ksource;
                Content.MoveBytes(ktarget, ksource, (int)klength);
            }

            //
            // Delete Entry from OffsetArray
            //
            if (OffsetEntrySize > 0)
            {
                if (index == EntryCount - 1)
                {
                    // if last entry, no move is needed
                }
                else
                {
                    // Adjust EntryOffsets
                    UpdateEntryOffsets(index + 1, -oldentrylength); // +1 because the current entry gets deleted

                    // Move offsets to the right
                    var osource = Content.Pointer + Header.High + 1;
                    var otarget = osource + OffsetEntrySize;
                    var olength = (EntryCount - index - 1) * OffsetEntrySize;
                    Content.MoveBytes(otarget, osource, olength);
                }

                // Adjust High
                Header.High += OffsetEntrySize;
            }

            // Adjust Low
            Header.Low -= oldentrylength;

            // Adjust KeyCount
            Header.KeyCount--;

            ValidateEntries();

            FillFreeSpace();

            return true;
        }

        #endregion

        #region Branches for IndexPages

        public KvPagenumber GetLeftBranch(int index)
        {
            Perf.CallCount();

            if (IsIndexPage)
            {
                ValidateIndex(index, true);

                if (index == EntryCount)
                {
                    return Content.ReadULong(Content.Pointer + Header.Low - BranchSize);
                    //return GetRightBranch(index - 1);
                }

                return Content.ReadULong(GetEntryPointer(index) - BranchSize);
            }

            throw new NotSupportedException("Wrong Pagetype!");
        }

        public KvPagenumber GetRightBranch(int index)
        {
            Perf.CallCount();

            if (IsIndexPage)
            {
                ValidateIndex(index, false);

                //return Unsafe.ReadULong(GetEntryPointer(index) + GetEntryAt(index).EntrySize);
                return Content.ReadULong(GetEntryPointer(index) + GetEntrySize(index));
            }

            throw new NotSupportedException("Wrong Pagetype!");
        }

        public void SetLeftBranch(int index, KvPagenumber pageno)
        {
            Perf.CallCount();

            if (IsIndexPage)
            {
                ValidateIndex(index, true);

                if (index == EntryCount)
                {
                    SetRightBranch(index - 1, pageno);
                    return;
                }

                Content.WriteULong(GetEntryPointer(index) - BranchSize, pageno);

                return;
            }

            throw new NotSupportedException("Wrong Pagetype!");
        }

        public void SetRightBranch(int index, KvPagenumber pageno)
        {
            Perf.CallCount();

            if (IsIndexPage)
            {
                ValidateIndex(index, false);
                //Unsafe.WriteULong(GetEntryPointer(index) + GetEntryAt(index).EntrySize, pageno);
                Content.WriteULong(GetEntryPointer(index) + GetEntrySize(index), pageno);

                return;
            }

            throw new NotSupportedException("Wrong Pagetype!");
        }

        #endregion

        #region Binary Search in Entries

        /// <summary>
        /// returns the index of the first key that is greater or equal than the given key 
        /// this is the index where insertion must happen
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="exact">true, if the key matches exactly</param>
        /// <returns></returns>
        internal int GetKeyIndex(ref ReadOnlySpan<byte> key, out bool exact)
        {
            Perf.CallCount();

            int left = 0;
            int right = EntryCount;
            while (left < right)
            {
                var pivot = (left + right) >> 1;
                var result = UniversalComparer.CompareBytesAsSequence(GetKeyBytesAt(pivot), ref key);
                //var result = IsFreespacePage ? UniversalComparer.CompareFreespace(GetKeyBytesAt(pivot), ref key) :
                //                               UniversalComparer.CompareBytesAsSequence(GetKeyBytesAt(pivot), ref key);
                //UniversalComparer.CompareBytesUlongWise(GetKeyBytesAt(pivot), ref key);
                if (result == 0)
                {
                    // never return true for indexpages
                    exact = !IsIndexPage;

                    // for index pages search the first key that is greater than the given key
                    // so we can always take the left branch
                    return pivot + IndexOffset;
                }
                else if (result < 0)
                {
                    left = pivot + 1;
                }
                else
                {
                    right = pivot;
                }
            }

            exact = false;
            return left;
        }

        //public int GetKeyIndexFs(ref ReadOnlySpan<byte> key, out bool exact)
        //{
        //    Perf.CallCount();

        //    int left = 0;
        //    int right = EntryCount;
        //    while (left < right)
        //    {
        //        var pivot = (left + right) >> 1;
        //        //var result = CompareKeys(GetKeyBytesAt(pivot), ref key);
        //        var result = UniversalComparer.CompareFreespace(GetKeyBytesAt(pivot), ref key);                             
        //        //UniversalComparer.CompareBytesUlongWise(GetKeyBytesAt(pivot), ref key);
        //        if (result == 0)
        //        {
        //            // never return true for indexpages
        //            exact = !IsIndexPage;

        //            // for index pages search the first key that is greater than the given key
        //            // so we can always take the left branch
        //            return pivot + SplitIndexOffset;
        //        }
        //        else if (result < 0)
        //        {
        //            left = pivot + 1;
        //        }
        //        else
        //        {
        //            right = pivot;
        //        }
        //    }

        //    exact = false;
        //    return left;
        //}

        //public int GetKeyIndexData(ref ReadOnlySpan<byte> key, out bool exact)
        //{
        //    Perf.CallCount();

        //    int left = 0;
        //    int right = EntryCount;
        //    while (left < right)
        //    {
        //        var pivot = (left + right) >> 1;
        //        //var result = CompareKeys(GetKeyBytesAt(pivot), ref key);
        //        var result = UniversalComparer.CompareBytesAsSequence(GetKeyBytesAt(pivot), ref key);
        //        if (result == 0)
        //        {
        //            // never return true for indexpages
        //            exact = !IsIndexPage;

        //            // for index pages search the first key that is greater than the given key
        //            // so we can always take the left branch
        //            return pivot + SplitIndexOffset;
        //        }
        //        else if (result < 0)
        //        {
        //            left = pivot + 1;
        //        }
        //        else
        //        {
        //            right = pivot;
        //        }
        //    }

        //    exact = false;
        //    return left;
        //}


        /// <summary>
        /// returns the index of the first key that is greater than the given key 
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="exact">true, if the key already exists</param>
        /// <returns></returns>
        //public int GetKeyIndexGt(ref ReadOnlySpan<byte> key)
        //{
        //    Perf.CallCount();

        //    int left = 0;
        //    int right = EntryCount;
        //    while (left < right)
        //    {
        //        var pivot = (left + right) >> 1;
        //        //var result = CompareKeys(GetKeyBytesAt(pivot), ref key);
        //        var result = IsFreespacePage ? UniversalComparer.CompareFreespace(GetKeyBytesAt(pivot), ref key) :
        //                                       UniversalComparer.CompareBytesAsSequence(GetKeyBytesAt(pivot), ref key);
        //        UniversalComparer.CompareBytesUlongWise(GetKeyBytesAt(pivot), ref key);
        //        if (result == 0)
        //        {
        //            return pivot + 1;
        //        }
        //        else if (result < 0)
        //        {
        //            left = pivot + 1;
        //        }
        //        else
        //        {
        //            right = pivot;
        //        }
        //    }

        //    return left;
        //}

        #endregion

        #region Split / Merge

        private (ushort, bool) GetSplitIndex(int insertindex, bool appendmode)
        {
            Perf.CallCount();

            KvDebug.Assert((IsIndexPage && EntryCount >= Limits.MinKeysPerIndexPage) ||
                         (!IsIndexPage && EntryCount >= Limits.MinKeysPerLeafPage), "Too few keys for split");

            ushort splitindex;

            // do not use appendmode in freespace pages because free space entries are usually randomly inserted
            if (!IsFreespacePage && appendmode && insertindex == EntryCount)
            {
                return ((IsIndexPage ? (ushort)(EntryCount - 1) : (ushort)EntryCount), true);
            }

            if (EntryCount < 4)
            {
                splitindex = 1;
            }
            else
            {
                if (IsFreespacePage)
                {
                    // freespace entries have fixed size, so we can simply divide entry count by 2
                    splitindex = (ushort)((EntryCount + IndexOffset) >> 1);
                }
                else
                {
                    //int half = (Header.ContentSize - EntryCount * OffsetEntrySize - BranchSize);
                    int half = Header.ContentSize >> 1;

                    //Console.WriteLine("Half: {0}", half);
                    int sum = BranchSize;

                    int i;
                    // find first entry that starts at an offset greater or equal to half
                    for (i = 0; i < EntryCount - 1; i++)
                    {
                        sum += GetEntrySize(i) + BranchSize + OffsetEntrySize;

                        if (sum > half)
                        {
                            break;
                        }

                        //if (EntryOffsetArray[-i] == half)
                        //{
                        //    splitindex = (ushort)i;
                        //    break;
                        //}
                        //else if (EntryOffsetArray[-i] > half)
                        //{
                        //    splitindex = (ushort)(i - 1);
                        //    break;
                        //}
                    }

                    //if (sum == half)
                    //{
                    //    splitindex = (ushort)(i+1);
                    //}
                    //if (sum > half)
                    //{
                    splitindex = (ushort)(i);
                    //}


                    //if (splitindex == 0)
                    //{
                    //    splitindex = (ushort)(EntryCount - 1);
                    //}
                }
            }

            if (IsIndexPage && splitindex == EntryCount - 1)
            {
                // can't split at last key in indexpages
                splitindex--;
            }

            KvDebug.Assert(splitindex > 0 && splitindex < (EntryCount - IndexOffset), "Splitpoint not found!");

            return (splitindex, false);
        }


        /// <summary>
        /// splits this page into rightpage 
        /// </summary>
        /// <param name="rightpage">an empty page into which to split</param>
        /// <param name="insertindex">the insertionpoint in the full page</param>
        /// <returns></returns>
        internal (bool, int, ushort, KeyFromPool?) Split(KeyPool pool, ref ContentPage rightpage, int insertindex, bool appendmode)
        {
            Perf.CallCount();

            DumpPage(ref this, "Source");

            //
            // determine splitindex
            //
            var (splitindex, append) = GetSplitIndex(insertindex, appendmode);

            //ushort splitindex = 0;

            //if (EntryCount < 4)
            //{
            //    splitindex = 1;
            //}
            //else
            //{
            //    var half = this.Header.UsedSpace >> 1;
            //    int sum = BranchSize;

            //    // TODO find better algorithm
            //    for (ushort i = 0; i < EntryCount; i++)
            //    {
            //        //sum += GetEntryAt(i).EntrySize + BranchSize + OffsetEntrySize;
            //        sum += GetEntrySize(i) + BranchSize + OffsetEntrySize;
            //        if (sum > half)
            //        {
            //            splitindex = i;
            //            break;
            //        }
            //    }
            //}

            DumpSplitInfo(ref this, splitindex, insertindex);

            // insert into left by default
            ref var inserttargetpage = ref this;
            KeyFromPool? splitkey = null;

            if (append)
            {
                //inserttargetpage = ref rightpage;
                insertindex = 0;

                if (IsIndexPage)
                {
                    // extract splitkey
                    splitkey = pool.CopyKey(GetKeyBytesAt(splitindex));

                    this.Header.Low -= (ushort)(GetEntrySize(splitindex) + BranchSize);
                    this.Header.High += (ushort)(OffsetEntrySize * IndexOffset);
                    this.Header.KeyCount = (ushort)(splitindex);
                }

                // return page where index lies, adjusted insertindex and splitkey for indexpages
                return (false, insertindex, splitindex, splitkey);
            }

            var isleft = true;

            if (insertindex > splitindex)
            {
                // insert into right page
                isleft = false;
                inserttargetpage = ref rightpage;

                if (!IsIndexPage && splitindex < EntryCount - 1)
                {
                    splitindex++;
                }

                insertindex -= (ushort)(splitindex + IndexOffset);

                DumpSplitInfo(ref this, splitindex, insertindex);
            }

            if (splitindex < EntryCount)
            {
                // move keys to rightpage
                var sourcekeys = GetEntryPointer(splitindex + IndexOffset) - BranchSize;
                var targetkeys = rightpage.Content.Pointer;
                ushort keyslen = (ushort)(Content.Pointer + Header.Low - sourcekeys); // ( GetEntryPointer(splitindex + SplitIndexOffset) - BranchSize));
                rightpage.Content.MoveBytes(targetkeys, sourcekeys, keyslen);

                // adjust rightpage
                rightpage.Header.KeyCount = (ushort)(EntryCount - splitindex - IndexOffset);
                rightpage.Header.Low = keyslen;

                // move offsets to rightpage
                if (OffsetEntrySize > 0)
                {
                    // move offsets to right page
                    var osource = Content.Pointer + Header.High + 1;
                    var olength = (EntryCount - splitindex - IndexOffset) * OffsetEntrySize;
                    var otarget = rightpage.Content.Pointer + rightpage.Header.ContentSize - olength;
                    rightpage.Content.MoveBytes(otarget, osource, olength);

                    // adjust high
                    rightpage.Header.High -= (ushort)olength;

                    // determine change in entryoffsets
                    var delta = (int)(sourcekeys - this.Content.Pointer);
                    KvDebug.Assert(delta >= 0, "Delta is negative.");

                    rightpage.UpdateEntryOffsets(0, -delta);

                    // adjust high
                    this.Header.High += (ushort)(olength + OffsetEntrySize * IndexOffset);
                }

                if (IsIndexPage)
                {
                    // extract splitkey
                    splitkey = pool.CopyKey(GetKeyBytesAt(splitindex));

                    //this.Header.Low -= (ushort)(GetEntryAt(splitindex).EntrySize);
                    this.Header.Low -= (ushort)(GetEntrySize(splitindex));
                }

                // adjust left page
                this.Header.KeyCount = (ushort)(splitindex);
                this.Header.Low -= keyslen;
            }
            else if (IsIndexPage)
            {
                // FAIL
                throw new NotSupportedException("Cannot split Page. SplitIndex out of bounds.");
            }

#if DEBUG
            //var fs = Limits.MaxTotalEntrySize(inserttargetpage);
            //DumpPage(this, "Left Page" + (isleft ? " (Target)" : ""));
            //DumpPage(rightpage, "Right Page" + (!isleft ? " (Target)" : ""));
            //KvDebug.Assert(inserttargetpage.Header.FreeSpace >= fs, "Freespace mismatch!");
#endif
            this.FillFreeSpace();

            // return page where index lies, adjusted insertindex and splitkey for indexpages
            return (isleft, insertindex, splitindex, splitkey);
        }

        /// <summary>
        /// merges this page with rightpage 
        /// </summary>
        /// <param name="rightpage">a page from which to merge</param>
        /// <returns></returns>
        internal ContentPage Merge(ref ContentPage rightpage, bool hasentry, ref EntryExtern indexentry)
        {
            Perf.CallCount();

            var ret = this;

            if (IsIndexPage)
            {
                KvDebug.Assert(hasentry, "IndexEntry not set!");

                // append indexentry
                this.InsertEntry(EntryCount, ref indexentry);
            }

            // append keys to this page
            var sourcekeys = rightpage.Content.Pointer;
            var targetkeys = this.Content.Pointer + this.Header.Low - BranchSize;
            ushort keyslen = rightpage.Header.Low;
            Content.MoveBytes(targetkeys, sourcekeys, keyslen);

            // adjust this page
            this.Header.KeyCount += (ushort)(rightpage.EntryCount);
            this.Header.Low += (ushort)(keyslen - BranchSize);

            // append offsets to this page
            if (OffsetEntrySize > 0)
            {
                // move offsets to right page
                var osource = rightpage.Content.Pointer + rightpage.Header.High + 1;
                var olength = rightpage.EntryCount * OffsetEntrySize;
                var otarget = Content.Pointer + Header.High - olength + 1;
                Content.MoveBytes(otarget, osource, olength);

                // adjust high
                // TODO check if necessary
                // rightpage.Header.High -= (ushort)olength;

                // determine change in entryoffsets
                var delta = (int)(targetkeys - Content.Pointer);
                KvDebug.Assert(delta >= 0, "Delta is negative.");

                UpdateEntryOffsets(EntryCount - rightpage.EntryCount, delta);

                // adjust high
                this.Header.High -= (ushort)(olength);
            }

            // TODO check if rightpage needs to be cleared

            return ret;
        }

        #endregion

        #region Content Accessors

        //public EntryExtern GetEntry(byte[] key)
        //{
        //    Performance.Counters.Count();

        //    KvDebug.Assert(key != null && key.Length > 0, "Key cannot be null or empty.");

        //    var index = GetKeyIndex(key, out var exact);
        //    if (exact)
        //    {
        //        var entry = GetEntryAt(index);

        //        return entry.GetEntryExtern();
        //    }

        //    return null;
        //}

        public EntryExtern GetEntry(int index)
        {
            Perf.CallCount();

            ValidateIndex(index, false);

            var entry = GetEntryAt(index);

            return entry.ToEntryExtern();
        }

        public byte[] GetKey(int index)
        {
            Perf.CallCount();

            ValidateIndex(index, false);

            return GetKeyBytesAt(index).ToArray();
        }

        //public bool ExistsKey(byte[] key)
        //{
        //    Performance.Counters.Count();

        //    GetKeyIndex(key, out var exact);

        //    return exact;
        //}

        #endregion

        #region DebugTools

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="allowkeycount">true if index==keycount is allowed (insert)</param>
        [Conditional("DEBUG")]
        private static void DumpPage(ref ContentPage cp, string name)
        {
            return;

            var page = cp.Page;

            Console.WriteLine("********************************************************");
            Console.WriteLine("{3} Pagenumber: {0}    PageType: {1}    PageSize: {2}", page.PageNumber, page.PageType, page.PageSize, name);
            Console.WriteLine("Entries: {0}", cp.EntryCount);

            for (int i = 0; i < cp.EntryCount; i++)
            {
                Console.WriteLine("Entry[{0}]: {1} ({2})", i, cp.GetEntrySize(i) + cp.BranchSize + cp.OffsetEntrySize, cp.GetEntryOffset(i));
            }

            Console.WriteLine("Used Space: {0}", cp.Header.UsedSpace);
            Console.WriteLine("Free Space: {0}", cp.Header.FreeSpace);

            Console.WriteLine("********************************************************");
        }

        private void DumpSplitInfo(ref ContentPage cp, ushort splitindex, int insertindex)
        {
            return;

            Console.WriteLine("InsertIndex: {0}", insertindex);
            Console.WriteLine("SplitIndex: {0}", splitindex);

            var left = 0;
            var right = 0;
            for (int i = 0; i < cp.EntryCount; i++)
            {
                var size = cp.GetEntrySize(i) + cp.BranchSize + cp.OffsetEntrySize;

                if (i < splitindex)
                {
                    left += size;
                }
                else
                {
                    right += size;
                }
            }

            Console.WriteLine("({0} - {1})", left, right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="allowkeycount">true if index==keycount is allowed (insert)</param>
        [Conditional("DEBUG")]
        private void ValidateIndex(int index, bool allowkeycount)
        {
            if (!KvDebugOptions.ValidateIndex) return;

            if (allowkeycount)
            {
                if (!(index >= 0 && index <= EntryCount))
                {
                    throw new NotSupportedException("Index out of bounds.");
                }
            }
            else
            {
                if (!(index >= 0 && index < EntryCount))
                {
                    throw new NotSupportedException("Index out of bounds.");
                }
            }
        }

        [Conditional("DEBUG")]
        public void ValidateEntries()
        {
            if (!KvDebugOptions.ValidateEntries) return;

            byte* lastkeyoffset = null;

            for (int i = 0; i < EntryCount; i++)
            {
                var keyoffset = GetEntryPointer(i);

                if (keyoffset <= lastkeyoffset)
                {
                    throw new NotSupportedException("EntryOffset smaller than last.");
                }

                if (keyoffset - Content.Pointer >= this.Header.ContentSize)
                {
                    throw new NotSupportedException("EntryOffset outside content.");
                }

                lastkeyoffset = keyoffset;
            }
        }

        [Conditional("DEBUG")]
        public void FillFreeSpace()
        {
            if (!KvDebugOptions.FillFreeSpace) return;

            var b = PageValidator.GetFillByte(Header.PageType);
            var p = Content.Pointer + Header.Low;

            for (int i = Header.Low; i <= Header.High; i++, p++)
            {
                *p = b;
            }
        }

        #endregion
    }
}
