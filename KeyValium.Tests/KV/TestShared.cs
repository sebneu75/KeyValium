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
    public sealed class TestShared : IDisposable
    {
        public TestShared()
        {
            var td = new TestDescription(nameof(TestShared))
            {
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 128,
                MaxValueSize = 128,
                KeyCount = 100000,
                CommitSize = 100,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Random,
                OrderRead = KeyOrder.Random,
                OrderDelete = KeyOrder.Random
            };

            td.Options.InternalSharingMode = InternalSharingModes.SharedNetwork;

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_Shared()
        {
            // create empty database
            pdb.CreateNewDatabase(false, false);

            pdb.Database.Dispose();

            ThreadPool.SetMaxThreads(1000, 10000);

            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);
            items = KeyValueGenerator.Order(items, pdb.Description.OrderInsert);

            var threads = 100;

            Assert.True(pdb.Description.KeyCount % threads == 0, "Not evenly divisible!");

            var tasks = new List<Task>();

            for (int i = 0; i < pdb.Description.KeyCount; i += (int)pdb.Description.KeyCount / threads)
            {
                var list = items.Skip(i).Take((int)pdb.Description.KeyCount / threads).ToList();

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

            var rnd = new Random();

            using (var db = Database.Open(dbfile, td.Options))
            {
                for (int i = 0; i < list.Count; i += pdb.Description.CommitSize)
                {
                    var items = list.Skip(i).Take(pdb.Description.CommitSize).ToList();

                    using (var tx = db.BeginWriteTransaction())
                    {
                        foreach (var pair in items)
                        {
                            tx.Insert(null, pair.Key, pair.Value);
                        }

                        tx.Commit();
                    }
                }
            }
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




