using BenchmarkDotNet.Attributes;
using KeyValium.Benchmarks.Compression;
using KeyValium.Cursors;
using KeyValium.Memory;
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
    [InvocationCount(10000)]
    public class BenchAllocation
    {
        //[Params(64)] //, 256, 1024, 4096, 16384, 65536)]
        //public int Size;

        //private ObjectPool<KeyPointer> pool;

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        //static KeyPointer CreateKeyPointer()
        //{
        //    return new KeyPointer(null, 0);
        //}

        //[GlobalSetup]
        //public void GlobalSetup()
        //{
        //    pool = new ObjectPool<KeyPointer>(CreateKeyPointer);
        //}

        //[GlobalCleanup]
        //public void GlobalCleanup()
        //{
        //}

        //[IterationSetup]
        //public void IterationSetup()
        //{
        //}

        //[IterationCleanup]
        //public void IterationCleanup()
        //{
        //}

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        //[Benchmark]
        //public unsafe void NewKeyPointerPooled()
        //{
        //    var x = pool.Allocate();
        //    //pool.Free(x);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        //[Benchmark]
        //public unsafe void NewKeyPointer()
        //{
        //    var x = new KeyPointer(null, 0);
        //    var y = x.Page;
        //    //pool.Free(x);
        //}

        //[Benchmark(Baseline = true)]
        //public unsafe void New()
        //{
        //    var x = new byte[Size];
        //}

        //[Benchmark()]
        //public unsafe void GcAlloc()
        //{
        //    var x = GC.AllocateArray<byte>(Size, false);
        //}

        //[Benchmark()]
        //public unsafe void GcAllocPinned()
        //{
        //    var x = GC.AllocateArray<byte>(Size, true);
        //}

        //[Benchmark()]
        //public unsafe void GcAllocUI()
        //{
        //    var x = GC.AllocateUninitializedArray<byte>(Size, false);
        //}

        //[Benchmark()]
        //public unsafe void GcAllocUIPinned()
        //{
        //    var x = GC.AllocateUninitializedArray<byte>(Size, true);
        //}

        ////[Benchmark()]
        ////public unsafe void AllocCoTask()
        ////{
        ////    var x = Marshal.AllocCoTaskMem(Size);
        ////}

        ////[Benchmark()]
        ////public unsafe void AllocHGlobal()
        ////{
        ////    var x = Marshal.AllocHGlobal(Size);
        ////}
    }
}

