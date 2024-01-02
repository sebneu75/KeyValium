using KeyValium.Inspector;
using KeyValium.Pages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    /// <summary>
    /// Contains all Validations for pages. Validation happens when a page is read from or written to disk.
    /// </summary>
    internal class PageValidator
    {
        internal PageValidator(Database db, PageValidationMode mode)
        {
            Database = db;
            Mode = mode;
        }

        internal readonly Database Database;

        internal readonly PageValidationMode Mode;

        /// <summary>
        /// Validates a page. Throws an exception if validation fails.
        /// </summary>
        /// <param name="page">The page to validate.</param>
        /// <param name="pageno">The expected page number.</param>
        /// <param name="iswrite">True, if called before a write access.</param>
        /// <exception cref="NotSupportedException"></exception>
        internal unsafe void ValidatePage(AnyPage page, KvPagenumber pageno, bool iswrite)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (Mode == PageValidationMode.None)
            {
                return;
            }

            if (page.PageType == PageTypes.Raw)
            {
                // do nothing with raw pages
                return;
            }

            if (iswrite && !Mode.HasFlag(PageValidationMode.BeforeWriteToDisk))
            {
                return;
            }
            else if (!iswrite && !Mode.HasFlag(PageValidationMode.AfterReadFromDisk))
            {
                return;
            }

            //
            // validate common header
            //
            switch (page.PageType)
            {
                case PageTypes.DataIndex:
                case PageTypes.DataLeaf:
                case PageTypes.FsIndex:
                case PageTypes.FsLeaf:
                case PageTypes.DataOverflow:
                case PageTypes.Meta:
                case PageTypes.FileHeader:
                    ValidateCommonHeader(page, pageno, iswrite);
                    break;

                default:
                    Error(page, pageno, iswrite, "Unknown page type {0}.", page.PageType);
                    break;
            }

            //
            // validate other headers and content
            // meta has no additional headers
            // fileheader is validated in other method
            //
            switch (page.PageType)
            {
                case PageTypes.DataIndex:
                case PageTypes.DataLeaf:
                case PageTypes.FsIndex:
                case PageTypes.FsLeaf:
                    ValidateContentHeader(page, pageno, iswrite);
                    ValidateContentPage(page, pageno, iswrite);
                    break;

                case PageTypes.DataOverflow:
                    ValidateOverflowHeader(page, pageno, iswrite);
                    break;

                case PageTypes.Meta:
                    var mp = new MetaPage(page);
                    ValidateMetaPage(page, pageno, iswrite, mp);
                    break;

                case PageTypes.FileHeader:
                    ValidateFileHeader(page, pageno, iswrite);
                    break;
            }
        }

        private void ValidateContentPage(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var cp = ref page.AsContentPage;

            if (cp.IsFreespacePage)
            {
                ValidateFsPage(page, pageno, iswrite);
            }
            else
            {
                ValidateDataPage(page, pageno, iswrite);
            }
        }

        private void ValidateFsPage(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var cp = ref page.AsContentPage;

            var csize = cp.Header.ContentSize;
            var low = cp.Header.Low;
            var high = cp.Header.High;
            var keys = cp.Header.KeyCount;

            //
            // check branches
            //
            if (cp.IsIndexPage)
            {
                for (int i = 0; i <= keys; i++)
                {
                    var branch = cp.GetLeftBranch(i);

                    // branch must be greater than number of metapages
                    if (branch <= Limits.MetaPages)
                    {
                        Error(page, pageno, iswrite, "Branch contains invalid page number.");
                    }
                }
            }

            //
            // check flags
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                var flags = entry.Flags;

                // Flags must be zero in free space pages
                if (flags != 0)
                {
                    Error(page, pageno, iswrite, "Flags are nonzero.");
                }
            }

            //
            // check keys
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                if (entry.KeyLength != Limits.FreespaceKeySize)
                {
                    Error(page, pageno, iswrite, "Key has wrong size.");
                }

                if (entry.FirstPage <= Limits.MetaPages)
                {
                    Error(page, pageno, iswrite, "FirstPage is invalid.");
                }

                // TODO keys must be ordered ascending
            }

            //
            // check values
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                if (!cp.IsIndexPage)
                {
                    if (entry.LastPage <= Limits.MetaPages)
                    {
                        Error(page, pageno, iswrite, "LastPage is invalid.");
                    }

                    if (entry.FirstPage > entry.LastPage)
                    {
                        Error(page, pageno, iswrite, "FirstPage is greater than LastPage.");
                    }
                }
            }

            // check sort order
        }

        private void ValidateDataPage(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var cp = ref page.AsContentPage;

            var csize = cp.Header.ContentSize;
            var low = cp.Header.Low;
            var high = cp.Header.High;
            var keys = cp.Header.KeyCount;

            // check offset table size
            var csize2 = high + cp.OffsetEntrySize * keys + 1;
            if (csize != csize2)
            {
                Error(page, pageno, iswrite, "High is not directly before start of offset table.");
            }

            var lastoffset = ushort.MaxValue;

            //
            // check offset table
            //
            for (int i = 0; i < keys; i++)
            {
                var offset = cp.GetEntryOffset(i);

                if (i == 0)
                {
                    // first offset must match branchsize
                    if (offset != cp.BranchSize)
                    {
                        Error(page, pageno, iswrite, "First offset does not match branch size.");
                    }
                }
                else
                {
                    // offsets must be ascending
                    if (offset <= lastoffset)
                    {
                        Error(page, pageno, iswrite, "Offsets are not ascending.");
                    }
                }

                // offsets must be smaller than low
                if (offset >= low)
                {
                    Error(page, pageno, iswrite, "Offset is greater than Low.");
                }

                // save last offset
                lastoffset = offset;
            }

            //
            // check branches
            //
            if (cp.IsIndexPage)
            {
                for (int i = 0; i <= keys; i++)
                {
                    var branch = cp.GetLeftBranch(i);

                    // branch must be greater than number of metapages
                    if (branch <= Limits.MetaPages)
                    {
                        Error(page, pageno, iswrite, "Branch contains invalid page number.");
                    }

                }
            }

            //
            // check flags
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                var flags = entry.Flags;

                if (cp.IsIndexPage)
                {
                    // Flags must be zero in index pages
                    if (flags != 0)
                    {
                        Error(page, pageno, iswrite, "Flags are nonzero.");
                    }
                }
                else
                {
                    if ((flags & EntryFlags.IsOverflow) != 0)
                    {
                        // if overflow flag is set hasvalue flag must be set
                        if ((flags & EntryFlags.HasValue) == 0)
                        {
                            Error(page, pageno, iswrite, "Invalid flags IsOverflow and not HasValue.");
                        }

                        // check overflow page number
                        if (entry.OverflowPageNumber <= Limits.MetaPages)
                        {
                            Error(page, pageno, iswrite, "OverflowPageNumber is invalid.");
                        }

                        // check overflow length
                        if (entry.OverflowLength == 0)
                        {
                            Error(page, pageno, iswrite, "OverflowLength is invalid.");
                        }
                    }

                    if ((flags & EntryFlags.HasSubtree) != 0)
                    {
                        // check subtree page number
                        if (entry.SubTree.Value > 0 && entry.SubTree.Value <= Limits.MetaPages)
                        {
                            Error(page, pageno, iswrite, "SubTree points to meta page.");
                        }

                        // local count must be less or equal to total count
                        if (entry.LocalCount > entry.TotalCount)
                        {
                            Error(page, pageno, iswrite, "LocalCount is larger than TotalCount.");
                        }

                        // if subtree is 0 counts must be 0
                        if (entry.SubTree.Value == 0 && (entry.LocalCount != 0 || entry.TotalCount != 0))
                        {
                            Error(page, pageno, iswrite, "SubTree is zero but count is nonzero.");
                        }
                    }
                }
            }

            //
            // check keys
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                if (entry.KeyLength > Database.Limits.MaximumKeySize)
                {
                    Error(page, pageno, iswrite, "Key too long.");
                }

                // TODO keys must be ordered ascending
            }

            //
            // check values
            //
            for (int i = 0; i < keys; i++)
            {
                var entry = cp.GetEntryAt(i);

                if (!cp.IsIndexPage)
                {
                    if (entry.InlineValueLength > Database.Limits.MaxInlineValueSize(entry.KeyLength))
                    {
                        Error(page, pageno, iswrite, "InlineValue is greater MaxInlineValueSize.");
                    }
                }
            }

            // check max key length

            // check sort order
        }

        private void ValidateMetaPage(AnyPage page, ulong pageno, bool iswrite, MetaPage mp)
        {
            if (mp.HeaderTid != mp.FooterTid)
            {
                Error(page, pageno, iswrite, "HeaderTid does not match FooterTid.");
            }

            if (mp.FsRootPage > 0 && mp.FsRootPage <= Limits.MetaPages)
            {
                Error(page, pageno, iswrite, "FsRootPage points to meta page.");
            }

            if (mp.FsRootPage > mp.LastPage)
            {
                Error(page, pageno, iswrite, "FsRootPage is larger than LastPage.");
            }

            if (mp.DataRootPage > 0 && mp.DataRootPage <= Limits.MetaPages)
            {
                Error(page, pageno, iswrite, "DataRootPage points to meta page..");
            }

            if (mp.DataRootPage > mp.LastPage)
            {
                Error(page, pageno, iswrite, "DataRootPage is larger than LastPage.");
            }

            if (mp.DataLocalCount > mp.DataTotalCount)
            {
                Error(page, pageno, iswrite, "DataLocalCount is larger than DataTotalCount.");
            }

            if (mp.FsLocalCount > mp.FsTotalCount)
            {
                Error(page, pageno, iswrite, "FsLocalCount is larger than FsTotalCount.");
            }

            if (mp.DataRootPage == 0 && (mp.DataLocalCount != 0 || mp.DataTotalCount != 0))
            {
                Error(page, pageno, iswrite, "DataRootPage is zero but count is nonzero.");
            }

            if (mp.FsRootPage == 0 && (mp.FsLocalCount != 0 || mp.FsTotalCount != 0))
            {
                Error(page, pageno, iswrite, "FsRootPage is zero but count is nonzero.");
            }
        }

        private void ValidateContentHeader(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var header = ref page.Header;

            // check unused2
            if (header.Unused2 != 0)
            {
                Error(page, pageno, iswrite, "Unused2 is nonzero.");
            }

            var csize = header.ContentSize;

            //
            // check low and high
            // 
            var low = header.Low;
            var high = header.High;

            if (low >= csize)
            {
                Error(page, pageno, iswrite, "Low is outside of content.");
            }

            if (high >= csize)
            {
                Error(page, pageno, iswrite, "High is outside of content.");
            }

            if (low > high && low - high > 1)
            {
                Error(page, pageno, iswrite, "Low is larger than High.");
            }

            //
            // check keycount
            //
            var keycount = header.KeyCount;

            if (keycount == 0)
            {
                Error(page, pageno, iswrite, "KeyCount is zero.");
            }

            // an entry occupies at least 4 bytes (Flags + KeyLength)
            if (keycount > csize / 4)
            {
                Error(page, pageno, iswrite, "KeyCount is too large.");
            }

            //
            // check free space
            // 
            if (header.UsedSpace == 0)
            {
                Error(page, pageno, iswrite, "Page is empty.");
            }
        }

        private void ValidateOverflowHeader(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var header = ref page.Header;

            if (header.ContentLength == 0)
            {
                Error(page, pageno, iswrite, "Content length is zero.");
            }
        }

        internal static void ValidateCommonHeader(AnyPage page, ulong pageno, bool iswrite)
        {
            ref var header = ref page.Header;

            // check magic
            if (header.Magic != Limits.Magic)
            {
                Error(page, pageno, iswrite, "Magic mismatch.");
            }

            // check page type done by caller

            // check unused1
            if (header.Unused1 != 0)
            {
                Error(page, pageno, iswrite, "Unused1 is nonzero.");
            }

            //check page number
            if (header.PageNumber != pageno)
            {
                Error(page, pageno, iswrite, "Pagenumber mismatch.");
            }

            // check Tid except on file header
            if (header.PageType != PageTypes.FileHeader && header.PageType != PageTypes.Meta)
            {
                if (header.Tid == 0)
                {
                    Error(page, pageno, iswrite, "Tid is zero.");
                }
            }
        }

        internal static void ValidateFileHeader(AnyPage page, ulong pageno, bool iswrite)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (page.PageType != PageTypes.FileHeader)
            {
                Error(page, pageno, iswrite, "Fileheader has wrong page type.");
            }

            if (page.PageNumber != 0)
            {
                Error(page, pageno, iswrite, "Nonzero header page number.");
            }

            ValidateCommonHeader(page, pageno, iswrite);

            var header = page.Header;

            // check Version
            if (header.Version != 1)
            {
                Error(page, pageno, iswrite, "Version is not 1.");
            }

            // check page size exponent
            if (header.PageSizeExponent < 8 || header.PageSizeExponent > 16)
            {
                Error(page, pageno, iswrite, "Page size exponent is out of range.");
            }

            // check flags
            var flags = (ushort)header.Flags;
            if (flags > 1)
            {
                Error(page, pageno, iswrite, "Flags are not set correctly.");
            }

            // check unused3
            if (header.Unused3 != 0)
            {
                Error(page, pageno, iswrite, "Unused3 is nonzero.");
            }

            // internal type code 
            if (header.InternalTypeCode > InternalTypes.MultiDictionary)
            {
                Error(page, pageno, iswrite, "Internal type code is out of range.");
            }

            // user type code is not checked
        }

        private static void Error(AnyPage page, ulong pageno, bool iswrite, string format, params object[] args)
        {
            var header = string.Format("Validation error in page {0} on {1}: ", pageno, iswrite ? "Write" : "Read");

            var msg = header + string.Format(format, args);

            throw new KeyValiumException(ErrorCodes.PageValidation, msg);
        }

        public static byte GetFillByte(ushort pagetype)
        {
            Perf.CallCount();

            switch (pagetype)
            {
                case PageTypes.FileHeader:
                    return 0x48;    // H
                case PageTypes.Meta:
                    return 0x4d;    // M
                case PageTypes.DataIndex:
                    return 0x49;    // I
                case PageTypes.DataLeaf:
                    return 0x4c;    // L
                case PageTypes.DataOverflow:
                    return 0x4f;    // O
                case PageTypes.FsIndex:
                    return 0x66;    // f
                case PageTypes.FsLeaf:
                    return 0x46;    // F
            }

            return 0;
        }

    }
}
