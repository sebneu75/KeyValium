using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;
using KeyValium;
using KeyValium.Benchmarks.Compression;
using KeyValium.Pages.Entries;
using KeyValium.Pages;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyValium.Memory;

namespace KeyValium.Benchmarks.Misc
{
    [IterationCount(10)]
    [InvocationCount(1)]
    public class BenchPage
    {
        const int KeyCount = 100;

        const int Iterations = 1000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _td = new TestDescription(nameof(BenchPage))
            {
                MinKeySize = 8,
                MaxKeySize = 8,
                MinValueSize = 16,
                MaxValueSize = 16,
                KeyCount = KeyCount,
                CommitSize = KeyCount,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending,
            };

            var alloc = new PageAllocator(_td.PageSize, false);
            _page = alloc.GetPage(100, true, PageTypes.DataLeaf, 0); ;
            //var page = new AnyPage(new RawPage(100, td.DbPageSize, true, PageTypes.DataLeaf, 0), PageStates.Dirty);
            _page.InitContentHeader(0);
            _leaf = _page.AsContentPage;

            _keys = _td.GenerateKeys(0, _td.KeyCount);
        }

        private TestDescription _td;

        AnyPage _page;

        ContentPage _leaf;

        List<KeyValuePair<byte[], byte[]>> _keys;

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [IterationSetup]
        public void IterationSetup()
        {
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark(OperationsPerInvoke = KeyCount * Iterations)]
        public void Ascending()
        {
            Run(_td, _leaf, _keys, KeyOrder.Ascending, KeyOrder.Descending);
        }

        [Benchmark(OperationsPerInvoke = KeyCount * Iterations)]
        public void Descending()
        {
            Run(_td, _leaf, _keys, KeyOrder.Descending, KeyOrder.Ascending);
        }

        [Benchmark(OperationsPerInvoke = KeyCount * Iterations)]
        public void Random()
        {
            Run(_td, _leaf, _keys, KeyOrder.Random, KeyOrder.Random);
        }

        private void Run(TestDescription td, ContentPage leaf, List<KeyValuePair<byte[], byte[]>> keys, KeyOrder insertorder, KeyOrder deleteorder)
        {
            //var toinsert = KeyValueGenerator.Order(keys, insertorder).Select(x =>
            //{
            //    var keyspan = new ReadOnlySpan<byte>(x.Key);
            //    return EntryExtern.CreateLeafEntry(keyspan, new ByteStream(x.Value), null, 0, 0);
            //}).ToList();

            //var todelete = KeyValueGenerator.Order(keys, deleteorder).Select(x =>
            //{
            //    var keyspan = new ReadOnlySpan<byte>(x.Key);
            //    return EntryExtern.CreateLeafEntry(keyspan, new ByteStream(x.Value), null, 0, 0);
            //}).ToList();

            //for (int k = 0; k < Iterations; k++)
            //{
            //    for (int i = 0; i < toinsert.Count; i++)
            //    {
            //        toinsert[i].Key.Seek(0, System.IO.SeekOrigin.Begin);
            //        toinsert[i].Value.Seek(0, System.IO.SeekOrigin.Begin);
            //    }

            //    // insert keys
            //    for (int i = 0; i < toinsert.Count; i++)
            //    {
            //        leaf.InsertEntry(toinsert[i]);
            //        //KvDebug.DumpPage(leaf, "After Insert ("+i.ToString()+")");
            //    }

            //    //KvDebug.DumpPage(leaf, "After Insert");

            //    // delete keys
            //    for (int i = 0; i < todelete.Count; i++)
            //    {
            //        leaf.DeleteEntry(todelete[i].Key);
            //    }

                //KvDebug.DumpPage(leaf, "After Delete");
            //}
        }
    }
}
