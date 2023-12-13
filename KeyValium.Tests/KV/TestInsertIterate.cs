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
    public sealed class TestInsertIterate : IDisposable
    {
        public TestInsertIterate()
        {
            var td = new TestDescription(nameof(TestInsertIterate))
            {
                MinKeySize = 0,
                MaxKeySize = -1,
                MinValueSize = 0,
                MaxValueSize = -1,
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
        public void Test_InsertIterate()
        {
            pdb.CreateNewDatabase(false, false);


            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);

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

            //
            // iterate
            //
            using (var tx = pdb.Database.BeginReadTransaction())
            {
                Iterate(tx, true);
            }

            using (var tx = pdb.Database.BeginReadTransaction())
            {
                Iterate2(tx, true);
            }
        }

        private void InsertItems(Transaction tx, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                tx.Insert(null, items[i].Key, items[i].Value);

                //tx.DumpTree(GetFilename("AfterInsert", items[i]));
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

        private long Iterate2(Transaction tx, bool forward)
        {
            var ret = 0;
            byte[] lastkey = null;

            tx.ForEach(null, item =>
            {
                return true;
            });

            return ret;
        }

        private long Iterate(Transaction tx, bool forward)
        {
            var ret = 0;
            byte[] lastkey = null;

            using (var iter = tx.GetIterator(null, forward))
            {
                //if (forward)
                //{
                    while (iter.MoveNext())
                    {
                        ret++;
                        //var key = iter.CurrentKey();

                        //if (lastkey != null)
                        //{
                        //    var result = UniversalComparer.CompareBytes(lastkey, key);

                        //    Debug.Assert(result < 0, "Order mismatch");
                        //}
                        //var key = Util.GetHexString(cursor.GetCurrentKey());
                        //Console.WriteLine(key);

                        //lastkey = key;
                    }
                //}
                //else
                //{
                //    while (iter.MovePrevious())
                //    {
                //        ret++;
                //        //var key = iter.CurrentKey();

                //        //if (lastkey != null)
                //        //{
                //        //    var result = UniversalComparer.CompareBytes(lastkey, key);

                //        //    Debug.Assert( result > 0, "Order mismatch");
                //        //}
                //        //var key = Util.GetHexString(cursor.GetCurrentKey());
                //        //Console.WriteLine(key);

                //        //lastkey = key;
                //    }
                //}
            }

            //Console.WriteLine("Iterated {0} over {1} items.", forward ? "forwards" : "backwards", ret);

            return ret;
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}

