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

namespace KeyValium.Benchmarks
{
    [Config(typeof(Config))]
    [IterationCount(100)]
    [InvocationCount(100)]
    public class BenchmarkTemplate
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                //AddJob(Job.Dry);
                AddColumn(new CompressionRatioColumn());
            }
        }

        [ParamsAllValues]
        public CompressionAlgorithm CompAlg;

        [Params(1024 * 64)]
        public int BufferSize;

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
        public void Bench()
        {
        }
    }
}

