using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Tests.Collections
{
    public class TestKvHashSet : IDisposable
    {
        const int BUFFERSIZE = 64;

        public TestKvHashSet()
        {
        }

        [Fact]
        public void TestLruCache()
        {
            var rnd = new Random();

            var count = 10000;

            var dict = new Dictionary<ulong, PageRef>();
            var cache = new LruCache(count);

            for (int i = 0; i < count; i++)
            {
                for (int k = 0; k < 100; k++)
                {
                    // insert random numbers
                    var num = (ulong)rnd.Next(1, count);
                    var pr = new PageRef(num, null, num);

                    cache.UpsertPage(ref pr);
                    dict[pr.PageNumber] = pr;
                }

                CompareSets(dict, cache._pages);

                for (int k = 0; k < 100; k++)
                {
                    // remove random numbers
                    var num = (ulong)rnd.Next(1, count);
                    var pr = new PageRef(num, null, num);

                    cache.RemovePage(pr.PageNumber);
                    dict.Remove(pr.PageNumber);
                }

                CompareSets(dict, cache._pages);
            }

            //for (int i = 0; i < 100000; i++)
            //{
            //    var pr = cache.GetPage(new KvPagenumber((ulong)i));
            //    if (pr == null)
            //    {
            //        throw new NotSupportedException();
            //    }
            //}
        }

        [Fact]
        public void TestList()
        {
            var list = new KvList<ulong>();

            for (int i = 0; i < 100000; i++)
            {
                list.InsertFirst((ulong)i);
            }

            list.InsertFirst(10);
            list.InsertFirst(20);
            list.InsertFirst(30);

            list.RemoveLast(out var pageno);
            list.RemoveLast(out pageno);
            list.RemoveLast(out pageno);

            list.RemoveLast(out pageno);
        }

        [Fact]
        public void Test1()
        {
            var hash = new KvHashSet();

            for (ulong pageno = 0; pageno < 1000; pageno++)
            {
                hash.Add(pageno);

                Assert.True(hash.Contains(pageno), "FAIL!");

                Assert.False(hash.Contains(pageno + 1), "FAIL!");
            }

            for (ulong pageno = 0; pageno < 1000; pageno++)
            {
                Assert.True(hash.Contains(pageno), "FAIL!");

                hash.Remove(pageno);

                Assert.False(hash.Contains(pageno), "FAIL!");
            }
        }

        private static void CompareSets(Dictionary<ulong, PageRef> dict, KvDictionary<PageRef> cache)
        {
            var items1 = dict.Select(x => x.Key).ToHashSet();
            var items2 = cache.ToList().Select(x => x.Key).ToHashSet();

            Assert.True(items1.Count == items2.Count, "Count differs!");

            items1.SymmetricExceptWith(items2);

            Assert.True(items1.Count == 0, "Item mismatch!");
        }

        public void Dispose()
        {

        }
    }
}






