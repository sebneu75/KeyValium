using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using KeyValium.Benchmarks.Compression;
using KeyValium.Frontends.Serializers;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Serialization
{
    [IterationCount(100)]
    [InvocationCount(10000)]
    public class BenchSerialize
    {
        public TestObject TestObject = new TestObject();

        internal KvSerializer Serializer = new();

        [ParamsAllValues]
        public bool Zip;

        byte[] Serialized;

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
            Serialized = Serializer.Serialize(TestObject, Zip);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark]
        public void Serialization()
        {
            var serialized = Serializer.Serialize(TestObject, Zip);
        }

        [Benchmark]
        public void Deserialization()
        {
            var deserialized = Serializer.Deserialize<TestObject>(Serialized, Zip);
        }
    }
}

