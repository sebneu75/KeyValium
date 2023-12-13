using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using KeyValium.Benchmarks.Compression;
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

    [IterationCount(20)]
    [InvocationCount(100000)]
    public class BenchMemCopy
    {
        [Params(256, 1024, 4096, 16384)] //, 65536, 1024 * 1024)]
        //[Params(1, 47, 139, 277, 491, 1111, 8633, 16987, 55555)]
        public int Size;

        private byte[] Source;

        private byte[] Target;

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
            Source = new byte[Size];
            Target = new byte[Size];
        }

        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [Benchmark(Baseline = true)]
        public unsafe void SpanCopyTo()
        {
            var source = new Span<byte>(Source);
            var target = new Span<byte>(Target);

            source.CopyTo(target);
        }

        [Benchmark]
        public unsafe void ArrayCopy()
        {
            Array.Copy(Source, Target, Size);
        }

        [Benchmark]
        public unsafe void UnsafeCopyBlock()
        {
            System.Runtime.CompilerServices.Unsafe.CopyBlock(ref Target[0], ref Source[0], (uint)Size);
        }

        [Benchmark]
        public unsafe void UnsafeCopyBlockUnaligned()
        {
            System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref Target[0], ref Source[0], (uint)Size);
        }

        [Benchmark]
        public unsafe void MarshalCopy()
        {
            fixed (byte* target = Target)
            {
                Marshal.Copy(Source, 0, (IntPtr)target, Size);
            }
        }

        [Benchmark]
        public unsafe void BufferMemoryCopy()
        {
            fixed (byte* source = Source)
            fixed (byte* target = Target)
            {
                System.Buffer.MemoryCopy(source, target, Size, Size);
            }
        }

        [Benchmark]
        public unsafe void BufferBlockCopy()
        {
            System.Buffer.BlockCopy(Source, 0, Target, 0, Size);
        }

        [Benchmark]
        public unsafe void NativeMemCpy()
        {
            fixed (byte* source = Source)
            fixed (byte* target = Target)
            {
                memcpy((IntPtr)(target), (IntPtr)(source), (UIntPtr)Size);
            }
        }

        [Benchmark]
        public unsafe void NativeMemMove()
        {
            fixed (byte* source = Source)
            fixed (byte* target = Target)
            {
                memmove((IntPtr)target, (IntPtr)source, (UIntPtr)Size);
            }
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memmove", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        static extern IntPtr memmove(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memset(IntPtr dest, int c, int byteCount);

        private static void BenchZero<T>(int sizet, byte[] target) where T : struct
        {
            //MemFill(target, 0xaa);
            //CheckMem(target, 0xaa);
            ZeroBuffer<T>(sizet, target);
            //CheckMem(target, 0x00);
        }

        private static void BenchCopy<T>(int sizet, byte[] target, byte[] source) where T : struct
        {
            //MemFill(source, 0xaa);
            //CheckMem(source, 0xaa);
            //MemFill(target, 0x00);
            //CheckMem(target, 0x00);

            CopyBuffer<T>(sizet, target, source);
            //CheckMem(target, 0xaa);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void ZeroBuffer<T>(int size, byte[] bytes) where T : struct
        {
            var count = bytes.Length / size;
            fixed (byte* ptr = bytes)
            {
                var target = (T*)ptr;
                var e = target + count;

                while (target < e)
                {
                    *(target++) = default;
                    //count--;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void CopyBuffer<T>(int size, byte[] target, byte[] source) where T : struct
        {
            var count = source.Length / size;
            fixed (byte* src = source)
            fixed (byte* trg = target)
            {
                var s = (T*)src;
                var t = (T*)trg;
                var e = s + count;
                while (s < e)
                {
                    *(t++) = *(s++);
                    //count--;
                }
            }
        }

        private static void CheckMem(byte[] target, byte val)
        {
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] != val)
                {
                    throw new Exception("Memory corrupted!");
                }
            }
        }

        private static void MemFill(byte[] target, byte val)
        {
            FillByte(target, val);
        }

        public static unsafe void FillByte(byte[] bytes, byte val = 0)
        {
            var count = bytes.Length / sizeof(byte);
            fixed (byte* ptr = bytes)
            {
                var target = (byte*)ptr;
                while (count > 0)
                {
                    *(target++) = val;
                    count--;
                }
            }
        }
    }
}
