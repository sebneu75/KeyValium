using BenchmarkDotNet.Attributes;
using KeyValium.Cursors;
using KeyValium.Memory;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Threading
{
    [IterationCount(100)]
    [InvocationCount(10000)]
    public class BenchPrimitives
    {
        [Params(64, 256, 1024, 4096, 16384, 65536)]
        public int Size;

        byte[] Bytes;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Bytes = new byte[Size];
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark]
        public unsafe void Pointer()
        {
            fixed (byte* p = Bytes)
            {
                var s = Size >> 3;

                var ptr = (ulong*)p;
                for (int i = 0; i < s; i++)
                {
                    *ptr = 0;
                    ptr++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        [Benchmark]
        public unsafe void Unsafexx()
        {
            var s = Size >> 3;
            var span = new Span<byte>(Bytes);

            for (int i = 0; i < s; i++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(i << 3, 8), 0);
            }
        }
    }
}
