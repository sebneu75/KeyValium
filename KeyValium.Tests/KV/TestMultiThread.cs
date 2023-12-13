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
    public sealed class TestMultiThread : IDisposable
    {
        public TestMultiThread()
        {
            var td = new TestDescription(nameof(TestMultiThread))
            {
                PageSize = 4096,
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 128,
                MaxValueSize = 128,
                KeyCount = 10000,
                CommitSize = 1000,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Random,
                OrderRead = KeyOrder.Random,
                OrderDelete = KeyOrder.Random
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_MultiThread()
        {
            pdb.CreateNewDatabase(false, false);

            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);
            
            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                items = KeyValueGenerator.Order(items, pdb.Description.OrderInsert);

                var tasks = new List<Task>();

                for (int i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
                {
                    var list = items.Skip(i).Take(pdb.Description.CommitSize).ToList();

                    var task = Task.Run(() => InsertDelete(tx, list));
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginReadTransaction())
            {
                foreach (var key in items)
                {
                    var val = tx.Get(null, key.Key);

                    Assert.True(TestBench.Tools.BytesEqual(val.Value, key.Value), "FAIL");
                }
            }
        }

        private void InsertDelete(Transaction tx, List<KeyValuePair<byte[], byte[]>> list)
        {
            foreach (var pair in list)
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                tx.Insert(null, pair.Key, pair.Value);
            }

            //foreach (var pair in list)
            //{
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            //    tx.DeleteKey(pair.Key);
            //}
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}




