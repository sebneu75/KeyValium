using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;
using KeyValium.Benchmarks.Compression;
using KeyValium.Cache;
using KeyValium.Collections;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Collections
{
    [IterationCount(10)]
    [InvocationCount(1)]
    public class BenchHashSet
    {
        const int Count = 1000000;

        private HashSet<ulong> hash;
        private KvHashSet kvhash;
        private Dictionary<ulong, object> dict;
        private SortedDictionary<ulong, object> sdict;
        private PageRangeList rlist;
        private LruCache lru;

        private HashSet<ulong> hash2;
        private KvHashSet kvhash2;
        private Dictionary<ulong, object> dict2;
        private SortedDictionary<ulong, object> sdict2;
        private PageRangeList rlist2;
        private LruCache lru2;

        [GlobalSetup]
        public void GlobalSetup()
        {
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [IterationSetup()]
        public void IterationSetup()
        {
            hash = new HashSet<ulong>();
            kvhash = new KvHashSet();
            dict = new Dictionary<ulong, object>();
            sdict = new SortedDictionary<ulong, object>();
            rlist = new PageRangeList();
            lru = new LruCache(1000000);

            hash2 = new HashSet<ulong>();
            kvhash2 = new KvHashSet();
            dict2 = new Dictionary<ulong, object>();
            sdict2 = new SortedDictionary<ulong, object>();
            rlist2 = new PageRangeList();
            lru2 = new LruCache(1000000);

            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                hash2.Add(pageno);
                kvhash2.Add(pageno);
                dict2.Add(pageno, null);
                sdict2.Add(pageno, null);
                rlist2.AddPage(pageno);

                var pr = new PageRef(pageno, null, pageno);
                lru2.UpsertPage(ref pr);
            }
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                hash.Add(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddKvHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                kvhash.Add(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                dict.Add(pageno, null);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddSDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                sdict.Add(pageno, null);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddRangeList()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                rlist.AddPage(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void AddLru()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                var pr = new PageRef(pageno, null, pageno);
                lru.UpsertPage(ref pr);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void ContainsHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                hash2.Contains(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void ContainsKvHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                kvhash2.Contains(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void ContainsDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                dict2.ContainsKey(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void ContainsSDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                sdict2.ContainsKey(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void ContainsRangeList()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                rlist2.Contains(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void TryGetHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                hash2.TryGetValue(pageno, out var _);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void TryGetKvHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                kvhash2.Contains(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void TryGetDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                dict2.TryGetValue(pageno, out var _);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void TryGetSDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                sdict2.TryGetValue(pageno, out var _);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void TryGetLru()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                lru2.GetPage(pageno, out var _);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                hash2.Remove(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteKvHashSet()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                kvhash2.Remove(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                dict2.Remove(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteSDictionary()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                sdict2.Remove(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteRangeList()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                rlist2.RemovePage(pageno);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void DeleteLru()
        {
            for (ulong pageno = 0; pageno < Count; pageno++)
            {
                lru2.RemovePage(pageno);
            }
        }
    }
}
