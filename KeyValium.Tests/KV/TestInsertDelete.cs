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
    public sealed class TestInsertDelete : IDisposable
    {
        public TestInsertDelete()
        {
            var td = new TestDescription(nameof(TestInsertDelete))
            {
                PageSize = 4096,
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 16,
                MaxValueSize = 16,
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
        public void Test_InsertDelete()
        {
            pdb.CreateNewDatabase(false, false);

            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);
            var tid = 0;

            var list = KeyValueGenerator.Order(items, pdb.Description.OrderInsert);
            for (int i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
            {
                //
                // insert
                //
                using (var tx = pdb.Database.BeginWriteTransaction())
                {
                    InsertItems(tx, list.Skip(i).Take(pdb.Description.CommitSize).ToList());

                    tx.Commit();
                }
            }

            list = KeyValueGenerator.Order(items, pdb.Description.OrderRead);
            for (int i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
            {
                //
                // read
                //
                using (var tx = pdb.Database.BeginReadTransaction())
                {
                    ReadItems(tx, list.Skip(i).Take(pdb.Description.CommitSize).ToList());
                }
            }

            list = KeyValueGenerator.Order(items, pdb.Description.OrderDelete);
            for (int i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
            {
                //
                // delete
                //
                using (var tx = pdb.Database.BeginWriteTransaction())
                {
                    DeleteItems(tx, list.Skip(i).Take(pdb.Description.CommitSize).ToList());

                    tx.Commit();
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



