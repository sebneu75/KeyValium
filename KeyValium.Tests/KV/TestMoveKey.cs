using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace KeyValium.Tests.KV
{
    public sealed class TestMoveKey : IDisposable
    {
        public TestMoveKey()
        {
            var td = new TestDescription(nameof(TestMoveKey))
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
        public void Test_MoveKey()
        {
            var items = pdb.Description.GenerateKeys(0, 10);

            CreateTrees(items, 5);

            // move a key
            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                // get keys
                var keylist1 = new ReadOnlyMemory<byte>[3];
                keylist1[0] = items[1].Key.AsMemory();
                keylist1[1] = items[2].Key.AsMemory();
                keylist1[2] = items[3].Key.AsMemory();
                var sourcekey = items[4].Key;

                var keylist2 = new ReadOnlyMemory<byte>[3];
                keylist2[0] = items[1].Key.AsMemory();
                keylist2[1] = items[2].Key.AsMemory();
                keylist2[2] = items[3].Key.AsMemory();
                var targetkey = Encoding.UTF8.GetBytes("Moved");

                var keylist3 = new ReadOnlyMemory<byte>[4];
                keylist3[0] = items[1].Key.AsMemory();
                keylist3[1] = items[2].Key.AsMemory();
                keylist3[2] = items[3].Key.AsMemory();
                keylist3[3] = items[4].Key.AsMemory();

                using (var treeref1 = tx.EnsureTreeRef(TrackingScope.TransactionChain, keylist1))
                using (var treeref2 = tx.EnsureTreeRef(TrackingScope.TransactionChain, keylist2))
                using (var treeref3 = tx.EnsureTreeRef(TrackingScope.TransactionChain, keylist3))
                {
                    TreeRef targetref = null; // treeref2

                    Assert.True(treeref3.State == TreeRefState.Active);

                    var oldtc = tx.GetTotalCount(treeref1);
                    var oldlc = tx.GetLocalCount(treeref1);

                    var difftc = tx.GetTotalCount(treeref3);
                    var difflc = tx.GetLocalCount(treeref3);

                    var oldval = tx.Get(treeref1, sourcekey).Value;
                    tx.Move(treeref1, sourcekey, targetref, targetkey);

                    var newval = tx.Get(targetref, targetkey).Value;

                    var newtc = tx.GetTotalCount(treeref1);
                    var newlc = tx.GetLocalCount(treeref1);

                    Assert.True(TestBench.Tools.BytesEqual(oldval, newval), "Values not equal!");

                    Assert.True(treeref3.State == TreeRefState.Inactive);
                }
                
                tx.Commit();
            }
        }

        private void CreateTrees(List<KeyValuePair<byte[], byte[]>> items, int levels)
        {
            pdb.CreateNewDatabase(false, false);

            //
            // create trees
            //
            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                for (int i = 0; i < levels; i++)
                {
                    InsertLevel(tx, i, items);
                }

                tx.Commit();
            }





            //var state = new int[levels];

            ////
            //// create trees
            ////
            //using (var tx = pdb.Database.BeginWriteTransaction())
            //{
            //    // determine current keys


            //    tx.EnsureTreeRef(TrackingScope.TransactionChain, ).Insert(null, items[i].Key, items[i].Value);

            //    InsertItems(tx, list);

            //    tx.Commit();
            //}

            //list = KeyValueGenerator.Order(items, pdb.Description.OrderRead);
            ////
            //// read
            ////
            //using (var tx = pdb.Database.BeginReadTransaction())
            //{
            //    ReadItems(tx, list);
            //}

            //list = KeyValueGenerator.Order(items, pdb.Description.OrderDelete);
            //for (int i = 0; i < list.Count; i++)
            //{
            //    //
            //    // delete
            //    //
            //    using (var tx = pdb.Database.BeginWriteTransaction())
            //    {
            //        DeleteItems(tx, list.Skip(i).Take(1).ToList());

            //        tx.Commit();
            //    }

            //    // check remaining keys
            //    for (int k = i; k < list.Count; k++)
            //    {
            //        //
            //        // read
            //        //
            //        using (var tx = pdb.Database.BeginReadTransaction())
            //        {
            //            ReadItems(tx, list.Skip(k + 1).Take(1).ToList());
            //        }
            //    }
            //}
        }

        private void InsertLevel(Transaction tx, int level, List<KeyValuePair<byte[], byte[]>> items)
        {
            if (level == 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    tx.Insert(null, items[i].Key, items[i].Value);
                }
            }
            else
            {
                var done = false;
                var state = new int[level];

                while (!done)
                {
                    // get keys
                    var keylist = new ReadOnlyMemory<byte>[level];
                    for (int i = 0; i < level; i++)
                    {
                        keylist[i] = items[state[i]].Key.AsMemory();
                    }

                    using (var treeref = tx.EnsureTreeRef(TrackingScope.TransactionChain, keylist))
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            tx.Insert(treeref, items[i].Key, items[i].Value);
                        }
                    }

                    // increment state
                    state[0]++;

                    for (int i = 0; i < level; i++)
                    {
                        if (state[i] >= items.Count)
                        {
                            state[i] = 0;
                            if (i < level - 1)
                            {
                                state[i + 1]++;
                            }
                            else
                            {
                                done = true;
                            }
                        }
                        else
                        {
                            break;
                        }
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
