namespace KeyValium.Pages.Entries
{
    internal unsafe ref struct EntryInline
    {
        internal EntryInline(ByteSpan entry, ushort pagetype)
        {
            Perf.CallCount();

            Entry = entry;
            PageType = pagetype;

            PageTypes.ValidateContentPageType(PageType);
        }

        internal readonly ushort PageType;

        /// <summary>
        /// Pointer to Entry
        /// </summary>
        internal readonly ByteSpan Entry;

        /// <summary>
        /// size of index entry
        /// </summary>
        internal ushort EntrySize
        {
            get
            {
                Perf.CallCount();

                var ret = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);   // Flags + [KeyLength] + KeyLength

                if (PageType == PageTypes.DataLeaf)
                {
                    if ((Flags & EntryFlags.HasSubtree) != 0)
                    {
                        ret += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong);  // Subtree, TotalCount, LocalCount
                    }

                    if ((Flags & EntryFlags.IsOverflow) != 0)
                    {
                        ret += sizeof(KvPagenumber) + sizeof(ulong);    // OverflowPage + OverflowLength
                    }
                    else if ((Flags & EntryFlags.HasValue) != 0)
                    {
                        ret += sizeof(ushort);                  // [Valuelength]
                        ret += InlineValueLength;               // Valuelength   
                    }
                }
                else if (PageType == PageTypes.FsLeaf)
                {
                    ret += sizeof(KvPagenumber) + sizeof(KvTid);  // LastPage, Tid
                }

                return ret;
            }
        }

        internal ushort Flags
        {
            get
            {
                Perf.CallCount();

                return Entry.ReadUShort(Limits.OffsetFlags);
            }
        }

        /// <summary>
        /// length of key
        /// </summary>
        internal ushort KeyLength
        {
            get
            {
                Perf.CallCount();

                return Entry.ReadUShort(Limits.OffsetKeyLength);
            }
        }

        /// <summary>
        /// pointer to Keybytes in index pages
        /// </summary>
        internal ByteSpan KeyBytes
        {
            get
            {
                Perf.CallCount();

                return new ByteSpan(Entry.Pointer + Limits.OffsetKeyBytes, KeyLength);
            }
        }

        internal ushort InlineValueLength
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    return sizeof(ulong) + sizeof(ulong);   // OverflowPage + OverflowLength
                }
                else if ((Flags & EntryFlags.HasValue) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                    if ((Flags & EntryFlags.HasSubtree) != 0)
                    {
                        offset += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong); // Subtree + TotalCount + LocalCount
                    }

                    return Entry.ReadUShort(0 + offset);
                }

                return 0;
            }
        }

        internal ByteSpan InlineValueBytes
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasValue) == 0)
                {
                    return new ByteSpan(null, 0);
                }

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    return new ByteSpan(null, 0);
                }

                ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    offset += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong); // Subtree, TotalCount, LocalCount
                }
                offset += sizeof(ushort); // [ValueLength]

                return new ByteSpan(Entry.Pointer + offset, InlineValueLength);
            }
        }

        public KvPagenumber? SubTree
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                    return Entry.ReadULong(0 + offset);
                }

                return null;
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                    if (value.HasValue)
                    {
                        Entry.WriteULong(offset, value.Value);
                    }
                    else
                    {
                        Entry.WriteULong(offset, 0);
                    }
                }
                else
                {
                    throw new NotSupportedException("Subtree flag not set. Cannot write SubTree.");
                }
            }
        }

        public ulong TotalCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength + sizeof(KvPagenumber));
                    return Entry.ReadULong(offset);
                }

                return 0;
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength + sizeof(KvPagenumber));
                    Entry.WriteULong(offset, value);
                }
                else
                {
                    throw new NotSupportedException("Subtree flag not set. Cannot write TotalCount.");
                }
            }
        }

        public ulong LocalCount
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength + sizeof(KvPagenumber) + sizeof(ulong));
                    return Entry.ReadULong(offset);
                }

                return 0;
            }
            set
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.HasSubtree) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength + sizeof(KvPagenumber) + sizeof(ulong));
                    Entry.WriteULong(offset, value);
                }
                else
                {
                    throw new NotSupportedException("Subtree flag not set. Cannot write LocalCount.");
                }
            }
        }

        public KvPagenumber OverflowPageNumber
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                    if ((Flags & EntryFlags.HasSubtree) != 0)
                    {
                        offset += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong); // Subtree, TotalCount, LocalCount
                    }

                    return Entry.ReadULong(0 + offset);
                }

                return 0;
            }
        }

        public ulong OverflowLength
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.DataLeaf, "Invalid page type!");

                if ((Flags & EntryFlags.IsOverflow) != 0)
                {
                    ushort offset = (ushort)(sizeof(ushort) + sizeof(ushort) + KeyLength);
                    if ((Flags & EntryFlags.HasSubtree) != 0)
                    {
                        offset += sizeof(KvPagenumber) + sizeof(ulong) + sizeof(ulong); // Subtree, TotalCount, LocalCount
                    }
                    offset += sizeof(ulong);

                    return Entry.ReadULong(0 + offset);
                }

                return 0;
            }
        }

        public KvPagenumber FirstPage
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FsLeaf || PageType == PageTypes.FsIndex, "Invalid page type!");

                return Entry.ReadULongBE(Limits.OffsetKeyBytes);

            }
        }

        public KvPagenumber LastPage
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FsLeaf, "Invalid page type!");

                return Entry.ReadULong(Limits.OffsetKeyBytes + sizeof(KvPagenumber));
            }
        }

        public ulong PageCount
        {
            get
            {
                Perf.CallCount();

                return LastPage - FirstPage + 1;
            }
        }

        public KvTid Tid
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(PageType == PageTypes.FsLeaf, "Invalid page type!");

                return Entry.ReadULong(Limits.OffsetKeyBytes + sizeof(KvPagenumber) + sizeof(KvPagenumber));
            }
        }

        #region Conversion

        /// <summary>
        /// returns an external leaf entry
        /// </summary>
        /// <returns></returns>
        public EntryExtern ToEntryExtern()
        {
            Perf.CallCount();

            switch (PageType)
            {
                case PageTypes.DataIndex:
                    return new EntryExtern(KeyBytes.ToArray());

                case PageTypes.DataLeaf:
                    var valinfo = new ValInfo(InlineValueBytes.ToArray());
                    return new EntryExtern(KeyBytes.ToArray(), valinfo, SubTree, TotalCount, LocalCount, OverflowPageNumber, OverflowLength);

                case PageTypes.FsIndex:
                    return new EntryExtern(FirstPage);

                case PageTypes.FsLeaf:
                    return new EntryExtern(FirstPage, LastPage, Tid);
            }

            throw new KeyValiumException(ErrorCodes.InternalError, "Invalid Pagetype!");
        }

        #endregion
    }
}
