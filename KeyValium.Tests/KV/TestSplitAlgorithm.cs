using KeyValium.Cursors;
using KeyValium.Pages.Entries;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.KV
{
    public class TestSplitAlgorithm : IDisposable
    {
        public TestSplitAlgorithm()
        {
            pdb = new PreparedKeyValium("NullValues");
        }

        PreparedKeyValium pdb;

        const int CONTENT_SIZE = 4096 - 32;

        private Random _rnd = new Random();

        [Fact]
        public void SplitAlgorithm()
        {
            for (int i = 0; i < 100000; i++)
            {
                var isfreespace = _rnd.NextDouble() > 0.5;

                var list = isfreespace ? GetFreeSpaceList(2, 100) : GetRandomList(2, 100);

                var isindexpage = list.Count > 2 ? _rnd.NextDouble() > 0.5 : false;
                var splitindexoffset = isindexpage ? 1 : 0;
                var branchsize = isindexpage ? 8 : 0;

                var si = GetSplitIndex(list, isindexpage, splitindexoffset, isfreespace, branchsize);
            }
        }

        private List<int> GetFreeSpaceList(int min, int max)
        {
            var ret = new List<int>();

            var count = _rnd.Next(min, max);
            var sum = 0;

            for (int i = 0; i < count; i++)
            {
                var val = Limits.FreespaceLeafEntrySize;

                if (sum + val <= CONTENT_SIZE)
                {
                    ret.Add(val);
                }
            }

            return ret;
        }


        private List<int> GetRandomList(int min, int max)
        {
            var ret = new List<int>();

            var count = _rnd.Next(min, max);
            var sum = 0;

            for (int i = 0; i < count; i++)
            {
                var val = _rnd.Next(512);

                if (sum + val + 2 <= CONTENT_SIZE)
                {
                    ret.Add(val);
                    sum += val;
                }
            }

            return ret;
        }

        // TODO fix
        private ushort GetSplitIndex(List<int> items, bool isindexpage, int splitindexoffset, bool isfreespace, int branchsize)
        {
            System.Diagnostics.Debug.Assert((isindexpage && items.Count >= Limits.MinKeysPerIndexPage) ||
                         (!isindexpage && items.Count >= Limits.MinKeysPerLeafPage), "Too few keys for split");

            ushort splitindex = 0;

            var entryoffsets = new List<int>();

            var sum = branchsize;
            for (int i = 0; i < items.Count; i++)
            {
                entryoffsets.Add(sum);
                sum += items[i] + branchsize;
            }

            if (items.Count < 4)
            {
                splitindex = 1;
            }
            else
            {
                if (isfreespace)
                {
                    // TODO check
                    splitindex = (ushort)((items.Count + splitindexoffset) >> 1);
                }
                else
                {
                    int half = CONTENT_SIZE - items.Count * 2 - branchsize;
                    half >>= 1;
                    half += branchsize;

                    sum = 0;

                    // find first entry that starts at an offset greater or equal to half
                    // first key cannot be the split key, so skip it
                    for (int i = 1; i < items.Count; i++)
                    {
                        //var size = GetEntrySize(i) + BranchSize;
                        if (entryoffsets[i] == half)
                        {
                            splitindex = (ushort)i;
                            break;
                        }
                        else if (entryoffsets[i] > half)
                        {
                            splitindex = (ushort)(i - 1);
                            break;
                        }
                    }

                    if (splitindex == 0)
                    {
                        splitindex = (ushort)(items.Count - 1);
                    }
                }
            }

            if (isindexpage && splitindex == items.Count - 1)
            {
                // can't split at last key in indexpages
                splitindex--;
            }

            System.Diagnostics.Debug.Assert(splitindex > 0 && splitindex < (items.Count - splitindexoffset), "Splitpoint not found!");


            if (items.Count >= 4)
            {
                var sum1 = items.Take(splitindex).Sum(x => x);
                var sum2 = items.Skip(splitindex).Sum(x => x);

                System.Diagnostics.Debug.Assert(sum1 <= CONTENT_SIZE / 2);
                //Debug.Assert(sum1 <= sum2, "Sum mismatch!");
            }

            return splitindex;
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}



