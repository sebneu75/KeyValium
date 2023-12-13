using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Parameters;
using KeyValium.Benchmarks.Compression;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Misc
{
    [IterationCount(10)]
    [InvocationCount(100)]
    public class BenchLocks
    {
        const int Count = 1000000;

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

        private object _lock = new object();

        [Benchmark(OperationsPerInvoke = Count)]
        public void Locked()
        {
            ulong val = 0;
            for (int i = 0; i < Count; i++)
            {
                lock (_lock)
                {
                    Inc(ref val);
                }
            }
        }

        private ReaderWriterLockSlim _rwlock = new();

        [Benchmark(OperationsPerInvoke = Count)]
        public void RwLocked()
        {
            ulong val = 0;
            for (int i = 0; i < Count; i++)
            {
                _rwlock.EnterReadLock();
                {
                    Inc(ref val);
                }
                _rwlock.ExitReadLock();
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void Unlocked()
        {
            ulong val = 0;
            for (int i = 0; i < Count; i++)
            {
                Inc(ref val);
            }
        }

        private void Inc(ref ulong val)
        {
            //var x = new object();
            //var h = x.GetHashCode();
            val++;
        }
    }
}

