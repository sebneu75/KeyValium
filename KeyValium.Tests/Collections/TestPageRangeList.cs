using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium;
using KeyValium.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Tests.Collections
{
    public class TestPageRangeList : IDisposable
    {
        const int BUFFERSIZE = 64;

        public TestPageRangeList()
        {
        }

        [Fact]
        public void Test1()
        {
            var tree = new RedBlackTree<int>();
            var set = new SortedSet<int>();

            var list = GetList(100);

            foreach (var item in list)
            {
                set.Add(item);
                tree.Insert(item);
            }
        }

        [Fact]
        public void Test2()
        {
            //var ranges1 = new PageRangeListOld();
            var ranges2 = new PageRangeList();

            ulong count = 1000;
            var rnd = new Random();

            for (ulong i = 0; i < count; i += 2)
            {
                //ranges1.AddPage(i);
                ranges2.AddPage(i);
            }

            for (ulong i = 0; i < count; i += 2)
            {
                //ranges1.AddPage(i + 1);
                ranges2.AddPage(i + 1);
            }

            //PrintRanges(ranges1);
            PrintRanges(ranges2);

            //var count1 = ranges1.Ranges.Count;
            var count2 = ranges2.RangeCount;

            //if (count1 != count2)
            //{
            //    throw new Exception("Count mismatch!");
            //}
        }

        [Fact]
        public void Test3()
        {
            var ranges = new PageRangeList();

            ulong count = 1000;

            for (ulong i = 0; i < count; i += 2)
            {
                ranges.AddPage(i);
            }

            ranges.RemoveAllInRange(1, 997);

            Assert.True(ranges.RangeCount == 2);
            Assert.True(ranges.PageCount == 2);

            var msg = string.Join(",", ranges.ToList());

            Console.WriteLine(msg);
        }

        [Fact]
        public void Test4()
        {
            var ranges = new PageRangeList();

            ranges.AddRange(10, 20);

            // gap before
            var range1 = ranges.Copy();
            range1.RemoveAllInRange(0, 9);
            Assert.True(range1.RangeCount==1);
            Assert.True(range1.ContainsRange(10, 20));

            // exact
            var range2 = ranges.Copy();
            range2.RemoveAllInRange(10,20);
            Assert.True(range2.RangeCount == 0);
            Assert.False(range2.ContainsRange(10, 20));

            // gap after
            var range3 = ranges.Copy();
            range3.RemoveAllInRange(21, 30);
            Assert.True(range3.RangeCount == 1);
            Assert.True(range3.ContainsRange(10, 20));

            // overlap before
            var range4 = ranges.Copy();
            range4.RemoveAllInRange(5, 15);
            Assert.True(range4.RangeCount == 1);
            Assert.True(range4.ContainsRange(16, 20));

            // overlap 
            var range5 = ranges.Copy();
            range5.RemoveAllInRange(12, 18);
            Assert.True(range5.RangeCount == 2);
            Assert.True(range5.ContainsRange(10, 11));
            Assert.True(range5.ContainsRange(19, 20));

            // overlap after
            var range6 = ranges.Copy();
            range6.RemoveAllInRange(15, 25);
            Assert.True(range6.RangeCount == 1);
            Assert.True(range6.ContainsRange(10, 14));

            ranges.RemoveAllInRange(10, 900);

            Assert.True(ranges.RangeCount == 0);

            var msg = string.Join(",", ranges.ToList());

            Console.WriteLine(msg);
        }

        [Fact]
        public void Test5()
        {
            var ranges = new PageRangeList();
            ranges.AddRange(10, 20);
            ranges.AddRange(30, 40);
            ranges.AddRange(50, 60);

            // gap before
            var range1 = ranges.Copy();
            range1.RemoveAllInRange(0, 9);
            Assert.True(range1.RangeCount == 3);
            Assert.True(range1.ContainsRange(10, 20));
            Assert.True(range1.ContainsRange(30, 40));
            Assert.True(range1.ContainsRange(50, 60));

            // exact
            var range2 = ranges.Copy();
            range2.RemoveAllInRange(30, 40);
            Assert.True(range2.RangeCount == 2);
            Assert.True(range2.ContainsRange(10, 20));
            Assert.False(range2.ContainsRange(30, 40));
            Assert.True(range2.ContainsRange(50, 60));

            // gap after
            var range3 = ranges.Copy();
            range3.RemoveAllInRange(61, 70);
            Assert.True(range3.RangeCount == 3);
            Assert.True(range3.ContainsRange(10, 20));
            Assert.True(range3.ContainsRange(30, 40));
            Assert.True(range3.ContainsRange(50, 60));

            // overlap before
            var range4 = ranges.Copy();
            range4.RemoveAllInRange(5, 15);
            Assert.True(range4.RangeCount == 3);
            Assert.True(range4.ContainsRange(16, 20));
            Assert.True(range4.ContainsRange(30, 40));
            Assert.True(range4.ContainsRange(50, 60));

            // overlap 
            var range5 = ranges.Copy();
            range5.RemoveAllInRange(15, 35);
            Assert.True(range5.RangeCount == 3);
            Assert.True(range5.ContainsRange(10, 14));
            Assert.True(range5.ContainsRange(36, 40));
            Assert.True(range5.ContainsRange(50, 60));

            // overlap after
            var range6 = ranges.Copy();
            range6.RemoveAllInRange(25, 45);
            Assert.True(range6.RangeCount == 2);
            Assert.True(range6.ContainsRange(10, 20));
            Assert.False(range6.ContainsRange(30, 40));
            Assert.True(range6.ContainsRange(50, 60));

            var msg = string.Join(",", ranges.ToList());

            Console.WriteLine(msg);
        }

        [Fact]
        public void Test6()
        {
            var ranges = new PageRangeList();

            ranges.AddRange(10, 20);



            Assert.Throws<ArgumentException>(() => ranges.AddRange(5, 10));

            //Assert.
            var msg = string.Join(",", ranges.ToList());

            Console.WriteLine(msg);
        }



        //private void PrintRanges(PageRangeListOld list)
        //{
        //    var items = list.Ranges.ToList();

        //    Console.WriteLine("PRL: {0}", string.Join("-", items));
        //}

        private void PrintRanges(PageRangeList list)
        {
            var items = list.ToList();

            Console.WriteLine("PRL2: {0}", string.Join("-", items));
        }

        private List<int> GetList(int count)
        {
            var ret = new List<int>();

            for (int i = 0; i < count; i++)
            {
                ret.Add(i);
            }

            return ret;
        }

        public void Dispose()
        {

        }
    }
}






