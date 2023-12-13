using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Compression
{
    [Config(typeof(Config))]
    [IterationCount(1)]
    [InvocationCount(100)]
    public class BenchCompression
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

        public int x;

        [Params(1024 * 16)]
        public int RndCount;

        [Params(CompressionLevel.Fastest, CompressionLevel.Optimal, CompressionLevel.SmallestSize)]
        public CompressionLevel Level;

        private byte[] UncompressedBuffer;

        private byte[] CompressedBuffer;

        [GlobalSetup]
        public void GlobalSetup()
        {
        }

        [IterationSetup]
        public void IterationSetup()
        {
            UncompressedBuffer = GetBuffer(BufferSize, RndCount);

            CompressedBuffer = CompressInternal(CompAlg, Level, UncompressedBuffer);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
        }

        [Benchmark]
        public void Compress()
        {
            CompressInternal(CompAlg, Level, UncompressedBuffer);
        }

        [Benchmark]
        public void Decompress()
        {
            DecompressInternal(CompAlg, CompressedBuffer);
        }

        private static byte[] CompressInternal(CompressionAlgorithm alg, CompressionLevel level, byte[] buffer)
        {
            using (var msin = new MemoryStream(buffer, false))
            {
                using (var msout = new MemoryStream())
                {
                    using (var comp = GetCompressionStream(alg, level, msout))
                    {
                        msin.CopyTo(comp);
                    }

                    return msout.ToArray();
                }
            }
        }

        private byte[] DecompressInternal(CompressionAlgorithm alg, byte[] buffer)
        {
            using (var msin = new MemoryStream(buffer))
            {
                using (var msout = new MemoryStream())
                {
                    using (var comp = GetDecompressionStream(alg, msin))
                    {
                        comp.CopyTo(msout);
                    }

                    return msout.ToArray();
                }
            }
        }

        private static Stream GetCompressionStream(CompressionAlgorithm alg, CompressionLevel level, MemoryStream msout)
        {
            switch (alg)
            {
                case CompressionAlgorithm.Brotli:
                    return new BrotliStream(msout, level, true);

                case CompressionAlgorithm.Deflate:
                    return new DeflateStream(msout, level, true);

                case CompressionAlgorithm.GZip:
                    return new GZipStream(msout, level, true);

                case CompressionAlgorithm.ZLib:
                    return new ZLibStream(msout, level, true);
            }

            return null;
        }

        private static Stream GetDecompressionStream(CompressionAlgorithm alg, MemoryStream msout)
        {
            switch (alg)
            {
                case CompressionAlgorithm.Brotli:
                    return new BrotliStream(msout, CompressionMode.Decompress, true);

                case CompressionAlgorithm.Deflate:
                    return new DeflateStream(msout, CompressionMode.Decompress, true);

                case CompressionAlgorithm.GZip:
                    return new GZipStream(msout, CompressionMode.Decompress, true);

                case CompressionAlgorithm.ZLib:
                    return new ZLibStream(msout, CompressionMode.Decompress, true);
            }

            return null;
        }



        //private void Run()
        //{
        //    var sw = new Stopwatch();

        //    sw.Restart();
        //    var compressed = Compress(alg, buffer, 0);
        //    sw.Stop();

        //    var ct = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000000.0;

        //    sw.Restart();
        //    var decompressed = Decompress(alg, compressed, 0);
        //    sw.Stop();

        //    var dt = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000000.0;

        //    var tt = ct + dt;

        //    var result = MemoryExtensions.SequenceCompareTo<byte>(buffer, decompressed);

        //    var ratio = (double)compressed.Length / (double)buffer.Length;

        //    var msg = string.Format("{0}: {1} {2} {3} {4:#0.0%} {5:#0.0}µs {6:#0.0}µs {7:#0.0}µs",
        //              alg, result == 0, buffer.Length, compressed.Length, ratio, ct, dt, tt);

        //    Console.WriteLine(msg);

        //    if (result != 0)
        //    {
        //        Console.WriteLine("FAIL.");
        //    }
        //}

        private static byte[] GetBuffer(int length, int rndcount)
        {
            var buffer = new byte[length];

            var rnd = new Random();
            for (int i = 0; i < rndcount; i++)
            {
                var pos = rnd.Next(buffer.Length);
                var val = rnd.Next(256);

                buffer[i] = (byte)val;
            }

            return buffer;
        }

        private static byte[] _buffer;

        internal static double GetCompressionRatio(ParameterInstances parameters)
        {
            var bufsize = (int)parameters.Items.First(x => x.Name == nameof(BufferSize)).Value;
            var rndcount = (int)parameters.Items.First(x => x.Name == nameof(RndCount)).Value;
            var level = (CompressionLevel)parameters.Items.First(x => x.Name == nameof(Level)).Value;
            var alg = (CompressionAlgorithm)parameters.Items.First(x => x.Name == nameof(CompAlg)).Value;

            if (_buffer == null || _buffer.Length != bufsize)
            {
                _buffer = GetBuffer(bufsize, rndcount);
            }

            var compressed = CompressInternal(alg, level, _buffer);


            return (double)compressed.Length / _buffer.Length;
        }
    }
}

