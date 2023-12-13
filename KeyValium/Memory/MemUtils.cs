using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Memory
{
    internal unsafe class MemUtils
    {
        //static MemUtils()
        //{
        //    Perf.CallCount();

        //    AtomSize = GetAtomSize();
        //    CopyDirection = GetCopyDirection(AtomSize);

        //    if (CopyDirection && AtomSize == 16)
        //    {
        //        // TODO
        //        MoveMemory = MoveMemoryS;
        //    }
        //    else
        //    {
        //        MoveMemory = MoveMemoryS;
        //    }
        //}

        //internal readonly static int AtomSize;

        ///// <summary>
        ///// true: Forward
        ///// false: Backward
        ///// </summary>
        //internal readonly static bool CopyDirection;

        #region Block definitions

        //[StructLayout(LayoutKind.Sequential, Size = 1)]
        //private struct Block1
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 2)]
        //private struct Block2
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 4)]
        //private struct Block4
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 8)]
        //private struct Block8
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 16)]
        //private struct Block16
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 32)]
        //private struct Block32
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 64)]
        //private struct Block64
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 256)]
        //private struct Block256
        //{
        //}

        //[StructLayout(LayoutKind.Sequential, Size = 4096)]
        //private struct Block4k
        //{
        //}

        #endregion

        #region generic static functions

        //private static bool IsMultiple(int size)
        //{
        //    Perf.CallCount();

        //    return ((size > 0) && (size & (Limits.MinPageSize - 1)) == 0);
        //}

        ///// <summary>
        ///// zeroes a page
        ///// </summary>
        ///// <param name="pointer">pointer to page</param>
        ///// <param name="size">must be a multiple of 512</param>
        //internal static unsafe void ZeroPage256(byte* pointer, int size)
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(IsMultiple(size), "Size must be a multiple of MinPageSize!");

        //    while (size >= sizeof(Block4k))
        //    {
        //        *(Block4k*)pointer = default;
        //        pointer += sizeof(Block4k);
        //        size -= sizeof(Block4k);
        //    }

        //    while (size >= sizeof(Block256))
        //    {
        //        *(Block256*)pointer = default;
        //        pointer += sizeof(Block256);
        //        size -= sizeof(Block256);
        //    }

        //    KvDebug.Assert(size == 0, "ZeroPage256 Failed!");
        //}

        //internal static void ZeroMemory(byte* pointer, int size)
        //{
        //    Perf.CallCount();

        //    while (size >= sizeof(Block4k))
        //    {
        //        *(Block4k*)pointer = default;
        //        pointer += sizeof(Block4k);
        //        size -= sizeof(Block4k);
        //    }

        //    while (size >= sizeof(Block256))
        //    {
        //        *(Block256*)pointer = default;
        //        pointer += sizeof(Block256);
        //        size -= sizeof(Block256);
        //    }

        //    while (size >= sizeof(Block64))
        //    {
        //        *(Block64*)pointer = default;
        //        pointer += sizeof(Block64);
        //        size -= sizeof(Block64);
        //    }

        //    if (size >= sizeof(Block32))
        //    {
        //        *(Block32*)pointer = default;
        //        pointer += sizeof(Block32);
        //        size -= sizeof(Block32);
        //    }

        //    if (size >= sizeof(Block16))
        //    {
        //        *(Block16*)pointer = default;
        //        pointer += sizeof(Block16);
        //        size -= sizeof(Block16);
        //    }

        //    if (size >= sizeof(ulong))
        //    {
        //        *(ulong*)pointer = default;
        //        pointer += sizeof(ulong);
        //        size -= sizeof(ulong);
        //    }

        //    if (size >= sizeof(uint))
        //    {
        //        *(uint*)pointer = default;
        //        pointer += sizeof(uint);
        //        size -= sizeof(uint);
        //    }

        //    if (size >= sizeof(ushort))
        //    {
        //        *(ushort*)pointer = default;
        //        pointer += sizeof(ushort);
        //        size -= sizeof(ushort);
        //    }

        //    if (size >= sizeof(byte))
        //    {
        //        *(byte*)pointer = default;
        //        pointer += sizeof(byte);
        //        size -= sizeof(byte);
        //    }

        //    KvDebug.Assert(size == 0, "ZeroMemory Failed!");
        //}

        //internal static unsafe void CopyPage256(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(IsMultiple(size), "Size must be a multiple of MinPageSize!");

        //    while (size >= sizeof(Block4k))
        //    {
        //        *(Block4k*)target = *(Block4k*)source;
        //        target += sizeof(Block4k);
        //        source += sizeof(Block4k);
        //        size -= sizeof(Block4k);
        //    }

        //    while (size >= sizeof(Block256))
        //    {
        //        *(Block256*)target = *(Block256*)source;
        //        target += sizeof(Block256);
        //        source += sizeof(Block256);
        //        size -= sizeof(Block256);
        //    }

        //    KvDebug.Assert(size == 0, "CopyPage256 Failed!");
        //}
        
        internal static unsafe void MemoryMove(byte* target, byte* source, int size)
        {
            Perf.CallCount();

            Buffer.MemoryCopy(source, target, size, size);
        }

        internal static unsafe void MemoryCopy(byte* target, byte* source, int size)
        {
            Perf.CallCount();

            System.Runtime.CompilerServices.Unsafe.CopyBlock(target, source, (uint)size);
        }


        //private static unsafe void MoveMemoryS(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    Buffer.MemoryCopy(source, target, size, size);
        //}

        #endregion

        #region delegates for specific functions

        //internal delegate void MoveMemory_D(byte* target, byte* source, int size);

        //internal static readonly MoveMemory_D MoveMemory;

        #endregion

        #region optimized functions for specific page sizes

        //public static unsafe void ZeroPage4K(byte* pointer, int size)
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(size == 4096, "Pagesize mismatch!");

        //    *(Block4k*)pointer = default;
        //}

        //public static unsafe void CopyPage4K(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(size == 4096, "Pagesize mismatch!");

        //    *(Block4k*)target = *(Block4k*)source;
        //}

        #endregion


        //internal static unsafe void MoveMemory16(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    if (source < target && (source + size) > target)
        //    {
        //        MoveMemoryBackward(target, source, size);
        //    }
        //    else
        //    {
        //        MoveMemoryForward(target, source, size);
        //    }
        //}

        //internal static unsafe void MoveMemoryForward(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    while (size >= sizeof(Block4k))
        //    {
        //        *(Block4k*)target = *(Block4k*)source;
        //        target += sizeof(Block4k);
        //        source += sizeof(Block4k);
        //        size -= sizeof(Block4k);
        //    }

        //    while (size >= sizeof(Block256))
        //    {
        //        *(Block256*)target = *(Block256*)source;
        //        target += sizeof(Block256);
        //        source += sizeof(Block256);
        //        size -= sizeof(Block256);
        //    }

        //    while (size >= sizeof(Block64))
        //    {
        //        *(Block64*)target = *(Block64*)source;
        //        target += sizeof(Block64);
        //        source += sizeof(Block64);
        //        size -= sizeof(Block64);
        //    }

        //    if (size >= sizeof(Block32))
        //    {
        //        *(Block32*)target = *(Block32*)source;
        //        target += sizeof(Block32);
        //        source += sizeof(Block32);
        //        size -= sizeof(Block32);
        //    }

        //    if (size >= sizeof(Block16))
        //    {
        //        *(Block16*)target = *(Block16*)source;
        //        target += sizeof(Block16);
        //        source += sizeof(Block16);
        //        size -= sizeof(Block16);
        //    }

        //    if (size >= sizeof(ulong))
        //    {
        //        *(ulong*)target = *(ulong*)source;
        //        target += sizeof(ulong);
        //        source += sizeof(ulong);
        //        size -= sizeof(ulong);
        //    }

        //    if (size >= sizeof(uint))
        //    {
        //        *(uint*)target = *(uint*)source;
        //        target += sizeof(uint);
        //        source += sizeof(uint);
        //        size -= sizeof(uint);
        //    }

        //    if (size >= sizeof(ushort))
        //    {
        //        *(ushort*)target = *(ushort*)source;
        //        target += sizeof(ushort);
        //        source += sizeof(ushort);
        //        size -= sizeof(ushort);
        //    }

        //    if (size >= sizeof(byte))
        //    {
        //        *(byte*)target = *(byte*)source;
        //        target += sizeof(byte);
        //        source += sizeof(byte);
        //        size -= sizeof(byte);
        //    }

        //    KvDebug.Assert(size == 0, "MemCopy Failed!");
        //}

        //internal static unsafe void MoveMemoryBackward(byte* target, byte* source, int size)
        //{
        //    Perf.CallCount();

        //    KvDebug.Assert(target > source, "Mismatch!");

        //    target += size;
        //    source += size;

        //    var dist = target - source;

        //    if (dist >= 256)
        //    {
        //        while (size >= sizeof(Block256))
        //        {
        //            target -= sizeof(Block256);
        //            source -= sizeof(Block256);
        //            *(Block256*)target = *(Block256*)source;
        //            size -= sizeof(Block256);
        //        }
        //    }

        //    if (dist >= 64)
        //    {
        //        while (size >= sizeof(Block64))
        //        {
        //            target -= sizeof(Block64);
        //            source -= sizeof(Block64);
        //            *(Block64*)target = *(Block64*)source;
        //            size -= sizeof(Block64);
        //        }
        //    }

        //    if (dist >= 32)
        //    {
        //        while (size >= sizeof(Block32))
        //        {
        //            target -= sizeof(Block32);
        //            source -= sizeof(Block32);
        //            *(Block32*)target = *(Block32*)source;
        //            size -= sizeof(Block32);
        //        }
        //    }

        //    //while (size >= sizeof(Block4k))
        //    //{
        //    //    target -= sizeof(Block4k);
        //    //    source -= sizeof(Block4k);
        //    //    *(Block4k*)target = *(Block4k*)source;
        //    //    size -= sizeof(Block4k);
        //    //}

        //    //while (size >= sizeof(Block256))
        //    //{
        //    //    target -= sizeof(Block256);
        //    //    source -= sizeof(Block256);
        //    //    *(Block256*)target = *(Block256*)source;
        //    //    size -= sizeof(Block256);
        //    //}

        //    //while (size >= sizeof(Block64))
        //    //{
        //    //    target -= sizeof(Block64);
        //    //    source -= sizeof(Block64);
        //    //    *(Block64*)target = *(Block64*)source;
        //    //    size -= sizeof(Block64);
        //    //}

        //    //if (size >= sizeof(Block32))
        //    //{
        //    //    target -= sizeof(Block32);
        //    //    source -= sizeof(Block32);
        //    //    *(Block32*)target = *(Block32*)source;
        //    //    size -= sizeof(Block32);
        //    //}

        //    while (size >= sizeof(Block16))
        //    {
        //        target -= sizeof(Block16);
        //        source -= sizeof(Block16);
        //        *(Block16*)target = *(Block16*)source;
        //        size -= sizeof(Block16);
        //    }

        //    if (size >= sizeof(ulong))
        //    {
        //        target -= sizeof(ulong);
        //        source -= sizeof(ulong);
        //        *(ulong*)target = *(ulong*)source;
        //        size -= sizeof(ulong);
        //    }

        //    if (size >= sizeof(uint))
        //    {
        //        target -= sizeof(uint);
        //        source -= sizeof(uint);
        //        *(uint*)target = *(uint*)source;
        //        size -= sizeof(uint);
        //    }

        //    if (size >= sizeof(ushort))
        //    {
        //        target -= sizeof(ushort);
        //        source -= sizeof(ushort);
        //        *(ushort*)target = *(ushort*)source;
        //        size -= sizeof(ushort);
        //    }

        //    if (size >= sizeof(byte))
        //    {
        //        target -= sizeof(byte);
        //        source -= sizeof(byte);
        //        *(byte*)target = *(byte*)source;
        //        size -= sizeof(byte);
        //    }

        //    KvDebug.Assert(size == 0, "MemCopy Failed!");
        //}

        #region Determining atomic size and direction of copy

        /// <summary>
        /// Determines how many bytes can be copied atomically.
        /// </summary>
        /// <returns>The number of bytes that can be copied atomically.</returns>
        //private static unsafe int GetAtomSize()
        //{
        //    Perf.CallCount();

        //    var sizes = new int[] { 64, 32, 16, 8, 4, 2, 1 };

        //    for (int i = 0; i < sizes.Length; i++)
        //    {
        //        var size = sizes[i];

        //        var target1 = GetExpectedResult(size, -1);
        //        var target2 = GetExpectedResult(size, +1);

        //        var buffer1 = GetInitializedBuffer(size);
        //        var buffer2 = GetInitializedBuffer(size);

        //        MoveBytes(buffer1, size, -1);
        //        MoveBytes(buffer2, size, +1);

        //        // Compare with expected result
        //        var ret1 = new Span<byte>(target1).SequenceEqual(buffer1);
        //        var ret2 = new Span<byte>(target2).SequenceEqual(buffer2);

        //        if (ret1 && ret2)
        //        {
        //            return size;
        //        }
        //    }

        //    throw new NotSupportedException("Atom size could not be determined.");
        //}

        /// <summary>
        /// Determines the direction of copy for block of multiple atoms
        /// </summary>
        /// <returns>true - foward, false - backward</returns>
        //private static unsafe bool GetCopyDirection(int atomsize)
        //{
        //    Perf.CallCount();

        //    var size = 2 * atomsize;

        //    var target1 = GetExpectedResult(size, -1);
        //    var target2 = GetExpectedResult(size, +1);

        //    var buffer1 = GetInitializedBuffer(size);
        //    var buffer2 = GetInitializedBuffer(size);

        //    MoveBytes(buffer1, size, -1);
        //    MoveBytes(buffer2, size, +1);

        //    // Compare with expected result
        //    var ret1 = new Span<byte>(target1).SequenceEqual(buffer1);
        //    var ret2 = new Span<byte>(target2).SequenceEqual(buffer2);

        //    if (ret1 && !ret2)
        //    {
        //        // sequences of atoms are copied forward (from left to right)
        //        return true;
        //    }
        //    else if (!ret1 && ret2)
        //    {
        //        // sequences of atoms are copied backward (from right to leftt)
        //        return false;
        //    }

        //    throw new NotSupportedException("Memory copy direction could not be determined.");
        //}


        //private static unsafe void MoveBytes(byte[] buffer, int size, int direction)
        //{
        //    Perf.CallCount();

        //    fixed (byte* ptr = buffer)
        //    {
        //        switch (size)
        //        {
        //            case 64:
        //                *(Block64*)(ptr + 1 + direction) = *(Block64*)(ptr + 1);
        //                break;

        //            case 32:
        //                *(Block32*)(ptr + 1 + direction) = *(Block32*)(ptr + 1);
        //                break;

        //            case 16:
        //                *(Block16*)(ptr + 1 + direction) = *(Block16*)(ptr + 1);
        //                break;

        //            case 8:
        //                *(Block8*)(ptr + 1 + direction) = *(Block8*)(ptr + 1);
        //                break;

        //            case 4:
        //                *(Block4*)(ptr + 1 + direction) = *(Block4*)(ptr + 1);
        //                break;

        //            case 2:
        //                *(Block2*)(ptr + 1 + direction) = *(Block2*)(ptr + 1);
        //                break;

        //            case 1:
        //                *(Block1*)(ptr + 1 + direction) = *(Block1*)(ptr + 1);
        //                break;

        //            default:
        //                throw new NotSupportedException("Size is not supported");
        //        }
        //    }
        //}

        //private unsafe static byte[] GetExpectedResult(int size, int direction)
        //{
        //    Perf.CallCount();

        //    var buffer = GetInitializedBuffer(size);

        //    // generate expected result
        //    fixed (byte* ptr = buffer)
        //    {
        //        Buffer.MemoryCopy(ptr + 1, ptr + 1 + direction, size, size);
        //    }

        //    return buffer;
        //}

        //private static byte[] GetInitializedBuffer(int size)
        //{
        //    Perf.CallCount();

        //    var ret = new byte[size + 2];

        //    for (byte i = 0; i < ret.Length; i++)
        //    {
        //        ret[i] = i;
        //    }

        //    return ret;
        //}

        #endregion
    }
}

