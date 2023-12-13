using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Pages
{
    internal struct PageProperties
    {
        /// <summary>
        /// An array of page properties. The PageType is used as an index into the array
        /// </summary>
        internal static readonly PageProperties[] Props = new PageProperties[PageTypes.MAX_CONTENT_PAGETYPE + 1]
            {
                new PageProperties(0,0,0,false, false),                                     // 0x00 - Unused 
                new PageProperties(sizeof(KvPagenumber), sizeof(ushort),1, true, false),    // 0x01 - DataIndex
                new PageProperties(0, sizeof(ushort), 0, false, false),                     // 0x02 - DataLeaf
                new PageProperties(sizeof(KvPagenumber), 0, 1, true, true),                 // 0x03 - FsIndex
                new PageProperties(0,0,0,false, true)                                       // 0x04 - FsLeaf
            };

        private PageProperties(ushort bsize, ushort osize, ushort sindex, bool isindex, bool isfs)
        {
            Perf.CallCount();

            BranchSize = bsize;
            OffsetEntrySize = osize;
            SplitIndexOffset = sindex;
            IsIndexPage = isindex;
            IsFsPage = isfs;
        }

        /// <summary>
        /// Branchsizes
        /// sizeof(ulong) for Indexpages, otherwise 0         
        /// </summary>
        internal readonly ushort BranchSize;

        /// <summary>
        /// Offset Entrysizes
        /// sizeof(ushort) for Datapages, otherwise 0 
        /// </summary>
        internal readonly ushort OffsetEntrySize;

        /// <summary>
        /// returns the SplitIndexOffset
        /// 1 for Indexpages, otherwise 0 
        /// the key at SplitIndex will be moved to the parent page when an indexpage split happens)
        /// </summary>
        internal readonly ushort SplitIndexOffset;

        /// <summary>
        /// true if the pagetype is an index page
        /// </summary>
        internal readonly bool IsIndexPage;

        /// <summary>
        /// true if the pagetype is an freespace page
        /// </summary>
        internal readonly bool IsFsPage;
    }
}
