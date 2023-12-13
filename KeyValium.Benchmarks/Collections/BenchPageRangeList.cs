using BenchmarkDotNet.Attributes;
using KeyValium.Benchmarks.Compression;
using KeyValium.Collections;
using KeyValium.TestBench;
using Perfolizer.Mathematics.Randomization;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Benchmarks.Collections
{
    [IterationCount(1)]
    [InvocationCount(1)]
    public class BenchPageRangeList
    {
        [Params(10, 100, 1000)] //, 10000, 100000)]
        public int Count;

        Random _rnd;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _rnd = new Random();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [IterationSetup]
        public void IterationSetup()
        {
            var list = GetRandomRangeList(Count);

            _rangesasc = list.OrderBy(x => x.First).ToArray();
            _rangesdesc = list.OrderByDescending(x => x.First).ToArray();
            _rangesrandom = KeyValueGenerator.Shuffle(_rnd, list).ToArray();
        }

        private List<PageRange> GetRandomRangeList(int count)
        {
            var ret = new List<PageRange>();

            var pageno = (ulong)_rnd.Next(1000);

            for (int i = 0; i < count; i++)
            {
                var len = (ulong)_rnd.Next(100);

                ret.Add(new PageRange(pageno, pageno + len));

                pageno += len+ (ulong)_rnd.Next(10, 100);
            }

            return ret;
        }

        private PageRange[] _rangesrandom;

        private PageRange[] _rangesasc;

        private PageRange[] _rangesdesc;

        private PageRangeList _ranges;

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark(Baseline = true)]
        public void InsDelRandom()
        {
            var list = new PageRangeList();

            for (int i = 0; i < _rangesrandom.Length; i++)
            {
                ref var item = ref _rangesrandom[i];
                list.AddRange(item);
            }

            for (int i = 0; i < _rangesrandom.Length; i++)
            {
                ref var item = ref _rangesrandom[i];
                list.RemoveRange(item);
            }
        }

        [Benchmark]
        public void InsDelAsc()
        {
            var list = new PageRangeList();

            for (int i = 0; i < _rangesasc.Length; i++)
            {
                ref var item = ref _rangesasc[i];
                list.AddRange(item);
            }

            for (int i = 0; i < _rangesasc.Length; i++)
            {
                ref var item = ref _rangesasc[i];
                list.RemoveRange(item);
            }

        }

        [Benchmark]
        public void InsDelDesc()
        {
            var list = new PageRangeList();

            for (int i = 0; i < _rangesdesc.Length; i++)
            {
                ref var item = ref _rangesdesc[i];
                list.AddRange(item);
            }

            for (int i = 0; i < _rangesdesc.Length; i++)
            {
                ref var item = ref _rangesdesc[i];
                list.RemoveRange(item);
            }
        }

        //[Benchmark]
        //public void OldInsDelRandom()
        //{
        //    var list = new PageRangeListOld();

        //    for (int i = 0; i < _rangesrandom.Length; i++)
        //    {
        //        ref var item = ref _rangesrandom[i];
        //        list.AddRange(item);
        //    }

        //    for (int i = 0; i < _rangesrandom.Length; i++)
        //    {
        //        ref var item = ref _rangesrandom[i];
        //        list.RemoveRange(item);
        //    }
        //}

        //[Benchmark]
        //public void OldInsDelAsc()
        //{
        //    var list = new PageRangeListOld();

        //    for (int i = 0; i < _rangesasc.Length; i++)
        //    {
        //        ref var item = ref _rangesasc[i];
        //        list.AddRange(item);
        //    }

        //    for (int i = 0; i < _rangesasc.Length; i++)
        //    {
        //        ref var item = ref _rangesasc[i];
        //        list.RemoveRange(item);
        //    }
        //}

        //[Benchmark]
        //public void OldInsDelDesc()
        //{
        //    var list = new PageRangeListOld();

        //    for (int i = 0; i < _rangesdesc.Length; i++)
        //    {
        //        ref var item = ref _rangesdesc[i];
        //        list.AddRange(item);
        //    }

        //    for (int i = 0; i < _rangesdesc.Length; i++)
        //    {
        //        ref var item = ref _rangesdesc[i];
        //        list.RemoveRange(item);
        //    }
        //}

        [Benchmark]
        public void SetInsDelRandom()
        {
            var list = new SortedSet<PageRange>(new Comparer());

            for (int i = 0; i < _rangesrandom.Length; i++)
            {
                ref var item = ref _rangesrandom[i];
                list.Add(item);
            }

            for (int i = 0; i < _rangesrandom.Length; i++)
            {
                ref var item = ref _rangesrandom[i];
                list.Remove(item);
            }
        }

        [Benchmark]
        public void SetInsDelAsc()
        {
            var list = new SortedSet<PageRange>(new Comparer());

            for (int i = 0; i < _rangesasc.Length; i++)
            {
                ref var item = ref _rangesasc[i];
                list.Add(item);
            }

            for (int i = 0; i < _rangesasc.Length; i++)
            {
                ref var item = ref _rangesasc[i];
                list.Remove(item);
            }
        }

        [Benchmark]
        public void SetInsDelDesc()
        {
            var list = new SortedSet<PageRange>(new Comparer());
                        
            for (int i = 0; i < _rangesdesc.Length; i++)
            {
                ref var item = ref _rangesdesc[i];
                list.Add(item);
            }

            for (int i = 0; i < _rangesdesc.Length; i++)
            {
                ref var item = ref _rangesdesc[i];
                list.Remove(item);
            }
        }


        //[Benchmark]
        //public void RbInsertRandom()
        //{
        //    var list = new RedBlackTree();

        //    for (int i = 0; i < _rangesrandom.Length; i++)
        //    {
        //        ref var item = ref _rangesrandom[i];
        //        list.Insert(item);
        //    }
        //}

        //[Benchmark]
        //public void RbInsertAsc()
        //{
        //    var list = new RedBlackTree();

        //    for (int i = 0; i < _rangesasc.Length; i++)
        //    {
        //        ref var item = ref _rangesasc[i];
        //        list.Insert(item);
        //    }
        //}

        //[Benchmark]
        //public void RbInsertDesc()
        //{
        //    var list = new RedBlackTree();

        //    for (int i = 0; i < _rangesdesc.Length; i++)
        //    {
        //        ref var item = ref _rangesdesc[i];
        //        list.Insert(item);
        //    }
        //}

        private class Comparer : IComparer<PageRange>
        {
            public int Compare(PageRange x, PageRange y)
            {
                if (x.First == y.First)
                {
                    return 0;
                }

                return x.First < y.First ? -1 : +1;
            }
        }
    }
}

