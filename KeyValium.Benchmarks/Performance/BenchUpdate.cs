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
using KeyValium.Pages.Entries;
using KeyValium.TestBench.Helpers;

namespace KeyValium.Benchmarks.Performance
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [AllStatisticsColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [HideColumns("Q1", "Q3")]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(1000000)]
    public unsafe class BenchUpdate
    {
        private static string Token = DateTime.Now.ToString("(yyyy-MM-dd-HH-mm-ss-ffffff)");

#if DEBUG
        const int KeyCount = 10000;
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
            _pdb.CommitRT();
            _pdb.CurrentTransaction.Dispose();
            _pdb.CurrentTransaction = null;
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
            Console.WriteLine("*** SetupIteration");

            _pdb.CurrentTransaction?.Commit();
            _pdb.CurrentTransaction?.Dispose();
            _pdb.CurrentTransaction = null;

            _pdb.BeginWT();

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
            Console.WriteLine("*** CleanupIteration");

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

        #region Read

        [BenchmarkCategory("Read")]
        //[Benchmark()]
        public void GetKeyValue()
        {
            var val = _tx.Get(_treeref, _key);
        }

        #endregion

        #region Update

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void UpdateKeyValue()
        {
            _tx.Update(_treeref, _key, _val);
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void ValInfo()
        {
            var x = new ValInfo(_val);
        }

        [IterationSetup(Target = nameof(GetCurrentEntryInfo))]
        public void SetupGetCurrentEntryInfo()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }
        }

        [IterationCleanup(Target = nameof(GetCurrentEntryInfo))]
        public void CleanupGetCurrentEntryInfo()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void GetCurrentEntryInfo()
        {
            _cursor.GetCurrentEntryInfo(out var subtree, out var gc, out var lc, out var ovpageno);
        }

        [IterationSetup(Target = nameof(DeleteOverflowPages))]
        public void SetupDeleteOverflowPages()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }
        }

        [IterationCleanup(Target = nameof(DeleteOverflowPages))]
        public void CleanupDeleteOverflowPages()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void DeleteOverflowPages()
        {
            _tx.DeleteOverflowPages(_cursor.GetCurrentValueOverflow());
        }

        [IterationSetup(Target = nameof(CreateLeafEntry))]
        public void SetupCreateLeafEntry()
        {
            SetupIteration();
        }

        [IterationCleanup(Target = nameof(CreateLeafEntry))]
        public void CleanupCreateLeafEntry()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void CreateLeafEntry()
        {
            var bs = new ValInfo(_val);
            var entry = new EntryExtern(_key, bs, 0, 0, 0, 0, 0);
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void MaxInlineValueSize()
        {
            var val = _tx.Database.Limits.MaxInlineValueSize((ushort)_key.Length);
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void SpillCheck()
        {
            _tx.SpillCheck();
        }

        [IterationSetup(Target = nameof(UpdateKey))]
        public void SetupUpdateKey()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }
        }

        [IterationCleanup(Target = nameof(UpdateKey))]
        public void CleanupUpdateKey()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void UpdateKey()
        {
            var bs = new ValInfo(_val);
            var entry = new EntryExtern(_key, bs, 0, 0, 0, 0, 0);
            _cursor.UpdateKey(ref entry);
        }

        [IterationSetup(Target = nameof(Touch))]
        public void SetupTouch()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }
        }

        [IterationCleanup(Target = nameof(Touch))]
        public void CleanupTouch()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void Touch()
        {
            _cursor.Touch(false);
        }

        [IterationSetup(Target = nameof(GetEntrySize))]
        public void SetupGetEntrySize()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }

            _espage = _cursor.CurrentPath.CurrentItem.Page;
            _esindex = _cursor.CurrentPath.CurrentItem.KeyIndex;
            _cp = _espage.AsContentPage;
        }

        AnyPage _espage;
        ContentPage _cp;
        int _esindex;

        [IterationCleanup(Target = nameof(GetEntrySize))]
        public void CleanupGetEntrySize()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void GetEntrySize()
        {
            var entrysize = _cp.GetEntrySize(_esindex);
        }

        [IterationSetup(Target = nameof(ATOUpdateKey))]
        public void SetupATOUpdateKey()
        {
            SetupIteration();

            ReadOnlySpan<byte> key = _key;
            if (!_cursor.SetPosition(key))
            {
                throw new Exception("Key not found");
            }

            _espage = _cursor.CurrentPath.CurrentItem.Page;
            _esindex = _cursor.CurrentPath.CurrentItem.KeyIndex;
            _cp = _espage.AsContentPage;
        }

        [IterationCleanup(Target = nameof(ATOUpdateKey))]
        public void CleanupATOUpdateKey()
        {
        }

        [BenchmarkCategory("Update")]
        [Benchmark()]
        public void ATOUpdateKey()
        {
            _tx.ATOUpdateKey(_cursor, _espage.PageNumber, _esindex);
        }

        #endregion
        // for index pages search the first key that is greater than the given key
        // so we can always take the left branch
    }
}


