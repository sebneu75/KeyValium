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
using System.Xml.Linq;

namespace KeyValium.Benchmarks.Misc
{
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(10)]
    public class BenchInsert
    {
        const int KeyCount = 100000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var td = new TestDescription(nameof(BenchInsert))
            {
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 128,
                MaxValueSize = 128,
                KeyCount = KeyCount,
                CommitSize = KeyCount,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Descending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending,
                OrderUpdate = KeyOrder.Ascending,
            };

            _pdb = new PreparedKeyValium(td);
        }

        PreparedDatabase _pdb;

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _pdb.PrepareInsert();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _pdb.FinishInsert();
        }

        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Insert()
        {
            _pdb.Insert();
        }
    }
}


