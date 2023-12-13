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

    [IterationCount(100)]
    [InvocationCount(1000)]
    public class BenchMemZeroCopy
    {
        [Params(256, 1024, 4096)] //, 65536, 1024 * 1024)]
        //[Params(1, 47, 139, 277, 491, 1111, 8633, 16987, 55555)]
        public int Size;

        private byte[] Buffer;

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
            Buffer = new byte[16 * 1024 * 1024];
            Source = new byte[Size];
            Target = new byte[Size];
        }

        [IterationCleanup]
        public void IterationCleanup()
        {

        }

        [Benchmark]
        public unsafe void Zero256()
        {
            if ((Size & 255) != 0)
                return;

            fixed (byte* ptr = Buffer)
            {
                //KeyValium.Memory.MemUtils.ZeroPage256(ptr, Size);
            }
        }

        [Benchmark]
        public unsafe void Zero()
        {
            fixed (byte* ptr = Buffer)
            {
                //KeyValium.Memory.MemUtils.ZeroMemory(ptr, Size);
            }
        }

        [Benchmark]
        public unsafe void Copy256()
        {
            if ((Size & 255) != 0)
                return;

            fixed (byte* ptr = Buffer)
            {
                //KeyValium.Memory.MemUtils.CopyPage256(ptr, ptr + Size, Size);
            }
        }

        [Benchmark]
        public unsafe void CopyForward()
        {
            fixed (byte* ptr = Buffer)
            {
                //KeyValium.Memory.MemUtils.MoveMemoryForward(ptr + Size / 2, ptr, Size);
            }
        }

        [Benchmark]
        public unsafe void CopyBackward()
        {
            fixed (byte* ptr = Buffer)
            {
                //KeyValium.Memory.MemUtils.MoveMemoryBackward(ptr, ptr + Size / 2, Size);
            }
        }

        [Benchmark]
        public unsafe void BufMemCopyForward()
        {
            fixed (byte* ptr = Buffer)
            {
                System.Buffer.MemoryCopy(ptr + Size / 2, ptr, Size, Size);
            }
        }

        [Benchmark]
        public unsafe void BufMemCopyBackward()
        {
            fixed (byte* ptr = Buffer)
            {
                System.Buffer.MemoryCopy(ptr, ptr + Size / 2, Size, Size);
            }
        }

        [Benchmark]
        public unsafe void BlockCopy()
        {
            System.Buffer.BlockCopy(Buffer, 0, Buffer, Size / 2, Size);
        }

        [Benchmark]
        public unsafe void MemSet()
        {
            fixed (byte* ptr = Buffer)
            {
                memset((IntPtr)ptr, 0, Size);
            }
        }

        [Benchmark]
        public unsafe void MemCpy()
        {
            fixed (byte* ptr = Buffer)
            {
                memcpy((IntPtr)ptr + Size / 2, (IntPtr)ptr, (UIntPtr)Size);
            }
        }

        [Benchmark]
        public unsafe void MemMove()
        {
            fixed (byte* ptr = Buffer)
            {
                memmove((IntPtr)ptr + Size / 2, (IntPtr)ptr, (UIntPtr)Size);
            }
        }

        [Benchmark]
        public unsafe void Zero_1()
        {
            BenchZero<byte>(sizeof(byte), Target);
        }

        [Benchmark]
        public unsafe void Zero_2()
        {
            BenchZero<short>(sizeof(short), Target);
        }

        [Benchmark]
        public unsafe void Zero_4()
        {
            BenchZero<int>(sizeof(int), Target);
        }
        [Benchmark]
        public unsafe void Zero_8()
        {
            BenchZero<long>(sizeof(long), Target);
        }
        [Benchmark]
        public unsafe void Zero_16()
        {
            BenchZero<Block16>(sizeof(Block16), Target);

        }
        [Benchmark]
        public unsafe void Zero_32()
        {
            BenchZero<Block32>(sizeof(Block32), Target);

        }
        [Benchmark]
        public unsafe void Zero_64()
        {
            BenchZero<Block64>(sizeof(Block64), Target);

        }
        [Benchmark]
        public unsafe void Zero_128()
        {
            BenchZero<Block128>(sizeof(Block128), Target);

        }
        [Benchmark]
        public unsafe void Zero_256()
        {
            BenchZero<Block256>(sizeof(Block256), Target);

        }
        [Benchmark]
        public unsafe void Zero_512()
        {
            BenchZero<Block512>(sizeof(Block512), Target);

        }
        [Benchmark]
        public unsafe void Zero_1K()
        {
            BenchZero<Block1k>(sizeof(Block1k), Target);

        }
        [Benchmark]
        public unsafe void Zero_2K()
        {
            BenchZero<Block2k>(sizeof(Block2k), Target);

        }
        [Benchmark]
        public unsafe void Zero_4K()
        {
            BenchZero<Block4k>(sizeof(Block4k), Target);

        }
        [Benchmark]
        public unsafe void Zero_8K()
        {
            BenchZero<Block8k>(sizeof(Block8k), Target);

        }
        [Benchmark]
        public unsafe void Zero_16K()
        {
            BenchZero<Block16k>(sizeof(Block16k), Target);

        }
        [Benchmark]
        public unsafe void Zero_32K()
        {
            BenchZero<Block32k>(sizeof(Block32k), Target);

        }
        [Benchmark]
        public unsafe void Zero_64K()
        {
            BenchZero<Block64k>(sizeof(Block64k), Target);

        }
        [Benchmark]
        public unsafe void Zero_128K()
        {
            BenchZero<Block128k>(sizeof(Block128k), Target);

        }
        [Benchmark]
        public unsafe void Zero_256K()
        {
            BenchZero<Block256k>(sizeof(Block256k), Target);

        }

        [Benchmark]
        public unsafe void Copy_1()
        {
            BenchCopy<byte>(sizeof(byte), Target, Source);
        }
        [Benchmark]
        public unsafe void Copy_2()
        {
            BenchCopy<short>(sizeof(short), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_4()
        {
            BenchCopy<int>(sizeof(int), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_8()
        {
            BenchCopy<long>(sizeof(long), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_16()
        {
            BenchCopy<Block16>(sizeof(Block16), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_32()
        {
            BenchCopy<Block32>(sizeof(Block32), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_64()
        {
            BenchCopy<Block64>(sizeof(Block64), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_128()
        {
            BenchCopy<Block128>(sizeof(Block128), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_256()
        {
            BenchCopy<Block256>(sizeof(Block256), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_512()
        {
            BenchCopy<Block512>(sizeof(Block512), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_1K()
        {
            BenchCopy<Block1k>(sizeof(Block1k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_2K()
        {
            BenchCopy<Block2k>(sizeof(Block2k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_4K()
        {
            BenchCopy<Block4k>(sizeof(Block4k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_8K()
        {
            BenchCopy<Block8k>(sizeof(Block8k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_16K()
        {
            BenchCopy<Block16k>(sizeof(Block16k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_32K()
        {
            BenchCopy<Block32k>(sizeof(Block32k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_64K()
        {
            BenchCopy<Block64k>(sizeof(Block64k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_128K()
        {
            BenchCopy<Block128k>(sizeof(Block128k), Target, Source);

        }
        [Benchmark]
        public unsafe void Copy_256K()
        {
            BenchCopy<Block256k>(sizeof(Block256k), Target, Source);

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

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    struct Block16
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    struct Block32
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 64)]
    struct Block64
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 128)]
    struct Block128
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 256)]
    struct Block256
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 512)]
    struct Block512
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 1024)]
    struct Block1k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 2048)]
    struct Block2k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 4096)]
    struct Block4k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 8192)]
    struct Block8k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 16384)]
    struct Block16k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 32768)]
    struct Block32k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 65536)]
    struct Block64k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 131072)]
    struct Block128k
    {
    }

    [StructLayout(LayoutKind.Sequential, Size = 262144)]
    struct Block256k
    {
    }
}

