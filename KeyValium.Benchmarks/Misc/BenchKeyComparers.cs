using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using KeyValium;
using KeyValium.Benchmarks.Compression;
using KeyValium.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Benchmarks.Misc
{
    [IterationCount(10)]
    [InvocationCount(10)]
    public class BenchKeyComparers
    {
        const int Count = 100000;

        [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096)]
        public int KeyLength;

        [ParamsAllValues]
        public bool Random;

        private byte[][] Bytes;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Bytes = new byte[2][];

            for (int i = 0; i < Bytes.Length; i++)
            {
                Bytes[i] = new byte[KeyLength];
            }

            if (Random)
            {
                var rnd = new Random();

                rnd.NextBytes(Bytes[0]);
                rnd.NextBytes(Bytes[1]);
            }
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

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);


        [Benchmark(OperationsPerInvoke = Count)]
        public void MemCmp()
        {
            var len = new UIntPtr((uint)Math.Min(Bytes[0].Length, Bytes[1].Length));

            for (int i = 0; i < Count; i++)
            {
                memcmp(Bytes[0], Bytes[1], len);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public void Sequence()
        {
            for (int i = 0; i < Count; i++)
            {
                MemoryExtensions.SequenceCompareTo<byte>(Bytes[0], Bytes[1]);
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public unsafe void ByteWise()
        {
            var len = Math.Min(Bytes[0].Length, Bytes[1].Length);

            fixed (byte* p1 = Bytes[0])
            {
                var bp = new ByteSpan(p1, len);
                var span = Bytes[1].AsSpan();

                for (int i = 0; i < Count; i++)
                {
                    UniversalComparer.CompareBytesByteWise(bp, span);
                }
            }
        }

        [Benchmark(OperationsPerInvoke = Count)]
        public unsafe void UlongWise()
        {
            var len = Math.Min(Bytes[0].Length, Bytes[1].Length);

            fixed (byte* p1 = Bytes[0])
            {
                var bp = new ByteSpan(p1, len);
                var span = new ReadOnlySpan<byte>(Bytes[1]);

                for (int i = 0; i < Count; i++)
                {
                    UniversalComparer.CompareBytesUlongWise(bp, ref span);
                }
            }
        }
    }
}

