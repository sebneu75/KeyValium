using KeyValium.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    internal class CacheStats
    {
        public CacheStats(int pagecount, int maxitems, ulong hits, ulong misses,
                          SortedDictionary<int, int> bucketcounts, PageRangeList ranges)
        {
            PageCount = pagecount;
            MaxItems = maxitems;
            Hits = hits;
            Misses = misses;
            Reads = Hits + Misses;
            BucketCounts = bucketcounts;
            Ranges = ranges;
        }

        public readonly int PageCount;
        public readonly int MaxItems;
        public readonly ulong Reads;
        public readonly ulong Hits;
        public readonly ulong Misses;
        public readonly SortedDictionary<int, int> BucketCounts;
        public readonly PageRangeList Ranges;

        public override string ToString()
        {
            double ratio = 0;
            if (Reads > 0)
            {
                ratio = (double)Hits / (double)(Reads);
            }

            var sb = new StringBuilder();

            sb.AppendFormat("*********************** Cache Stats ***********************\n");

            sb.AppendFormat("ItemCount: {0}\n", PageCount);
            sb.AppendFormat("MaxItems: {0}\n", MaxItems);
            sb.AppendFormat("Reads: {0} ({1} Hits / {2} Misses)\n", Reads, Hits, Misses);
            sb.AppendFormat("Hit Ratio: {0:#0.00%}\n", ratio);

            sb.AppendLine("BucketCounts: ");

            foreach (var kvp in BucketCounts.OrderByDescending(x => x.Key))
            {
                sb.AppendFormat("    Count: {0}   Buckets: {1}\n", kvp.Key, kvp.Value);
            }

            sb.AppendFormat("***********************************************************\n");

            return sb.ToString();
        }
    }
}
