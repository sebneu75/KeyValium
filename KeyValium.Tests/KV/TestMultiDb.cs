using KeyValium;
using KeyValium.Cursors;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.KV
{
    public sealed class TestMultiDb : IDisposable
    {
        public TestMultiDb()
        {
            var td = new TestDescription(nameof(TestMultiDb))
            {
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 128,
                MaxValueSize = 128,
                KeyCount = 10000,
                CommitSize = 100,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Random,
                OrderRead = KeyOrder.Random,
                OrderDelete = KeyOrder.Random
            };

            td.Options.Shared = true;

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_MultiDb()
        {
            // create empty database
            pdb.CreateNewDatabase(false, false);

            pdb.Database.Dispose();

            ThreadPool.SetMaxThreads(1000, 10000);

            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);
            items = KeyValueGenerator.Order(items, pdb.Description.OrderInsert);

            var tasks = new List<Task>();

            for (int i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
            {
                var list = items.Skip(i).Take(pdb.Description.CommitSize).ToList();

                var task = Task.Run(() => Insert(pdb.Description, list));
                tasks.Add(task);
            }

            Exception error = null;

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                throw;
            }

            using (var db = Database.Open(pdb.Description.DbFilename, pdb.Description.Options))
            {
                var tx = db.BeginReadTransaction();

                var cmp = new KeyComparer();

                foreach (var key in items)
                {
                    var val = tx.Get(null, key.Key);

                    Assert.True(TestBench.Tools.BytesEqual(val.Value, key.Value), "FAIL");
                }

                tx.Commit();
            }

            if (error != null)
            {
                throw error;
            }
        }

        private void Insert(TestDescription td, List<KeyValuePair<byte[], byte[]>> list)
        {
            var dbfile = td.DbFilename;

            using (var db = Database.Open(dbfile, td.Options))
            {
                var tx = db.BeginWriteTransaction();

                Write("Start", tx);

                foreach (var pair in list)
                {
                    tx.Insert(null, pair.Key, pair.Value);
                }

                tx.Commit();

                Write("End", tx);
            }


            //foreach (var pair in list)
            //{
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            //    tx.DeleteKey(pair.Key);
            //}
        }

        private void Write(string msg, Transaction tx)
        {
            Console.WriteLine("{0}: ThreadId: {1}   Tid: {2}   Oid: {3}", msg, Thread.CurrentThread.ManagedThreadId, tx.Tid, tx.Oid);
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}




