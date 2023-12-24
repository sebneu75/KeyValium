using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Performance
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [AllStatisticsColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [HideColumns("Q1", "Q3")]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(1)]
    public class BenchCommitRollback
    {
        private static string Token = DateTime.Now.ToString("(yyyy-MM-dd-HH-mm-ss-ffffff)");

        const int KeyCount = 100000;
        const int CommitSize = 10000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("*** GlobalSetup");

            var td = new TestDescription(nameof(BenchCommitRollback))
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

            td.Options.CacheSizeMB = 256;
            td.Token = Token;

            _pdb = new PreparedKeyValium(td);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            Console.WriteLine("*** GlobalCleanup");

            _pdb.Dispose();
        }

        PreparedKeyValium _pdb;

        #region BeginReadTransaction

        [IterationSetup(Target = nameof(BeginRT))]
        public void SetupBeginRT()
        {
            Console.WriteLine("*** SetupBeginRT");

            _pdb.PrepareBeginRT();
        }

        [IterationCleanup(Target = nameof(BeginRT))]
        public void CleanupBeginRT()
        {
            Console.WriteLine("*** CleanupBeginRT");

            _pdb.FinishBeginRT();
        }

        [BenchmarkCategory(nameof(BeginRT))]
        //[Benchmark(OperationsPerInvoke = 1)]
        public void BeginRT()
        {
            _pdb.BeginRT();
        }

        #endregion

        #region CommitReadTransaction

        [IterationSetup(Target = nameof(CommitRT))]
        public void SetupCommitRT()
        {
            Console.WriteLine("*** SetupCommitRT");

            _pdb.PrepareCommitRT();
        }

        [IterationCleanup(Target = nameof(CommitRT))]
        public void CleanupCommitRT()
        {
            Console.WriteLine("*** CleanupCommitRT");

            _pdb.FinishCommitRT();
        }

        [BenchmarkCategory(nameof(CommitRT))]
        [Benchmark(OperationsPerInvoke = 1)]
        public void CommitRT()
        {
            _pdb.CommitRT();
        }

        #endregion

        #region CommitReadTransaction1

        [IterationSetup(Target = nameof(CommitRT1))]
        public void SetupCommitRT1()
        {
            Console.WriteLine("*** SetupCommitRT1");

            _pdb.PrepareCommitRT();

            _pdb.CurrentTransaction.Commit();

            //_tx = _pdb.CurrentTransaction;
            //_isreadonly = _tx.IsReadOnly;

        }

        [IterationCleanup(Target = nameof(CommitRT1))]
        public void CleanupCommitRT1()
        {
            Console.WriteLine("*** CleanupCommitRT");

            _pdb.FinishCommitRT();
        }

        Transaction _tx;
        bool _isreadonly;

        [BenchmarkCategory(nameof(CommitRT1))]
        [Benchmark(OperationsPerInvoke = 10000)]
        public void CommitRT1()
        {
            for (int i = 0; i < 10000; i++)
            {
                using (var tx = _pdb.Database.BeginReadTransaction())
                {
                    tx.Commit();
                }
            }
        }

        #endregion

        #region RollbackReadTransaction

        [IterationSetup(Target = nameof(RollbackRT))]
        public void SetupRollbackRT()
        {
            Console.WriteLine("*** SetupRollbackRT");

            _pdb.PrepareRollbackRT();
        }

        [IterationCleanup(Target = nameof(RollbackRT))]
        public void CleanupRollbackRT()
        {
            Console.WriteLine("*** CleanupRollbackRT");

            _pdb.FinishRollbackRT();
        }

        [BenchmarkCategory(nameof(RollbackRT))]
        //[Benchmark(OperationsPerInvoke = 1)]
        public void RollbackRT()
        {
            _pdb.RollbackRT();
        }

        #endregion

        #region BeginWriteTransaction

        [IterationSetup(Target = nameof(BeginWT))]
        public void SetupBeginWT()
        {
            Console.WriteLine("*** SetupBeginWT");

            _pdb.PrepareBeginWT();
        }

        [IterationCleanup(Target = nameof(BeginWT))]
        public void CleanupBeginWT()
        {
            Console.WriteLine("*** CleanupBeginWT");

            _pdb.FinishBeginWT();
        }

        [BenchmarkCategory(nameof(BeginWT))]
        //[Benchmark(OperationsPerInvoke = 1)]
        public void BeginWT()
        {
            _pdb.BeginWT();
        }

        #endregion

        #region CommitWriteTransaction

        [IterationSetup(Target = nameof(CommitWT))]
        public void SetupCommitWT()
        {
            Console.WriteLine("*** SetupCommitWT");

            _pdb.PrepareCommitWT();
        }

        [IterationCleanup(Target = nameof(CommitWT))]
        public void CleanupCommitWT()
        {
            Console.WriteLine("*** CleanupCommitWT");

            _pdb.FinishCommitWT();
        }

        [BenchmarkCategory(nameof(CommitWT))]
        //[Benchmark(OperationsPerInvoke = 1)]
        public void CommitWT()
        {
            _pdb.CommitWT();
        }

        #endregion

        #region RollbackWriteTransaction

        [IterationSetup(Target = nameof(RollbackWT))]
        public void SetupRollbackWT()
        {
            Console.WriteLine("*** SetupRollbackWT");

            _pdb.PrepareRollbackWT();
        }

        [IterationCleanup(Target = nameof(RollbackWT))]
        public void CleanupRollbackWT()
        {
            Console.WriteLine("*** CleanupRollbackWT");

            _pdb.FinishRollbackWT();
        }

        [BenchmarkCategory(nameof(RollbackWT))]
        //[Benchmark(OperationsPerInvoke = 1)]
        public void RollbackWT()
        {
            _pdb.RollbackWT();
        }

        #endregion
    }
}
