﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="page">The page to validate</param>
        /// <param name="pagenumber">The expected page number</param>
        /// <param name="iswrite">true, if called before a write access</param>
        /// <exception cref="NotSupportedException"></exception>
        internal unsafe void ValidatePage(AnyPage page, KvPagenumber pagenumber, bool iswrite)
        {
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


            if (page.PageType == PageTypes.FileHeader)
            {
                KvDebug.Assert(page.Header.Magic == Limits.Magic, "Wrong Magic!"); // || page.Header.Magic == Limits.ReverseMagic
                KvDebug.Assert(page.Header.PageNumber == pagenumber, "Wrong page number!");

                // TODO
            }
            else if (page.PageType == PageTypes.Meta)
            {
                KvDebug.Assert(page.Header.Magic == Limits.Magic, "Wrong Magic!");
                KvDebug.Assert(page.Header.PageNumber == pagenumber, "Wrong page number!");

                // TODO
            }
            else if (page.PageType == PageTypes.DataOverflow)
            {
                KvDebug.Assert(page.Header.Magic == Limits.Magic, "Wrong Magic!");
                KvDebug.Assert(page.Header.PageNumber == pagenumber, "Wrong page number!");

                KvDebug.Assert(page.Header.Tid > 0, "Wrong Tid!");
                KvDebug.Assert(page.Header.ContentLength > 0, "Wrong ContentLength!");
            }
            else if (PageTypes.IsContentPageType(page.PageType))
            {
                KvDebug.Assert(page.Header.Magic == Limits.Magic, "Wrong Magic!");
                KvDebug.Assert(page.Header.PageNumber == pagenumber, "Wrong page number!");

                KvDebug.Assert(page.Header.Tid > 0, "Wrong Tid!");

                KvDebug.Assert(page.Header.KeyCount > 0, "Wrong KeyCount");
                KvDebug.Assert(page.Header.FreeSpace <= page.Header.ContentSize, "Wrong FreeSpace");

                ref var cp = ref page.AsContentPage;

                var high = cp.Content.Pointer + cp.Header.ContentSize - cp.OffsetEntrySize * cp.Header.KeyCount - 1 - cp.Content.Pointer;
                KvDebug.Assert(cp.Header.High == high, "Wrong high value!");
            }
            else
            {
                throw new NotSupportedException("Unhandled Page Type.");
            }

            if (page.AsContentPage.Content.Length != 0)
            {
                page.AsContentPage.ValidateEntries();
            }
        }

        internal static void ValidateFileHeader(AnyPage page)
        {
            // TODO
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