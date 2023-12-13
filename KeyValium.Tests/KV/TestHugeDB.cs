using KeyValium;
using KeyValium.Cursors;
using KeyValium.TestBench;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.KV
{
    public sealed class TestHugeDB : IDisposable
    {
        public TestHugeDB()
        {
            var td = new TestDescription(nameof(TestHugeDB))
            {
                PageSize = 4096,
                MinKeySize = 8,
                MaxKeySize = 8,
                MinValueSize = 0,
                MaxValueSize = 0,
                KeyCount = 10000000000,
                CommitSize = 1000000,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public unsafe void Test_HugeDB()
        {
            pdb.CreateNewDatabase();

            var bytes = new byte[8];
            long key;

            for (long i = 0; i < pdb.Description.KeyCount; i += pdb.Description.CommitSize)
            {
                using (var tx = pdb.Database.BeginWriteTransaction())
                {
                    tx.AppendMode = true;

                    for (int k = 0; k < pdb.Description.CommitSize; k++)
                    {
                        key = i + k;

                        BinaryPrimitives.WriteInt64BigEndian(bytes, key);
                        tx.Insert(null, bytes, (byte[])null);
                    }

                    tx.Commit();
                }

                Console.WriteLine("Inserted {0}/{1} Keys.", i, pdb.Description.KeyCount);
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

