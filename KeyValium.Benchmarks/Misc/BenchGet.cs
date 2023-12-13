using BenchmarkDotNet.Attributes;
using KeyValium.TestBench;
using KeyValium.Cursors;
using LightningDB;
using BenchmarkDotNet.Configs;
using KeyValium.Logging;
using System.Security.Cryptography;

namespace KeyValium.Benchmarks.Misc
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [AllStatisticsColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [HideColumns("Q1", "Q3")]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(1)]
    public class BenchGet
    {
        private static string Token = DateTime.Now.ToString("(yyyy-MM-dd-HH-mm-ss-ffffff)");

#if DEBUG
        const int KeyCount =  10000;
        const int CommitSize = 1000;
#else
        const int KeyCount = 100000;
        const int CommitSize = 10000;
#endif

        //[Params(KeyCount)]
        //public int Keys;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("*** GlobalSetup");

            var td = new TestDescription(nameof(BenchGet))
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

            _pdb.FinishRead();
            _pdb.Dispose();
        }

        PreparedKeyValium _pdb;

        // 450 ns
        [BenchmarkCategory(nameof(GetKeyValue))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void GetKeyValue()
        {
            var tx = _pdb.CurrentTransaction;
            var key = _pdb.GeneratedKeys[10].Key;
            var span = new ReadOnlySpan<byte>(key);
            var keyref = (TreeRef)null;

            for (int i = 0; i < KeyCount; i++)
            {
                tx.Get(null, span);
            }
        }

        // 10 ns
        [BenchmarkCategory(nameof(Validation))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Validation()
        {
            var tx = _pdb.CurrentTransaction;
            var key = _pdb.GeneratedKeys[10].Key;
            var span = new ReadOnlySpan<byte>(key);
            var keyref = (TreeRef)null;

            for (int i = 0; i < KeyCount; i++)
            {
                lock (tx.TxLock)
                {
                    try
                    {
                        tx.Validate(false);
                        keyref?.Validate(tx);
                        tx.ValidateKey(ref span);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(LogTopics.DataAccess, 0, ex);
                        throw;
                    }
                }
            }
        }


        // 1 ns
        [BenchmarkCategory(nameof(GetDataCursor))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void GetDataCursor()
        {
            var tx = _pdb.CurrentTransaction;
            var key = _pdb.GeneratedKeys[10].Key;
            var span = new ReadOnlySpan<byte>(key);
            var keyref = (TreeRef)null;

            for (int i = 0; i < KeyCount; i++)
            {
                using (var cursor = tx.GetDataCursor(keyref))
                {
                    //if (cursor.SetPosition(CursorPositions.Key, ref key))
                    //{
                    //    return cursor.GetCurrentValue();
                    //}
                }
            }
        }

        // 368 ns
        [BenchmarkCategory(nameof(SetPosition))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void SetPosition()
        {
            var tx = _pdb.CurrentTransaction;
            var key = _pdb.GeneratedKeys[10].Key;
            var span = new ReadOnlySpan<byte>(key);
            var keyref = (TreeRef)null;

            using (var cursor = tx.GetDataCursor(null))
            {
                for (int i = 0; i < KeyCount; i++)
                {
                    cursor.SetPosition(span);
                }
                //if ()
                //{
                //    return cursor.GetCurrentValue();
                //}
            }
        }

        // 25 ns
        [BenchmarkCategory(nameof(GetCurrentValue))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void GetCurrentValue()
        {
            var tx = _pdb.CurrentTransaction;
            var key = _pdb.GeneratedKeys[10].Key;
            var span = new ReadOnlySpan<byte>(key);
            var keyref = (TreeRef)null;

            using (var cursor = tx.GetDataCursor(null))
            {
                if (cursor.SetPosition(span))
                {
                    for (int i = 0; i < KeyCount; i++)
                    {
                        var ret = cursor.GetCurrentValue();
                    }
                }
            }
        }
    }
}
