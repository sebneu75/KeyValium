using BenchmarkDotNet.Attributes;
using KeyValium.Benchmarks.Compression;
using KeyValium.Cursors;
using KeyValium.Memory;
using System;
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

namespace KeyValium.Benchmarks.Memory
{

    [IterationCount(100)]
    [InvocationCount(100000)]
    public class BenchInterlocked
    {
        [Params(100, 1000, 10000, 100000, 1000000)]
        public int Count;

        int x = 0;

        [GlobalSetup]
        public void GlobalSetup()
        {
        }

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

        [Benchmark]
        public unsafe void ILIncrement()
        {
            Interlocked.Increment(ref x);
        }

        [Benchmark(Baseline = true)]
        public unsafe void Increment()
        {
            x++;
        }
    }
}

