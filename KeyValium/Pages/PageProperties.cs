using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Pages
{
    [StructLayout(LayoutKind.Auto)]
    internal struct PageTypeProperties
    {
        /// <summary>
        /// An array of page type properties. The PageType is used as an index into the array
        /// </summary>
        internal static readonly PageTypeProperties[] Props = new PageTypeProperties[PageTypes.MAX_CONTENT_PAGETYPE + 1]
            {
                new PageTypeProperties(0, 0, 0, false, false),                                      // 0x00 - Unused 
                new PageTypeProperties(sizeof(KvPagenumber), sizeof(ushort), 1, true, false),       // 0x01 - DataIndex
                new PageTypeProperties(0, sizeof(ushort), 0, false, false),                         // 0x02 - DataLeaf
                new PageTypeProperties(sizeof(KvPagenumber), 0, 1, true, true),                     // 0x03 - FsIndex
                new PageTypeProperties(0, 0, 0, false, true)                                        // 0x04 - FsLeaf
            };

        private PageTypeProperties(ushort bsize, ushort osize, ushort sindex, bool isindex, bool isfs)
        {
            Perf.CallCount();

            BranchSize = bsize;
            OffsetEntrySize = osize;
            IndexOffset = sindex;
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
        /// returns the IndexOffset
        /// 1 for Indexpages, otherwise 0 
        /// used for adjusting the SplitIndex and the KeyIndex in IndexPages 
        /// </summary>
        internal readonly ushort IndexOffset;

        /// <summary>
        /// true if the pagetype is an index page
        /// </summary>
        internal readonly bool IsIndexPage;

        /// <summary>
        /// true if the pagetype is a freespace page
        /// </summary>
        internal readonly bool IsFsPage;
    }
}
