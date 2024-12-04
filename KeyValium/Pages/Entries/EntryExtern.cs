using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace KeyValium.Pages.Entries
{
    /// <summary>
    /// wrapper for a Leaf Entry that does not reside within a node
    /// (for insert)
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal unsafe ref struct EntryExtern
    {
        static EntryExtern()
        {
            if (sizeof(FsKeyBuffer) != Limits.FreespaceKeySize)
            {
                throw new KeyValiumException(ErrorCodes.InternalError, "sizeof(FsKeyBuffer) != FsKeyLength!");
            }
        }

        #region Constructors

        /// <summary>
        /// Constructor for IndexEntries
        /// </summary>
        /// <param name="key"></param>
        internal EntryExtern(ReadOnlySpan<byte> key)
        {
            Perf.CallCount();

            PageType = PageTypes.DataIndex;
            IsFreespaceEntry = false;

            _key = key;

            //Value = default;
            //OverflowPageNumber = default;
            //OverflowLength = default;
            //SubTree = default;
            //LocalCount = default;
            //TotalCount = default;

            //
            // Calculate Flags
            //
            Flags = EntryFlags.None;

            //
            // Calculate EntrySize
            //
            EntrySize = (ushort)(sizeof(ushort) + sizeof(ushort) + Key.Length);   // Flags + [KeyLength] + KeyLength
        }

        /// <summary>
        /// Constructor for LeafEntries
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="subtree"></param>
        /// <param name="totalcount"></param>
        /// <param name="localcount"></param>
        /// <param name="ovpageno"></param>
        /// <param name="ovlength"></param>
        /// <returns></returns>
        internal EntryExtern(ReadOnlySpan<byte> key, ValInfo val, KvPagenumber? subtree, ulong totalcount, ulong localcount, KvPagenumber ovpageno, ulong ovlength)
        {
            Perf.CallCount();

            PageType = PageTypes.DataLeaf;
            IsFreespaceEntry = false;

            _key = key;

            Value = val;
            SubTree = subtree;
            TotalCount = totalcount;
            LocalCount = localcount;
            OverflowPageNumber = ovpageno;
            OverflowLength = ovlength;

            //
            // Calculate flags and size
            //
            Flags = EntryFlags.None;
            EntrySize = (ushort)(sizeof(ushort) + sizeof(ushort) + Key.Length);   // Flags + [KeyLength] + KeyLength            
            
            if (OverflowPageNumber != 0)
            {
                Flags |= EntryFlags.HasValue | EntryFlags.IsOverflow;
                EntrySize += sizeof(KvPagenumber) + sizeof(ulong);   // OverflowPage + OverflowLength
            }
            else if (Value.Length > 0)
            {
                Flags |= EntryFlags.HasValue;
                EntrySize += sizeof(ushort);    // Valuelength
                EntrySize += (ushort)Value.Length;
            }

            if (SubTree.HasValue)
            {
                Flags |= EntryFlags.HasSubtree;
                EntrySize += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong); // Subtree, TotalCount, LocalCount
            }
        }

        /// <summary>
        /// Constructor for FsLeaf-Entries
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="firstpage"></param>
        /// <param name="lastpage"></param>
        /// <returns></returns>
        internal EntryExtern(KvPagenumber firstpage, KvPagenumber lastpage, KvTid tid)
        {
            Perf.CallCount();

            PageType = PageTypes.FsLeaf;

            IsFreespaceEntry = true;

            //
            // Calculate Flags
            //
            Flags = EntryFlags.None;

            // write key to buffer
            BinaryPrimitives.WriteUInt64BigEndian(FsSpan, firstpage);

            LastPage = lastpage;
            Tid = tid;

            //
            // Calculate EntrySize
            //
            EntrySize = Limits.FreespaceLeafEntrySize;
        }

        /// <summary>
        /// Constructor for FsIndex-Entries
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="firstpage"></param>
        /// <param name="lastpage"></param>
        /// <returns></returns>
        internal EntryExtern(KvPagenumber firstpage)
        {
            Perf.CallCount();

            PageType = PageTypes.FsIndex;

            IsFreespaceEntry = true;

            //
            // Calculate Flags
            //
            Flags = EntryFlags.None;

            // write key to buffer
            BinaryPrimitives.WriteUInt64BigEndian(FsSpan, firstpage);

            //
            // Calculate EntrySize
            //
            EntrySize = Limits.FreespaceIndexEntrySize;
        }

        #endregion

        #region Variables

        internal readonly ulong OverflowLength;
        internal readonly ulong TotalCount;
        internal readonly ulong LocalCount;
        internal readonly KvPagenumber LastPage;
        internal readonly KvTid Tid;

        internal readonly KvPagenumber OverflowPageNumber;
        internal readonly KvPagenumber? SubTree;

        internal readonly bool IsFreespaceEntry;

        internal ReadOnlySpan<byte> _key;

        internal ValInfo Value;

        internal readonly ushort PageType;
        internal readonly ushort EntrySize;
        internal readonly ushort Flags;

        internal FsKeyBuffer FsKey;

        #endregion

        #region Properties

        internal ReadOnlySpan<byte> Key
        {
            get
            {
                if (IsFreespaceEntry)
                {
                    return FsSpan;
                }

                return _key;
            }
        }

        internal Span<byte> FsSpan
        {
            get
            {
                return MemoryMarshal.CreateSpan(ref FsKey._byte00, Limits.FreespaceKeySize);
            }
        }

        public KvPagenumber FirstPage
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FsIndex || PageType == PageTypes.FsLeaf, "Invalid page type!");
                KvDebug.Assert(Key.Length == Limits.FreespaceKeySize, "Invalid Length!");

                return BinaryPrimitives.ReadUInt64BigEndian(FsSpan);
            }
        }

        //public KvPagenumber LastPage
        //{
        //    get
        //    {
        //        Perf.CallCount();

        //        KvDebug.Assert(PageType == PageTypes.FsIndex || PageType == PageTypes.FsLeaf, "Invalid page type!");
        //        KvDebug.Assert(Key.Length == FsKeyLength, "Invalid Length!");

        //        fixed (byte* bp = Key)
        //        {
        //            return Unsafe.ReadULong(bp + sizeof(KvPagenumber));
        //        }
        //    }
        //}

        //public KvTid Tid
        //{
        //    get
        //    {
        //        Perf.CallCount();

        //        KvDebug.Assert(PageType == PageTypes.FsIndex || PageType == PageTypes.FsLeaf, "Invalid page type!");
        //        KvDebug.Assert(Key.Length == FsKeyLength, "Invalid Length!");

        //        fixed (byte* bp = Key)
        //        {
        //            return Unsafe.ReadULong(bp + sizeof(KvPagenumber) + sizeof(KvPagenumber));
        //        }
        //    }
        //}

        //public ulong PageCount
        //{
        //    get
        //    {
        //        Perf.CallCount();

        //        KvDebug.Assert(PageType == PageTypes.FsIndex || PageType == PageTypes.FsLeaf, "Invalid page type!");

        //        return LastPage - FirstPage + 1;
        //    }
        //}

        #endregion

        /// <summary>
        /// Writes the leaf entry to a page
        /// </summary>
        /// <param name="target">Pointer to start of Content</param>
        internal void WriteEntry(ref ByteSpan target)
        {
            Perf.CallCount();

            var offset = 0;

            var key = Key;

            // Flags
            target.WriteUShort(offset, Flags);
            offset += sizeof(ushort);

            // KeyLength
            target.WriteUShort(offset, (ushort)key.Length);
            offset += sizeof(ushort);

            // Key
            target.WriteBytes(offset, ref key);

            if (PageType == PageTypes.DataLeaf)
            {
                offset += key.Length;

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    target.WriteULong(offset, SubTree.Value);
                    offset += sizeof(KvPagenumber);
                    target.WriteULong(offset, TotalCount);
                    offset += sizeof(ulong);
                    target.WriteULong(offset, LocalCount);
                    offset += sizeof(ulong);
                }

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    target.WriteULong(offset, OverflowPageNumber);
                    offset += sizeof(KvPagenumber);
                    target.WriteULong(offset, OverflowLength);
                }
                else if ((Flags & EntryFlags.HasValue) != 0)
                {
                    // write value length
                    target.WriteUShort(offset, (ushort)Value.Length);
                    offset += sizeof(ushort);

                    Value.CopyTo(new Span<byte>(target.Pointer + offset, (ushort)Value.Length));
                }
            }
            else if (PageType == PageTypes.FsLeaf)
            {
                offset += key.Length;
                target.WriteULong(offset, LastPage);
                offset += sizeof(KvPagenumber);
                target.WriteULong(offset, Tid);
            }
        }

        /// <summary>
        /// Writes the leaf entry to a page
        /// </summary>
        /// <param name="target">Pointer to start of Content</param>
        [Conditional("DEBUG")]
        internal void WriteEntry2(Span<byte> target)
        {
            Perf.CallCount();

            var keylen = (ushort)Key.Length;

            BinaryPrimitives.WriteUInt16LittleEndian(target, Flags);
            target = target.Slice(sizeof(ushort));

            // KeyLength
            BinaryPrimitives.WriteUInt16LittleEndian(target, keylen);
            target = target.Slice(sizeof(ushort));

            // Key
            Key.CopyTo(target);

            //if (PageType == PageTypes2.FsIndex)
            //{
            //    if (Tid==0 )
            //    {
            //        Console.WriteLine();
            //    }
            //}

            if (PageType == PageTypes.DataLeaf)
            {
                target = target.Slice(keylen);

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(target, SubTree.Value);
                    target = target.Slice(sizeof(KvPagenumber));
                    BinaryPrimitives.WriteUInt64LittleEndian(target, TotalCount);
                    target = target.Slice(sizeof(ulong));
                    BinaryPrimitives.WriteUInt64LittleEndian(target, LocalCount);
                    target = target.Slice(sizeof(ulong));
                }

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(target, OverflowPageNumber);
                    target = target.Slice(sizeof(KvPagenumber));
                    BinaryPrimitives.WriteUInt64LittleEndian(target, OverflowLength);
                }
                else if ((Flags & EntryFlags.HasValue) != 0)
                {
                    // write value length
                    BinaryPrimitives.WriteUInt16LittleEndian(target, (ushort)Value.Length);
                    target = target.Slice(sizeof(ushort));

                    Value.CopyTo(target);
                }
            }
            else if (PageType == PageTypes.FsLeaf)
            {
                target = target.Slice(keylen);

                BinaryPrimitives.WriteUInt64LittleEndian(target, LastPage);
                target = target.Slice(sizeof(KvPagenumber));
                BinaryPrimitives.WriteUInt64LittleEndian(target, Tid);
            }
        }

        #region Copy

        //internal EntryExtern CopyFreespaceEntry()
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(PageType == PageTypes.FsLeaf, "Invalid page type!");

        //    return new EntryExtern(false, Tid, FirstPage, LastPage);
        //}

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct FsKeyBuffer
        {
            // 8 bytes for the freespace key
            internal byte _byte00;
            byte _byte01;
            byte _byte02;
            byte _byte03;
            byte _byte04;
            byte _byte05;
            byte _byte06;
            byte _byte07;
        }
    }
}

