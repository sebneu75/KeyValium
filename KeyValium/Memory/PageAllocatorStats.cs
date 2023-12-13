using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Memory
{
    internal class PageAllocatorStats
    {
        internal PageAllocatorStats(HashSet<AnyPage> usedpages, int queued, int inuse, ulong allocated, ulong used, ulong recycled, ulong deallocated, ulong copied, ulong copiednew)
        {
            Queued = queued;
            InUse = inuse;
            Allocated = allocated;
            Used = used;
            Recycled = recycled;
            Deallocated = deallocated;
            RefCounts = usedpages.GroupBy(x => x.RefCount).OrderByDescending(x => x.Key).ToList();
            Copied = copied;
            CopiedNew = copiednew;
        }

        internal readonly int Queued;
        internal readonly int InUse;
        internal readonly ulong Allocated;
        internal readonly ulong Used;
        internal readonly ulong Recycled;
        internal readonly ulong Deallocated;
        internal readonly ulong Copied;
        internal readonly ulong CopiedNew;

        internal readonly List<IGrouping<int, AnyPage>> RefCounts;

        public bool HasRefCountsGT(int count)
        {
            return RefCounts.Any(x => x.Key > count);
        }

        public long ItemsWithRefCount(int count)
        {
            var group = RefCounts.FirstOrDefault(x => x.Key == count);

            return group == null ? 0 : group.Count();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("*********************** PageAllocator Stats ***********************\n");

            sb.AppendFormat("     Queued: {0}\n", Queued);
            sb.AppendFormat("      InUse: {0}\n", InUse);
            sb.AppendFormat("  Allocated: {0}\n", Allocated);
            sb.AppendFormat("       Used: {0}\n", Used);
            sb.AppendFormat("   Recycled: {0}\n", Recycled);
            sb.AppendFormat("Deallocated: {0}\n", Deallocated);
            sb.AppendFormat("     Copied: {0}\n", Copied);
            sb.AppendFormat("  CopiedNew: {0}\n", CopiedNew);

            foreach (var group in RefCounts)
            {
                sb.AppendFormat("InUse (RefCount={0}): {1}\n", group.Key, group.Count());

                var groups = group.GroupBy(x => x.PageType).ToList();
                foreach (var g in groups.OrderBy(x => x.Key))
                {
                    var pages = g.OrderBy(x => x.PageNumber).Take(20).ToList();

                    sb.AppendFormat("    PageType {0}: {1} [{2}]\n", g.Key, g.Count(), string.Join(',', pages.Select(x => x.PageNumber)));
                }
            }

            sb.AppendFormat("*******************************************************************\n");


            return sb.ToString();
        }
    }
}
