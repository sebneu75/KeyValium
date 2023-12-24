namespace KeyValium.Pages
{
    internal static class PageTypes
    {
        // for overflow pages
        internal const ushort Raw = 0x00;

        // Data
        internal const ushort DataIndex = 0x01;
        internal const ushort DataLeaf = 0x02;

        // Freespace
        internal const ushort FsIndex = 0x03;
        internal const ushort FsLeaf = 0x04;

        // DataOverflow
        internal const ushort DataOverflow = 0x08;

        // Meta
        internal const ushort Meta = 0x10;

        // File Header
        internal const ushort FileHeader = 0x20;        
        
        // Lockfile Header
        internal const ushort LockFile = 0x80;

        /// <summary>
        /// returns the name of the given pagetype
        /// </summary>
        /// <param name="pagetype">the pagetype</param>
        /// <returns>name of the given pagetype</returns>
        public static string GetName(ushort pagetype)
        {
            Perf.CallCount();

            switch (pagetype)
            {
                case PageTypes.Raw:
                    return "Raw";
                case PageTypes.DataIndex:
                    return "DataIndex";
                case PageTypes.DataLeaf:
                    return "DataLeaf";
                case PageTypes.FsIndex:
                    return "FsIndex";
                case PageTypes.FsLeaf:
                    return "FsLeaf";
                case PageTypes.DataOverflow:
                    return "DataOverflow";
                case PageTypes.FileHeader:
                    return "FileHeader";
                case PageTypes.Meta:
                    return "Meta";
                case PageTypes.LockFile:
                    return "LockFile";
            }

            return "Unknown";
        }

        /// <summary>
        /// smallest content pagetype
        /// </summary>
        internal const ushort MIN_CONTENT_PAGETYPE = 1;

        /// <summary>
        /// largest content pagetype
        /// </summary>
        internal const ushort MAX_CONTENT_PAGETYPE = 4;

        internal static void ValidateContentPageType(ushort pagetype)
        {
            Perf.CallCount();

            if (!(pagetype >= MIN_CONTENT_PAGETYPE && pagetype <= MAX_CONTENT_PAGETYPE))
            {
                throw new KeyValiumException(ErrorCodes.InvalidPageType, "Invalid page type!");
            }
        }

        internal static bool IsContentPageType(ushort pagetype)
        {
            Perf.CallCount();

            return pagetype >= MIN_CONTENT_PAGETYPE && pagetype <= MAX_CONTENT_PAGETYPE;
        }
    }
}
