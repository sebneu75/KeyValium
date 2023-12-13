using LightningDB;
using LightningDB.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    public class PreparedLmdb : PreparedDatabase
    {
        public PreparedLmdb()
            : this("Unnamed")
        {
        }

        public PreparedLmdb(string name) : base(name)
        {
        }

        public PreparedLmdb(TestDescription description) : base(description)
        {
        }

        private LightningEnvironment _db;

        public LightningEnvironment Database
        {
            get
            {
                return _db;
            }
            private set
            {
                if (!ReferenceEquals(_db, value))
                {
                    _db?.Dispose();
                    _db = value;
                }
            }
        }

        public override void CreateNewDatabase(bool forcecreatekeys, bool insertkeys, KeyOrder order = KeyOrder.Ascending, int skip = 0)
        {
            Database?.Dispose();

            var dbfile = Description.DbFilename+".lmdb";
            var lockfile = dbfile + ".lock";

            if (File.Exists(dbfile))
            {
                File.Delete(dbfile);
            }

            if (File.Exists(lockfile))
            {
                File.Delete(lockfile);
            }

            var cfg = new EnvironmentConfiguration();
            cfg.MaxDatabases = 2;
            cfg.MapSize = 512 * 1024 * 1024; // 512MB

            Database = new LightningEnvironment(dbfile, cfg);
            Database.Open(EnvironmentOpenFlags.NoSubDir | EnvironmentOpenFlags.NoReadAhead | EnvironmentOpenFlags.NoSync);

            if (forcecreatekeys || GeneratedKeys == null || GeneratedKeys.Count == 0)
            {
                GeneratedKeys = Description.GenerateKeys(0, Description.KeyCount);
            }

            if (forcecreatekeys || QueuedKeys == null || QueuedKeys.Count == 0)
            {
                QueuedKeys = KeyValueGenerator.Order(GeneratedKeys, order);
            }

            if (forcecreatekeys || CurrentBatch == null || CurrentBatch.Count == 0)
            {
                // currently all keys are inserted at once (KeyCount should be CommitSize)
                CurrentBatch = QueuedKeys.Take((int)Description.KeyCount).ToList();
                QueuedKeys = QueuedKeys.Skip((int)Description.KeyCount).ToList();
            }

            if (insertkeys)
            {
                var list = KeyValueGenerator.Order(GeneratedKeys, KeyOrder.Ascending);

                var tx = Database.BeginWriteTransaction();

                for (int i = 0; i < list.Count; i++)
                {
                    tx.Item1.Put(tx.Item2, list[i].Key, list[i].Value);
                }
                
                tx.Item1.Commit();
                tx.Item1.Dispose();
                tx.Item2.Dispose();
            }
        }

        public override void OpenDatabase()
        {
            Database?.Dispose();

            var dbfile = Description.DbFilename + ".lmdb";
            var lockfile = dbfile + ".lock";

            var cfg = new EnvironmentConfiguration();
            cfg.MaxDatabases = 2;
            cfg.MapSize = 512 * 1024 * 1024; // 512MB

            Database = new LightningEnvironment(dbfile, cfg);
            Database.Open(EnvironmentOpenFlags.NoSubDir | EnvironmentOpenFlags.NoReadAhead | EnvironmentOpenFlags.NoSync);
        }

        private List<KeyValuePair<byte[], byte[]>> GeneratedKeys;

        private List<KeyValuePair<byte[], byte[]>> QueuedKeys;

        public List<KeyValuePair<byte[], byte[]>> CurrentBatch;

        public (LightningTransaction, LightningDatabase) CurrentTransaction;

        public override void PrepareBeginRT()
        {
            CreateNewDatabase(false, true);
        }

        public override void FinishBeginRT()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);
        }

        public override void PrepareCommitRT()
        {
            CreateNewDatabase(false, true);

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishCommitRT()
        {
            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);
        }

        public override void PrepareRollbackRT()
        {
            CreateNewDatabase(false, true);

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishRollbackRT()
        {
            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);
        }

        public override void PrepareBeginWT()
        {
            CreateNewDatabase(false, true);
        }

        public override void FinishBeginWT()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);

        }

        public override void PrepareCommitWT()
        {
            CreateNewDatabase(false, true);

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishCommitWT()
        {
            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);
        }

        public override void PrepareRollbackWT()
        {
            CreateNewDatabase(false, true);

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishRollbackWT()
        {
            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);
        }

        public override void PrepareInsert()
        {
            CreateNewDatabase(false, false, Description.OrderInsert);

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishInsert()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);

            CurrentBatch = null;
        }

        public override void PrepareUpdate()
        {
            CreateNewDatabase(false, true, Description.OrderUpdate);

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishUpdate()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);

            CurrentBatch = null;
        }

        public override void PrepareDelete()
        {
            CreateNewDatabase(false, true, Description.OrderDelete);

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishDelete()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);

            CurrentBatch = null;
        }

        public override void PrepareRead()
        {
            CreateNewDatabase(false, true, Description.OrderRead);

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishRead()
        {
            CurrentTransaction.Item1.Commit();

            CurrentTransaction.Item1.Dispose();
            CurrentTransaction.Item2.Dispose();
            CurrentTransaction = (null, null);

            CurrentBatch = null;
        }




        public override void BeginRT()
        {
            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void CommitRT()
        {
            CurrentTransaction.Item1.Commit();
        }

        public override void RollbackRT()
        {
            CurrentTransaction.Item1.Abort();
        }

        public override void BeginWT()
        {
            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void CommitWT()
        {
            CurrentTransaction.Item1.Commit();
        }

        public override void RollbackWT()
        {
            CurrentTransaction.Item1.Abort();
        }

        public override void Insert()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Item1.Put(tx.Item2, list[i].Key, list[i].Value);
            }
        }

        public override void Update()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Item1.Put(tx.Item2, list[i].Key, list[i].Value);
            }
        }

        public override void Delete()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Item1.Delete(tx.Item2, list[i].Key);
            }
        }

        public override void Read()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                var (resultCode, key, mdbval) = tx.Item1.Get(tx.Item2, list[i].Key);
                //var val = mdbval.CopyToNewArray();
            }
        }

        public override void Seek()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            var iter = tx.Item1.CreateCursor(tx.Item2);

            for (int i = 0; i < list.Count; i++)
            {
                var result = iter.SetKey(list[i].Key);
            }
        }

        public override void ItForward()
        {
            var tx = CurrentTransaction;

            using (var iter = tx.Item1.CreateCursor(tx.Item2))
            {
                while (iter.Next() == MDBResultCode.Success)
                {
                    var (resultCode, mdbkey, mdbval) = iter.GetCurrent();
                    var key = mdbkey.CopyToNewArray();
                    var val = mdbval.CopyToNewArray();
                }
            }
        }

        public override void ItBackward()
        {
            var tx = CurrentTransaction;

            using (var iter = tx.Item1.CreateCursor(tx.Item2))
            {
                iter.Last();

                while (iter.Previous() == MDBResultCode.Success)
                {
                    var (resultCode, mdbkey, mdbval) = iter.GetCurrent();
                    var key = mdbkey.CopyToNewArray();
                    var val = mdbval.CopyToNewArray();

                }
            }
        }

        #region IDisposable

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Database?.Dispose();
                    Database = null;
                }

                disposedValue = true;
            }

            base.Dispose(disposing);
        }

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
