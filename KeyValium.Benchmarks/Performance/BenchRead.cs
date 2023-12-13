using BenchmarkDotNet.Attributes;
using KeyValium.TestBench;
using KeyValium.Cursors;
using LightningDB;
using BenchmarkDotNet.Configs;
using KeyValium.Cache;
using KeyValium.Exceptions;
using KeyValium.Pages;
using KeyValium.Inspector;
using System;
using System.Reflection;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;

namespace KeyValium.Benchmarks.Performance
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [AllStatisticsColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [HideColumns("Q1", "Q3")]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(100000)]
    public class BenchRead
    {
        private static string Token = DateTime.Now.ToString("(yyyy-MM-dd-HH-mm-ss-ffffff)");

#if DEBUG
        const int KeyCount =  10000;
        const int CommitSize = 1000;
#else
        const int KeyCount = 1000000;
        const int CommitSize = 100000;
#endif

        //[Params(KeyCount)]
        //public int Keys;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("*** GlobalSetup");

            var td = new TestDescription(nameof(BenchRead))
            {
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 128,
                MaxValueSize = 128,
                KeyCount = KeyCount,
                CommitSize = CommitSize,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderDelete = KeyOrder.Descending,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderUpdate = KeyOrder.Ascending,
            };

            td.Options.CacheSizeDatabaseMB = 256;
            td.Token = Token;

            _pdb = new PreparedKeyValium(td);
            _pdb.PrepareRead();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            Console.WriteLine("*** GlobalCleanup");

            _pdb.Dispose();
        }

        PreparedKeyValium _pdb;

        [IterationSetup()]
        public void SetupIteration()
        {
            _tx = _pdb.CurrentTransaction;

            var pair = _pdb.CurrentBatch[_pdb.CurrentBatch.Count / 2 + 1];
            _key = pair.Key;
            _val = pair.Value;

            _treeref = null;

            _cursor = _tx.GetDataCursor(_treeref);

            _cursor.CurrentPath.Initialize(true);
            _root = _cursor.RootPagenumber;
            _page = _tx.GetPage(_root, true, out _);

            ReadOnlySpan<byte> key = _key;
            ref var cpage = ref _page.AsContentPage;
            _index = cpage.GetKeyIndex(ref key, out _);

        }

        [IterationCleanup()]
        public void CleanupIteration()
        {
        }

        #region Variables

        Transaction _tx;
        TreeRef _treeref;
        byte[] _key;
        byte[] _val;
        Cursor _cursor;
        ulong _root;
        AnyPage _page;
        int _index;

        #endregion

        #region Common Calls

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void ValidateTx()
        {
            _tx.Validate(false);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void ValidateTreeRef()
        {
            _treeref?.Validate(_tx);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void CreateSpan()
        {
            ReadOnlySpan<byte> key = _key;
            var item = key.Length;
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void ValidateKey()
        {
            ReadOnlySpan<byte> key = _key;
            _tx.ValidateKey(ref key);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void LockTx()
        {
            lock (_tx.TxLock)
            {
            }
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void GetDataCursor()
        {
            var cursor = _tx.GetDataCursor(_treeref);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void DisposeDataCursor()
        {
            _cursor.Dispose();
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SetPosition()
        {
            ReadOnlySpan<byte> key = _key;
            _cursor.SetPosition(key);
        }

        [IterationSetup(Target = nameof(GetCurrentValue))]
        public void SetupGetCurrentValue()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }
        }

        [IterationCleanup(Target = nameof(GetCurrentValue))]
        public void CleanupGetCurrentValue()
        {
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void GetCurrentValue()
        {
            var val = _cursor.GetCurrentValue();
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SP_InitalizeKeyPath()
        {
            _cursor.CurrentPath.Initialize(true);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SP_GetRootPagenumber()
        {
            var root = _cursor.RootPagenumber;
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SeekToKey()
        {
            ReadOnlySpan<byte> key = _key;
            var x = _cursor.SeekToKey(_root, ref key);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void GetPage()
        {
            var page = _tx.GetPage(_root, true, out _);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_AsContentPage()
        {
            ref var cpage = ref _page.AsContentPage;
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_PageTypeCheck()
        {
            ref var cpage = ref _page.AsContentPage;
            if (cpage.PageType != PageTypes.DataIndex && cpage.PageType != PageTypes.DataLeaf)
            {
                throw new KeyValiumException(ErrorCodes.UnhandledPageType, "Unexpected Pagetype: " + _page.PageType.ToString());
            }
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_GetKeyIndexGt()
        {
            ReadOnlySpan<byte> key = _key;
            ref var cpage = ref _page.AsContentPage;
            var index = cpage.GetKeyIndex(ref key, out _);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_GetLeftBranch()
        {
            ref var cpage = ref _page.AsContentPage;
            var pageno = cpage.GetLeftBranch(_index);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_KeyPathAppend()
        {
            _cursor.CurrentPath.Append(_page, _index);
        }

        [BenchmarkCategory("General")]
        [Benchmark()]
        public void SK_EntryCount()
        {
            ref var cpage = ref _page.AsContentPage;
            var count = cpage.EntryCount;
        }

        #region Read

        [BenchmarkCategory("Read")]
        [Benchmark()]
        public void GetKeyValue()
        {
            var val = _tx.Get(_treeref, _key);
        }

        #endregion

        // for index pages search the first key that is greater than the given key
        // so we can always take the left branch


        #endregion

        #region Seek

        //[IterationSetup(Target = nameof(Seek))]
        //public void SetupSeek()
        //{
        //    Console.WriteLine("*** SetupSeek");

        //    _pdb.PrepareRead();
        //}

        //[IterationCleanup(Target = nameof(Seek))]
        //public void CleanupSeek()
        //{
        //    Console.WriteLine("*** CleanupSeek");

        //    _pdb.FinishRead();
        //}

        //[BenchmarkCategory(nameof(Seek))]
        //[Benchmark(OperationsPerInvoke = KeyCount)]
        //public void Seek()
        //{
        //    _pdb.Seek();
        //}

        #endregion

        #region IterateForward

        //[IterationSetup(Target = nameof(ItForward))]
        //public void SetupItForward()
        //{
        //    Console.WriteLine("*** SetupItForward");

        //    _pdb.PrepareRead();
        //}

        //[IterationCleanup(Target = nameof(ItForward))]
        //public void CleanupItForward()
        //{
        //    Console.WriteLine("*** CleanupItForward");

        //    _pdb.FinishRead();
        //}

        //[BenchmarkCategory(nameof(ItForward))]
        //[Benchmark(OperationsPerInvoke = KeyCount)]
        //public void ItForward()
        //{
        //    _pdb.ItForward();
        //}

        #endregion

        #region IterateBackward

        //[IterationSetup(Target = nameof(ItBackward))]
        //public void SetupItBackward()
        //{
        //    Console.WriteLine("*** SetupItBackward");

        //    _pdb.PrepareRead();
        //}

        //[IterationCleanup(Target = nameof(ItBackward))]
        //public void CleanupItBackward()
        //{
        //    Console.WriteLine("*** CleanupItBackward");

        //    _pdb.FinishRead();
        //}

        //[BenchmarkCategory(nameof(ItBackward))]
        //[Benchmark(OperationsPerInvoke = KeyCount)]
        //public void ItBackward()
        //{
        //    _pdb.ItBackward();
        //}

        #endregion
    }
}