/*

namespace Test.Benchmarks
{
    internal class BenchCompression : BenchmarkBase
    {
        public override IEnumerable<TestDescription> GetItems()
        {
            var td = new TestDescription("Benchmark Compression");
            td.MinKeySize = 8;
            td.MaxKeySize = 8;
            td.MinValueSize = 16;
            td.MaxValueSize = 16;
            td.KeyCount = 100;
            td.CommitSize = 100;
            td.GenStrategy = Util.KeyGenStrategy.Sequential;
            td.OrderInsert = Util.KeyOrder.Ascending;
            td.OrderRead = Util.KeyOrder.Ascending;
            td.OrderDelete = Util.KeyOrder.Ascending;

            yield return td;
        }


        public override string Name => "BenchCompression";

        internal override void RunItem(TestDescription td)
        {
            RunSerialization();

            var bufsize = 1024 * 64;
            var rndcount = bufsize / 4;

            var buffer = GetBuffer(bufsize, rndcount);

            Run(CompressionAlgorithm.Brotli, buffer);
            Run(CompressionAlgorithm.Deflate, buffer);
            Run(CompressionAlgorithm.GZip, buffer);
            Run(CompressionAlgorithm.ZLib, buffer);

            Console.WriteLine("-----------------------");
        }

        private void RunSerialization()
        {
            var sw = new Stopwatch();

            var sd = new KvSerializer();
            var item = new TestObject();

            for (int i = 0; i < 10; i++)
            {
                sw.Restart();
                var serialized = sd.Serialize(item, false);
                sw.Stop();

                var st = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000000.0;

                sw.Restart();
                var deserialized = sd.Deserialize<TestObject>(serialized, false);
                sw.Stop();

                var dt = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000000.0;

                var tt = st + dt;

                //var result = MemoryExtensions.SequenceCompareTo<byte>(buffer, decompressed);

                var msg = string.Format("Serialization: {0:#0.0}µs    Deserialzation: {1:#0.0}µs    Total: {2:#0.0}µs",
                                        st, dt, tt);
                Console.WriteLine(msg);
            }
        }


        private byte[] Decompress(CompressionAlgorithm alg, byte[] buffer, int v)
        {
            using (var msin = new MemoryStream(buffer))
            {
                using (var msout = new MemoryStream())
                {
                    using (var comp = GetDecompressionStream(alg, msin))
                    {
                        comp.CopyTo(msout);
                    }

                    return msout.ToArray();
                }
            }
        }





        private void RunOld(TestDescription td, ContentPage leaf, List<KeyValuePair<byte[], byte[]>> keys, KeyOrder insertorder, KeyOrder deleteorder)
        {
            var iterations = 1000;

            var toinsert = KeyValueGenerator.Order(keys, insertorder).Select(x => EntryExtern.CreateLeafEntry(new ByteStream(x.Key), new ByteStream(x.Value), null, 0, 0)).ToList();
            var todelete = KeyValueGenerator.Order(keys, deleteorder).Select(x => EntryExtern.CreateLeafEntry(new ByteStream(x.Key), new ByteStream(x.Value), null, 0, 0)).ToList();

            var totalcount = keys.Count * iterations;

            var title = string.Format("Insert {0} - Delete {1}", insertorder, deleteorder);

            var m1 = td.Measure.MeasureTime(title, 0, totalcount, () =>
            {
                for (int k = 0; k < iterations; k++)
                {
                    // insert keys
                    for (int i = 0; i < toinsert.Count; i++)
                    {
                        leaf.InsertEntry(toinsert[i]);
                    }

                    // delete keys
                    for (int i = 0; i < todelete.Count; i++)
                    {
                        leaf.DeleteEntry(todelete[i].Key);
                    }
                }
            });

            Console.WriteLine(m1);
        }
    }
}
*/