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

    [IterationCount(20)]
    [InvocationCount(100000)]
    public class BenchMemMove
    {
        //[Params(256, 1024, 4096, 16384)] //, 65536, 1024 * 1024)]
        [Params(13, 129, 267, 1023, 3745, 8099)] //, 65536, 1024 * 1024)]
        //[Params(1, 47, 139, 277, 491, 1111, 8633, 16987, 55555)]
        public int Size;

        //[Params(-16, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 16)]
        [Params(-1, 0, 1)]
        public int Delta;

        private byte[] Buffer;

        private byte[] Result;

        private int SourceOffset;

        private int TargetOffset;

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
            var rnd = new Random();
            Buffer = new byte[Size * 16];
            rnd.NextBytes(Buffer);

            SourceOffset = (Buffer.Length - Size) / 2;
            TargetOffset = SourceOffset + Delta;

            Result = GetResult();
        }

        private byte[] GetResult()
        {
            var ret = Buffer.ToArray();

            if (Delta > 0)
            {
                // copy backward
                for (int i = Size - 1; i >= 0; i--)
                {
                    ret[i + TargetOffset] = ret[i + SourceOffset];
                }

            }
            else if (Delta < 0)
            {
                // copy forward
                for (int i = 0; i < Size; i++)
                {
                    ret[i + TargetOffset] = ret[i + SourceOffset];
                }
            }

            return ret;
        }

        [Conditional("DEBUG")]
        private void ValidateResult()
        {
            for (int i = 0; i < Buffer.Length; i++)
            {
                if (Result[i] != Buffer[i])
                {
                    throw new Exception("FAIL");
                }
            }
        }


        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [Benchmark(Baseline = true)]
        public unsafe void SpanCopyTo()
        {
            var source = new Span<byte>(Buffer, SourceOffset, Size);
            var target = new Span<byte>(Buffer, TargetOffset, Size);

            source.CopyTo(target);

            ValidateResult();
        }


        [Benchmark]
        public unsafe void BufferMemCopy()
        {
            fixed (byte* ptr = Buffer)
            {
                System.Buffer.MemoryCopy(ptr + SourceOffset, ptr + TargetOffset, Size, Size);
            }

            ValidateResult();
        }

        [Benchmark]
        public unsafe void BufferBlockCopy()
        {
            System.Buffer.BlockCopy(Buffer, SourceOffset, Buffer, TargetOffset, Size);

            ValidateResult();
        }

        [Benchmark]
        public unsafe void MemCpy()
        {
            fixed (byte* ptr = Buffer)
            {
                memcpy((IntPtr)(ptr + TargetOffset), (IntPtr)(ptr + SourceOffset), (UIntPtr)Size);
            }

            ValidateResult();
        }

        [Benchmark]
        public unsafe void MemMove()
        {
            fixed (byte* ptr = Buffer)
            {
                memmove((IntPtr)ptr + TargetOffset, (IntPtr)ptr + SourceOffset, (UIntPtr)Size);
            }

            ValidateResult();
        }

        [Benchmark]
        public unsafe void RtlMemMove()
        {
            fixed (byte* ptr = Buffer)
            {
                RtlMoveMemory((IntPtr)ptr + TargetOffset, (IntPtr)ptr + SourceOffset, Size);
            }

            ValidateResult();
        }

        [Benchmark]
        public unsafe void RtlMemCopy()
        {
            fixed (byte* ptr = Buffer)
            {
                RtlCopyMemory((IntPtr)ptr + TargetOffset, (IntPtr)ptr + SourceOffset, Size);
            }

            ValidateResult();
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static extern void RtlMoveMemory(IntPtr dest, IntPtr src, int size);

        [DllImport("Kernel32.dll", EntryPoint = "RtlCopyMemory", SetLastError = false)]
        static extern void RtlCopyMemory(IntPtr dest, IntPtr src, int size);

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

