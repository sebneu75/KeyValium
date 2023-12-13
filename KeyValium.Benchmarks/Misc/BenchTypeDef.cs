using BenchmarkDotNet.Running;
using KeyValium;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using KeyValium.Benchmarks.Compression;

namespace KeyValium.Benchmarks.Misc
{
    [IterationCount(100)]
    [InvocationCount(100)]
    public class BenchTypeDef
    {
        const int Count = 10000000;

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


        [Benchmark(OperationsPerInvoke = Count)]
        public void BenchInt16()
        {
            for (int i = 0; i < Count; i++)
            {
                var x = Add((short)1, (short)1);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void BenchInt32()
        {
            for (int i = 0; i < Count; i++)
            {
                var x = Add(1, 1);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void BenchInt64()
        {
            for (int i = 0; i < Count; i++)
            {
                var x = Add((long)1, (long)1);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void BenchTestType()
        {
            for (int i = 0; i < Count; i++)
            {
                var x = Add(new TestType(1), new TestType(1));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private TestType Add(TestType t1, TestType t2)
        {
            return t1 + t2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int Add(int t1, int t2)
        {
            return t1 + t2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private long Add(long t1, long t2)
        {
            return t1 + t2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private short Add(short t1, short t2)
        {
            return (Int16)(t1 + t2);
        }
    }
}

