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
    public sealed class TestDelete : IDisposable
    {
        public TestDelete()
        {
            var td = new TestDescription(nameof(TestDelete))
            {
                PageSize = 256,
                MinKeySize = 1,
                MaxKeySize = 1,
                MinValueSize = 32,
                MaxValueSize = 32,
                KeyCount = 10,
                CommitSize = 1,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_Delete()
        {
            pdb.CreateNewDatabase(false, false);

            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);
            var tid = 0;

            var list = KeyValueGenerator.Order(items, pdb.Description.OrderInsert);

            //
            // insert
            //
            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                InsertItems(tx, list);

                tx.Commit();
            }

            list = KeyValueGenerator.Order(items, pdb.Description.OrderRead);
            //
            // read
            //
            using (var tx = pdb.Database.BeginReadTransaction())
            {
                ReadItems(tx, list);
            }

            list = KeyValueGenerator.Order(items, pdb.Description.OrderDelete);
            for (int i = 0; i < list.Count; i++)
            {
                //
                // delete
                //
                using (var tx = pdb.Database.BeginWriteTransaction())
                {
                    DeleteItems(tx, list.Skip(i).Take(1).ToList());

                    tx.Commit();
                }

                // check remaining keys
                for (int k = i; k < list.Count; k ++)
                {
                    //
                    // read
                    //
                    using (var tx = pdb.Database.BeginReadTransaction())
                    {
                        ReadItems(tx, list.Skip(k+1).Take(1).ToList());
                    }
                }
            }
        }

        private void InsertItems(Transaction tx, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                tx.Insert(null, items[i].Key, items[i].Value);
            }
        }

        private void ReadItems(Transaction tx, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var val = tx.Get(null, items[i].Key);
                Assert.True(TestBench.Tools.BytesEqual(val.Value, items[i].Value), "Value mismatch!");
            }
        }

        private void DeleteItems(Transaction tx, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var ret = tx.Delete(null, items[i].Key);
                Assert.True(ret, "Key not deleted.");
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}



