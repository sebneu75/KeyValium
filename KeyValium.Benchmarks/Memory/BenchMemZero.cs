using BenchmarkDotNet.Attributes;
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

    [IterationCount(10)]
    [InvocationCount(100000)]
    public class BenchMemZero
    {
        [Params(256, 1024, 4096, 16384, 65536)] //, 65536, 1024 * 1024)]
        //[Params(1, 47, 139, 277, 491, 1111, 8633, 16987, 55555)]
        public int Size;

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
            Target = new byte[Size];
        }

        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [Benchmark]
        public unsafe void Zero()
        {
            fixed (byte* ptr = Target)
            {
                //KeyValium.Memory.MemUtils.ZeroMemory(ptr, Size);
            }
        }

        [Benchmark]
        public unsafe void Zero256()
        {
            if ((Size & 255) != 0)
                return;

            fixed (byte* ptr = Target)
            {
                //KeyValium.Memory.MemUtils.ZeroPage256(ptr, Size);
            }
        }

        [Benchmark(Baseline = true)]
        public unsafe void SpanFill()
        {
            var span = new Span<byte>(Target);
            span.Fill(0);
        }

        [Benchmark]
        public unsafe void SpanClear()
        {
            var span = new Span<byte>(Target);
            span.Clear();
        }

        [Benchmark]
        public unsafe void ZeroMemSet()
        {
            fixed (byte* ptr = Target)
            {
                memset((IntPtr)ptr, 0, Size);
            }
        }

        [Benchmark]
        public unsafe void ArrayClear()
        {
            Array.Clear(Target);
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

