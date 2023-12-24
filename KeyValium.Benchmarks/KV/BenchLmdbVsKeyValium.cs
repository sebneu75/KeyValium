using BenchmarkDotNet.Attributes;
using KeyValium.TestBench;
using KeyValium.Cursors;
using LightningDB;
using BenchmarkDotNet.Configs;

namespace KeyValium.Benchmarks.KV
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [AllStatisticsColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [HideColumns("Q1", "Q3")]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(1)]
    public class BenchLmdbVsKeyValium
    {
        private static string Token = DateTime.Now.ToString("(yyyy-MM-dd-HH-mm-ss-ffffff)");

#if DEBUG
        const int KeyCount =  10000;
        const int CommitSize = 1000;
#else
        const int KeyCount = 100000;
        const int CommitSize = 100000;
#endif

        //[Params(KeyCount)]
        //public int Keys;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Console.WriteLine("*** GlobalSetup");

            //foreach (var pagesize in PageSizes)
            //{
            //    var td = GetStandardTest(Name + "-Sequential");
            //    td.ParameterName = "PageSize";
            //    td.ParameterValue = pagesize.ToString();
            //    td.Options.PageSize = pagesize;

            //    yield return td;
            //}

            //var cachesizes = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };
            //foreach (var cachesize in cachesizes)
            //{
            //    var td = GetStandardTest(Name + "-Sequential");
            //    td.ParameterName = "CacheSize";
            //    td.ParameterValue = cachesize.ToString();
            //    td.Options.CacheSizeDatabaseMB = cachesize;

            //    yield return td;
            //}

            //var keysizes = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024 };
            //foreach (var keysize in keysizes)
            //{
            //    var td = GetStandardTest(Name + "-Sequential");
            //    td.ParameterName = "KeySize";
            //    td.ParameterValue = keysize.ToString();
            //    td.MinKeySize = keysize;
            //    td.MaxKeySize = keysize;

            //    yield return td;
            //}

            //var valuesizes = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
            //foreach (var valuesize in valuesizes)
            //{
            //    var td = GetStandardTest(Name + "-Sequential");
            //    td.ParameterName = "ValueSize";
            //    td.ParameterValue = valuesize.ToString();
            //    td.MinValueSize = valuesize;
            //    td.MaxValueSize = valuesize;

            //    yield return td;
            //}

            //var commitsizes = new int[] { 1, 10, 100, 1000, 10000, 100000 };
            //foreach (var commitsize in commitsizes)
            //{
            //    var td = GetStandardTest(Name + "-Sequential");
            //    td.ParameterName = "CommitSize";
            //    td.ParameterValue = commitsize.ToString();
            //    td.CommitSize = commitsize;

            //    yield return td;
            //}

            var td = new TestDescription(nameof(BenchLmdbVsKeyValium))
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
            _lmdb = new PreparedLmdb(td);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            Console.WriteLine("*** GlobalCleanup");

            _pdb.Dispose();
            _lmdb.Dispose();
        }

        PreparedDatabase _pdb;
        PreparedDatabase _lmdb;

        #region KeyValium

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
        [Benchmark(OperationsPerInvoke = 1)]
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
        [Benchmark(OperationsPerInvoke = 1)]
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
        [Benchmark(OperationsPerInvoke = 1)]
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
        [Benchmark(OperationsPerInvoke = 1)]
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
        [Benchmark(OperationsPerInvoke = 1)]
        public void RollbackWT()
        {
            _pdb.RollbackWT();
        }

        #endregion

        #region Insert

        [IterationSetup(Target = nameof(Insert))]
        public void SetupInsert()
        {
            Console.WriteLine("*** SetupInsert");

            _pdb.PrepareInsert();
        }

        [IterationCleanup(Target = nameof(Insert))]
        public void CleanupInsert()
        {
            Console.WriteLine("*** CleanupInsert");

            _pdb.FinishInsert();
        }

        [BenchmarkCategory(nameof(Insert))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Insert()
        {
            _pdb.Insert();
        }

        #endregion

        #region Update

        [IterationSetup(Target = nameof(Update))]
        public void SetupUpdate()
        {
            Console.WriteLine("*** SetupUpdate");

            _pdb.PrepareUpdate();
        }

        [IterationCleanup(Target = nameof(Update))]
        public void CleanupUpdate()
        {
            Console.WriteLine("*** CleanupUpdate");

            _pdb.FinishUpdate();
        }

        [BenchmarkCategory(nameof(Update))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Update()
        {
            _pdb.Update();
        }

        #endregion

        #region Delete

        [IterationSetup(Target = nameof(Delete))]
        public void SetupDelete()
        {
            Console.WriteLine("*** SetupDelete");

            _pdb.PrepareDelete();
        }

        [IterationCleanup(Target = nameof(Delete))]
        public void CleanupDelete()
        {
            Console.WriteLine("*** CleanupDelete");

            _pdb.FinishDelete();
        }

        [BenchmarkCategory(nameof(Delete))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Delete()
        {
            _pdb.Delete();
        }

        #endregion

        #region Read

        [IterationSetup(Target = nameof(Read))]
        public void SetupRead()
        {
            Console.WriteLine("*** SetupRead");

            _pdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(Read))]
        public void CleanupRead()
        {
            Console.WriteLine("*** CleanupRead");

            _pdb.FinishRead();
        }

        [BenchmarkCategory(nameof(Read))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Read()
        {
            _pdb.Read();
        }

        #endregion

        #region Seek

        [IterationSetup(Target = nameof(Seek))]
        public void SetupSeek()
        {
            Console.WriteLine("*** SetupSeek");

            _pdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(Seek))]
        public void CleanupSeek()
        {
            Console.WriteLine("*** CleanupSeek");

            _pdb.FinishRead();
        }

        [BenchmarkCategory(nameof(Seek))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void Seek()
        {
            _pdb.Seek();
        }

        #endregion

        #region IterateForward

        [IterationSetup(Target = nameof(ItForward))]
        public void SetupItForward()
        {
            Console.WriteLine("*** SetupItForward");

            _pdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(ItForward))]
        public void CleanupItForward()
        {
            Console.WriteLine("*** CleanupItForward");

            _pdb.FinishRead();
        }

        [BenchmarkCategory(nameof(ItForward))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void ItForward()
        {
            _pdb.ItForward();
        }

        #endregion

        #region IterateBackward

        [IterationSetup(Target = nameof(ItBackward))]
        public void SetupItBackward()
        {
            Console.WriteLine("*** SetupItBackward");

            _pdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(ItBackward))]
        public void CleanupItBackward()
        {
            Console.WriteLine("*** CleanupItBackward");

            _pdb.FinishRead();
        }

        [BenchmarkCategory(nameof(ItBackward))]
        [Benchmark(OperationsPerInvoke = KeyCount)]
        public void ItBackward()
        {
            _pdb.ItBackward();
        }

        #endregion

        #endregion

        #region Lmdb

        #region BeginReadTransaction

        [IterationSetup(Target = nameof(BeginRTLmdb))]
        public void SetupBeginRTLmdb()
        {
            Console.WriteLine("*** SetupBeginRTLmdb");

            _lmdb.PrepareBeginRT();
        }

        [IterationCleanup(Target = nameof(BeginRTLmdb))]
        public void CleanupBeginRTLmdb()
        {
            Console.WriteLine("*** CleanupBeginRTLmdb");

            _lmdb.FinishBeginRT();
        }

        [BenchmarkCategory(nameof(BeginRT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void BeginRTLmdb()
        {
            _lmdb.BeginRT();
        }

        #endregion

        #region CommitReadTransaction

        [IterationSetup(Target = nameof(CommitRTLmdb))]
        public void SetupCommitRTLmdb()
        {
            Console.WriteLine("*** SetupCommitRTLmdb");

            _lmdb.PrepareCommitRT();
        }

        [IterationCleanup(Target = nameof(CommitRTLmdb))]
        public void CleanupCommitRTLmdb()
        {
            Console.WriteLine("*** CleanupCommitRTLmdb");

            _lmdb.FinishCommitRT();
        }

        [BenchmarkCategory(nameof(CommitRT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void CommitRTLmdb()
        {
            _lmdb.CommitRT();
        }

        #endregion

        #region RollbackReadTransaction

        [IterationSetup(Target = nameof(RollbackRTLmdb))]
        public void SetupRollbackRTLmdb()
        {
            Console.WriteLine("*** SetupRollbackRTLmdb");

            _lmdb.PrepareRollbackRT();
        }

        [IterationCleanup(Target = nameof(RollbackRTLmdb))]
        public void CleanupRollbackRTLmdb()
        {
            Console.WriteLine("*** CleanupRollbackRTLmdb");

            _lmdb.FinishRollbackRT();
        }

        [BenchmarkCategory(nameof(RollbackRT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void RollbackRTLmdb()
        {
            _lmdb.RollbackRT();
        }

        #endregion

        #region BeginWriteTransaction

        [IterationSetup(Target = nameof(BeginWTLmdb))]
        public void SetupBeginWTLmdb()
        {
            Console.WriteLine("*** SetupBeginWTLmdb");

            _lmdb.PrepareBeginWT();
        }

        [IterationCleanup(Target = nameof(BeginWTLmdb))]
        public void CleanupBeginWTLmdb()
        {
            Console.WriteLine("*** CleanupBeginWTLmdb");

            _lmdb.FinishBeginWT();
        }

        [BenchmarkCategory(nameof(BeginWT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void BeginWTLmdb()
        {
            _lmdb.BeginWT();
        }

        #endregion

        #region CommitWriteTransaction

        [IterationSetup(Target = nameof(CommitWTLmdb))]
        public void SetupCommitWTLmdb()
        {
            Console.WriteLine("*** SetupCommitWTLmdb");

            _lmdb.PrepareCommitWT();
        }

        [IterationCleanup(Target = nameof(CommitWTLmdb))]
        public void CleanupCommitWTLmdb()
        {
            Console.WriteLine("*** CleanupCommitWTLmdb");

            _lmdb.FinishCommitWT();
        }

        [BenchmarkCategory(nameof(CommitWT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void CommitWTLmdb()
        {
            _lmdb.CommitWT();
        }

        #endregion

        #region RollbackWriteTransaction

        [IterationSetup(Target = nameof(RollbackWTLmdb))]
        public void SetupRollbackWTLmdb()
        {
            Console.WriteLine("*** SetupRollbackWTLmdb");

            _lmdb.PrepareRollbackWT();
        }

        [IterationCleanup(Target = nameof(RollbackWTLmdb))]
        public void CleanupRollbackWTLmdb()
        {
            Console.WriteLine("*** CleanupRollbackWTLmdb");

            _lmdb.FinishRollbackWT();
        }

        [BenchmarkCategory(nameof(RollbackWT))]
        [Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        public void RollbackWTLmdb()
        {
            _lmdb.RollbackWT();
        }

        #endregion

        #region Insert

        [IterationSetup(Target = nameof(InsertLmdb))]
        public void SetupInsertLmdb()
        {
            Console.WriteLine("*** SetupInsertLmdb");

            _lmdb.PrepareInsert();
        }

        [IterationCleanup(Target = nameof(InsertLmdb))]
        public void CleanupInsertLmdb()
        {
            Console.WriteLine("*** CleanupInsertLmdb");

            _lmdb.FinishInsert();
        }

        [BenchmarkCategory(nameof(Insert))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void InsertLmdb()
        {
            _lmdb.Insert();
        }

        #endregion

        #region Update

        [IterationSetup(Target = nameof(UpdateLmdb))]
        public void SetupUpdateLmdb()
        {
            Console.WriteLine("*** SetupUpdateLmdb");

            _lmdb.PrepareUpdate();
        }

        [IterationCleanup(Target = nameof(UpdateLmdb))]
        public void CleanupUpdateLmdb()
        {
            Console.WriteLine("*** CleanupUpdateLmdb");

            _lmdb.FinishUpdate();
        }

        [BenchmarkCategory(nameof(Update))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void UpdateLmdb()
        {
            _lmdb.Update();
        }

        #endregion

        #region Delete

        [IterationSetup(Target = nameof(DeleteLmdb))]
        public void SetupDeleteLmdb()
        {
            Console.WriteLine("*** SetupDeleteLmdb");

            _lmdb.PrepareDelete();
        }

        [IterationCleanup(Target = nameof(DeleteLmdb))]
        public void CleanupDeleteLmdb()
        {
            Console.WriteLine("*** CleanupDeleteLmdb");

            _lmdb.FinishDelete();
        }

        [BenchmarkCategory(nameof(Delete))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void DeleteLmdb()
        {
            _lmdb.Delete();
        }

        #endregion

        #region Read

        [IterationSetup(Target = nameof(ReadLmdb))]
        public void SetupReadLmdb()
        {
            Console.WriteLine("*** SetupReadLmdb");

            _lmdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(ReadLmdb))]
        public void CleanupReadLmdb()
        {
            Console.WriteLine("*** CleanupReadLmdb");

            _lmdb.FinishRead();
        }

        [BenchmarkCategory(nameof(Read))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void ReadLmdb()
        {
            _lmdb.Read();
        }

        #endregion

        #region Seek

        [IterationSetup(Target = nameof(SeekLmdb))]
        public void SetupSeekLmdb()
        {
            Console.WriteLine("*** SetupSeekLmdb");

            _lmdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(SeekLmdb))]
        public void CleanupSeekLmdb()
        {
            Console.WriteLine("*** CleanupSeekLmdb");

            _lmdb.FinishRead();
        }

        [BenchmarkCategory(nameof(Seek))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void SeekLmdb()
        {
            _lmdb.Seek();
        }

        #endregion

        #region IterateForward

        [IterationSetup(Target = nameof(ItForwardLmdb))]
        public void SetupItForwardLmdb()
        {
            Console.WriteLine("*** SetupItForwardLmdb");

            _lmdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(ItForwardLmdb))]
        public void CleanupItForwardLmdb()
        {
            Console.WriteLine("*** CleanupItForwardLmdb");

            _lmdb.FinishRead();
        }

        [BenchmarkCategory(nameof(ItForward))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void ItForwardLmdb()
        {
            _lmdb.ItForward();
        }

        #endregion

        #region IterateBackward

        [IterationSetup(Target = nameof(ItBackwardLmdb))]
        public void SetupItBackwardLmdb()
        {
            Console.WriteLine("*** SetupItBackwardLmdb");

            _lmdb.PrepareRead();
        }

        [IterationCleanup(Target = nameof(ItBackwardLmdb))]
        public void CleanupItBackwardLmdb()
        {
            Console.WriteLine("*** CleanupItBackwardLmdb");

            _lmdb.FinishRead();
        }

        [BenchmarkCategory(nameof(ItBackward))]
        [Benchmark(OperationsPerInvoke = KeyCount, Baseline = true)]
        public void ItBackwardLmdb()
        {
            _lmdb.ItBackward();
        }

        #endregion

        #endregion

    }
}





//namespace Test.Benchmarks
//{
//    internal class BenchAll_Lmdb : BenchmarkBase
//    {
//        public override IEnumerable<TestDescription> GetItems()
//        {
//            var td = new TestDescription(Name);
//            td.MinKeySize = 16;
//            td.MaxKeySize = 16;
//            td.MinValueSize = 128;
//            td.MaxValueSize = 128;
//            td.KeyCount = 100000;
//            td.CommitSize = 10000;
//            td.GenStrategy = Util.KeyGenStrategy.Sequential;
//            td.OrderDelete = Util.KeyOrder.Descending;
//            td.OrderInsert = Util.KeyOrder.Ascending;
//            td.OrderRead = Util.KeyOrder.Ascending;
//            td.OrderUpdate = Util.KeyOrder.Ascending;

//            yield return td;
//        }

//        public override string Name => "BenchAll-Lmdb";

//        override internal void RunItem(TestDescription td)
//        {
//            var dbfile = td.DbFilename + ".lmdb";
//            File.Delete(dbfile);

//            var items = td.GenerateKeys(0, td.KeyCount);

//            var cfg = new EnvironmentConfiguration();
//            cfg.MaxDatabases = 2;
//            cfg.MapSize = 512 * 1024 * 1024; // 512MB

//            using (var env = new LightningEnvironment(dbfile, cfg))
//            {
//                env.Open(EnvironmentOpenFlags.NoSubDir | EnvironmentOpenFlags.NoReadAhead | EnvironmentOpenFlags.NoSync);

//                var list = KeyValueGenerator.Order(items, td.OrderInsert);

//                InsertItems(td, env, list);

//                list = KeyValueGenerator.Order(list, td.OrderRead);
//                ReadItems(td, env, list);
//                IterateItems(td, env, true, list);

//                list = KeyValueGenerator.Order(list, td.OrderUpdate);
//                UpdateItems(td, env, list);

//                list = KeyValueGenerator.Order(list, td.OrderRead);
//                ReadItems(td, env, list);
//                IterateItems(td, env, false, list);

//                list = KeyValueGenerator.Order(list, td.OrderDelete);
//                DeleteItems(td, env, list);
//            }
//        }

//        protected void IterateItems(TestDescription td, LightningEnvironment env, bool forward, List<KeyValuePair<byte[], byte[]>> items)
//        {
//            var (tx, db) = BeginReadTransaction(td, env);
//            var count = 0;

//            var title = string.Format("Iterate{0}", forward ? "Forward" : "Backward");

//            try
//            {
//                var m = td.Measure.MeasureTime(title, 0, td.KeyCount, () =>
//                {
//                    using (var iter = tx.CreateCursor(db))
//                    {
//                        if (forward)
//                        {
//                            while (iter.Next() == MDBResultCode.Success)
//                            {
//                                var (resultCode, mdbkey, mdbval) = iter.GetCurrent();
//                                var key = mdbkey.CopyToNewArray();
//                                var val = mdbval.CopyToNewArray();

//                                var keylen = key == null ? 0 : key.Length;
//                                var vallen = val == null ? 0 : val.Length;

//                                Debug.Assert(keylen >= td.MinKeySize && keylen <= td.MaxKeySize, "Fail!");
//                                Debug.Assert(vallen >= td.MinValueSize && vallen <= td.MaxValueSize, "Fail!");

//                                count++;
//                            }
//                        }
//                        else
//                        {
//                            iter.Last();

//                            while (iter.Previous() == MDBResultCode.Success)
//                            {
//                                var (resultCode, mdbkey, mdbval) = iter.GetCurrent();
//                                var key = mdbkey.CopyToNewArray();
//                                var val = mdbval.CopyToNewArray();

//                                var keylen = key == null ? 0 : key.Length;
//                                var vallen = val == null ? 0 : val.Length;

//                                Debug.Assert(keylen >= td.MinKeySize && keylen <= td.MaxKeySize, "Fail!");
//                                Debug.Assert(vallen >= td.MinValueSize && vallen <= td.MaxValueSize, "Fail!");

//                                count++;
//                            }
//                        }
//                    }
//                });

//                Console.WriteLine("{0} (Count: {1})", m, count);

//                CommitReadTransaction(td, tx, db);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                RollbackReadTransaction(td, tx, db);
//                throw;
//            }
//        }

//        protected void ReadItems(TestDescription td, LightningEnvironment env, List<KeyValuePair<byte[], byte[]>> items)
//        {
//            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
//            {
//                var temp = items.Skip(i).Take(td.CommitSize).ToList();

//                var (tx, db) = BeginReadTransaction(td, env);

//                try
//                {
//                    var m = td.Measure.MeasureTime("Read", i / td.CommitSize, td.CommitSize, () =>
//                    {
//                        for (int k = 0; k < temp.Count; k++)
//                        {
//                            var (resultCode, key, mdbval) = tx.Get(db, temp[k].Key);
//                            var val = mdbval.CopyToNewArray();
//                            var len = val == null ? 0 : val.Length;
//                            Debug.Assert(len >= td.MinValueSize && len <= td.MaxValueSize, "Fail!");
//                        }
//                    });

//                    Console.WriteLine(m);

//                    CommitReadTransaction(td, tx, db);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    RollbackReadTransaction(td, tx, db);
//                    throw;
//                }
//            }
//        }

//        protected void InsertItems(TestDescription td, LightningEnvironment env, List<KeyValuePair<byte[], byte[]>> items)
//        {
//            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
//            {
//                var temp = items.Skip(i).Take(td.CommitSize).ToList();

//                var (tx, db) = BeginWriteTransaction(td, env);

//                try
//                {
//                    var m = td.Measure.MeasureTime("Insert", i / td.CommitSize, td.CommitSize, () =>
//                    {
//                        for (int k = 0; k < temp.Count; k++)
//                        {
//                            //Console.WriteLine(Tools.GetHexString(temp[k].Key));
//                            tx.Put(db, temp[k].Key, temp[k].Value);
//                        }
//                    });

//                    Console.WriteLine(m);

//                    CommitWriteTransaction(td, tx, db);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    RollbackWriteTransaction(td, tx, db);
//                    throw;
//                }
//            }
//        }

//        protected void UpdateItems(TestDescription td, LightningEnvironment env, List<KeyValuePair<byte[], byte[]>> items)
//        {
//            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
//            {
//                var temp = items.Skip(i).Take(td.CommitSize).ToList();

//                var (tx, db) = BeginWriteTransaction(td, env);

//                try
//                {
//                    var m = td.Measure.MeasureTime("Update", i / td.CommitSize, td.CommitSize, () =>
//                    {
//                        for (int k = 0; k < temp.Count; k++)
//                        {
//                            tx.Put(db, temp[k].Key, temp[k].Value);
//                        }
//                    });

//                    Console.WriteLine(m);

//                    CommitWriteTransaction(td, tx, db);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    RollbackWriteTransaction(td, tx, db);
//                    throw;
//                }
//            }
//        }

//        protected void DeleteItems(TestDescription td, LightningEnvironment env, List<KeyValuePair<byte[], byte[]>> items)
//        {
//            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
//            {
//                var temp = items.Skip(i).Take(td.CommitSize).ToList();

//                var (tx, db) = BeginWriteTransaction(td, env);

//                try
//                {
//                    var m = td.Measure.MeasureTime("Delete", i / td.CommitSize, td.CommitSize, () =>
//                    {
//                        for (int k = 0; k < temp.Count; k++)
//                        {
//                            tx.Delete(db, temp[k].Key);
//                        }
//                    });

//                    Console.WriteLine(m);

//                    CommitWriteTransaction(td, tx, db);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    RollbackWriteTransaction(td, tx, db);
//                    throw;
//                }
//            }
//        }

//        protected (LightningTransaction, LightningDatabase) BeginWriteTransaction(TestDescription td, LightningEnvironment env)
//        {
//            LightningTransaction tx = null;
//            LightningDatabase db = null;

//            var m = td.Measure.MeasureTime("BeginWT", 0, 1, () =>
//            {
//                tx = env.BeginTransaction();
//                db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
//            });


//            //Console.WriteLine(m);

//            return (tx, db);
//        }

//        protected void CommitWriteTransaction(TestDescription td, LightningTransaction tx, LightningDatabase db)
//        {
//            var m = td.Measure.MeasureTime("CommitWT", 0, 1, () =>
//            {
//                tx.Commit();
//            });

//            db.Dispose();
//            tx.Dispose();

//            //Console.WriteLine(m);
//        }

//        protected void RollbackWriteTransaction(TestDescription td, LightningTransaction tx, LightningDatabase db)
//        {
//            var m = td.Measure.MeasureTime("RollbackWT", 0, 1, () =>
//            {
//                tx.Abort();
//            });

//            db.Dispose();
//            tx.Dispose();

//            //Console.WriteLine(m);
//        }

//        protected (LightningTransaction, LightningDatabase) BeginReadTransaction(TestDescription td, LightningEnvironment env)
//        {
//            LightningTransaction tx = null;
//            LightningDatabase db = null;

//            var m = td.Measure.MeasureTime("BeginRT", 0, 1, () =>
//            {
//                tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
//                db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
//            });


//            //Console.WriteLine(m);

//            return (tx, db);
//        }

//        protected void CommitReadTransaction(TestDescription td, LightningTransaction tx, LightningDatabase db)
//        {
//            var m = td.Measure.MeasureTime("CommitRT", 0, 1, () =>
//            {
//                tx.Commit();
//            });

//            db.Dispose();
//            tx.Dispose();
//            //Console.WriteLine(m);
//        }

//        protected void RollbackReadTransaction(TestDescription td, LightningTransaction tx, LightningDatabase db)
//        {
//            var m = td.Measure.MeasureTime("RollbackRT", 0, 1, () =>
//            {
//                tx.Abort();
//            });

//            db.Dispose();
//            tx.Dispose();

//            //Console.WriteLine(m);
//        }
//    }
//}
