using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    public class PreparedKeyValium : PreparedDatabase
    {
        public PreparedKeyValium()
            : this("Unnamed")
        {
        }

        public PreparedKeyValium(string name) : base(name)
        {
        }

        public PreparedKeyValium(TestDescription description) : base(description)
        {
        }

        private KeyValium.Database _db;

        bool AppendMode = true;

        public KeyValium.Database Database
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

        public void CreateNewDatabase()
        {
            Database?.Dispose();

            var dbfile = Description.DbFilename;
            var lockfile = dbfile + ".lock";

            if (File.Exists(dbfile))
            {
                File.Delete(dbfile);
            }

            if (File.Exists(lockfile))
            {
                File.Delete(lockfile);
            }

            Database = Database.Open(dbfile, Description.Options);
        }

        public override void CreateNewDatabase(bool forcecreatekeys, bool insertkeys, KeyOrder order = KeyOrder.Ascending, int skip = 0)
        {
            Database?.Dispose();

            var dbfile = Description.DbFilename;
            var lockfile = dbfile + ".lock";

            if (File.Exists(dbfile))
            {
                File.Delete(dbfile);
            }

            if (File.Exists(lockfile))
            {
                File.Delete(lockfile);
            }

            Database = Database.Open(dbfile, Description.Options);

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

                using (var tx = Database.BeginWriteTransaction())
                {
                    tx.AppendMode = AppendMode;

                    for (int i = 0; i < list.Count; i++)
                    {
                        tx.Insert(null, list[i].Key, list[i].Value);
                        i += skip;
                    }

                    tx.Commit();
                }
            }
        }

        public override void OpenDatabase()
        {
            Database?.Dispose();

            var dbfile = Description.DbFilename;
            var lockfile = dbfile + ".lock";

            Database = Database.Open(dbfile, Description.Options);
        }

        internal List<KeyValuePair<byte[], byte[]>> GeneratedKeys;

        internal List<KeyValuePair<byte[], byte[]>> QueuedKeys;

        public List<KeyValuePair<byte[], byte[]>> CurrentBatch;

        public Transaction CurrentTransaction;

        public override void PrepareBeginRT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginReadTransaction())
                {
                }
            }
        }

        public override void FinishBeginRT()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareCommitRT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginReadTransaction())
                {
                }
            }

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishCommitRT()
        {
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareRollbackRT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginReadTransaction())
                {
                }
            }

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishRollbackRT()
        {
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareBeginWT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginWriteTransaction())
                {
                }
            }
        }

        public override void FinishBeginWT()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareCommitWT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginWriteTransaction())
                {
                }
            }

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishCommitWT()
        {
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareRollbackWT()
        {
            CreateNewDatabase(false, true);

            for (int i = 0; i < 100; i++)
            {
                using (var tx = Database.BeginWriteTransaction())
                {
                }
            }

            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void FinishRollbackWT()
        {
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public override void PrepareInsert()
        {
            CreateNewDatabase(false, false, Description.OrderInsert);

            CurrentTransaction = Database.BeginWriteTransaction();

            CurrentTransaction.AppendMode = AppendMode;
        }

        public override void FinishInsert()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            CurrentBatch = null;
        }

        public override void PrepareUpdate()
        {
            CreateNewDatabase(false, true, Description.OrderUpdate);

            CurrentTransaction = Database.BeginWriteTransaction();
            CurrentTransaction.AppendMode = AppendMode;
        }

        public override void FinishUpdate()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            CurrentBatch = null;
        }

        public void PrepareUpsert()
        {
            CreateNewDatabase(false, true, Description.OrderUpdate, 1);

            CurrentTransaction = Database.BeginWriteTransaction();
            CurrentTransaction.AppendMode = AppendMode;
        }

        public void FinishUpsert()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            CurrentBatch = null;
        }

        public override void PrepareDelete()
        {
            CreateNewDatabase(false, true, Description.OrderDelete);

            CurrentTransaction = Database.BeginWriteTransaction();
            CurrentTransaction.AppendMode = AppendMode;
        }

        public override void FinishDelete()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            CurrentBatch = null;
        }

        public override void PrepareRead()
        {
            CreateNewDatabase(false, true, Description.OrderRead);

            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void FinishRead()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            CurrentBatch = null;
        }

        public override void BeginRT()
        {
            CurrentTransaction = Database.BeginReadTransaction();
        }

        public override void CommitRT()
        {
            CurrentTransaction.Commit();
        }

        public override void RollbackRT()
        {
            CurrentTransaction.Rollback();
        }

        public override void BeginWT()
        {
            CurrentTransaction = Database.BeginWriteTransaction();
        }

        public override void CommitWT()
        {
            CurrentTransaction.Commit();
        }

        public override void RollbackWT()
        {
            CurrentTransaction.Rollback();
        }

        public override void Insert()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Insert(null, list[i].Key, list[i].Value);
            }
        }

        public void Upsert()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Upsert(null, list[i].Key, list[i].Value);
            }
        }

        public override void Update()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Update(null, list[i].Key, list[i].Value);
            }
        }

        public override void Delete()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                tx.Delete(null, list[i].Key);
            }
        }

        public override void Read()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            for (int i = 0; i < list.Count; i++)
            {
                var val = tx.Get(null, list[i].Key);
            }

            Console.WriteLine("Read Count={0}", list.Count);
        }

        public override void Seek()
        {
            var tx = CurrentTransaction;
            var list = CurrentBatch;

            using (var cursor = tx.GetCursor(null, InternalTrackingScope.None))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var keyspan = new ReadOnlySpan<byte>(list[i].Key);
                    var result = cursor.SetPositionEx(CursorPositions.Key, ref keyspan);
                }
            }
        }

        public override void ItForward()
        {
            var tx = CurrentTransaction;

            using (var iter = tx.GetIterator(null, true))
            {
                foreach (var item in iter)
                {
                    var key = item.Value.Key;
                    var val = item.Value.Value;
                }
            }
        }

        public override void ItBackward()
        {
            var tx = CurrentTransaction;

            using (var iter = tx.GetIterator(null, false))
            {
                foreach (var item in iter)
                {
                    var key = item.Value.Key;
                    var val = item.Value.Value;
                }
            }
        }

        public void ForEachDelegate()
        {
            var tx = CurrentTransaction;

            bool KeyValueIterator(ref ValueRef item)
            {
                var key = item.Key;
                var val = item.Value;

                return true;
            }

            tx.ForEach(null, KeyValueIterator);
        }

        public void ForEachLambda()
        {
            var tx = CurrentTransaction;

            tx.ForEach(null, item =>
            {
                var key = item.Value.Key;
                var val = item.Value.Value;

                return true;
            });
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
