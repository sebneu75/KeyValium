using BenchmarkDotNet.Attributes;
using KeyValium.Benchmarks.Compression;
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

namespace KeyValium.Benchmarks.Memory
{
    [IterationCount(100)]
    [InvocationCount(1)]
    public class BenchCollections
    {
        [Params(100, 1000, 10000, 100000)]
        public int Count;

        [Params(1, 2, 4, 8, 16, 32)]
        public int ParallelismDegree;

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

        [Benchmark(Baseline = true)]
        public void Stack()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = ParallelismDegree };

            var stack = new ConcurrentStack<int>();

            Parallel.For(0, Count, options, x => stack.Push(x));
            Parallel.For(0, Count, options, x => stack.TryPop(out var val));
        }

        [Benchmark()]
        public void Bag()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = ParallelismDegree };

            var bag = new ConcurrentBag<int>();

            Parallel.For(0, Count, options, x => bag.Add(x));
            Parallel.For(0, Count, options, x => bag.TryTake(out var val));
        }

        [Benchmark()]
        public void Queue()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = ParallelismDegree };

            var queue = new ConcurrentQueue<int>();

            Parallel.For(0, Count, options, x => queue.Enqueue(x));
            Parallel.For(0, Count, options, x => queue.TryDequeue(out var val));
        }
    }
}

