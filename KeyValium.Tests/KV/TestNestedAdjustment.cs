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
    public sealed class TestNestedAdjustment : IDisposable
    {
        public TestNestedAdjustment()
        {
            var td = new TestDescription(nameof(TestNestedAdjustment))
            {
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 32,
                MaxValueSize = 4000,
                KeyCount = 100,
                CommitSize = 100,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_NestedAdjustment()
        {
            pdb.CreateNewDatabase(false, false);

            //var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);

            var count = pdb.Description.KeyCount;

            // create cursor keys
            var ckeys = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                if (i % 10 == 5)
                {
                    ckeys.Add(Encoding.UTF8.GetBytes(i.ToString("0000")));
                }
            }

            // create keyref keys
            var refkeys = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                if (i % 10 == 4)
                {
                    refkeys.Add(Encoding.UTF8.GetBytes(i.ToString("0000")));
                }
            }

            // create keys
            var keys = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                if (i % 10 != 4 && i % 10 != 5)
                {
                    keys.Add(Encoding.UTF8.GetBytes(i.ToString("000")));
                }
            }

            using (var tx0 = pdb.Database.BeginWriteTransaction())
            {
                // insert data
                try
                {
                    // insert cursor keys 
                    foreach (var key in ckeys)
                    {
                        tx0.Insert(null, key, key);
                    }

                    // insert ref keys 
                    foreach (var key in refkeys)
                    {
                        tx0.Insert(null, key, key);
                    }

                    // create cursors
                    var cursors = new List<Tuple<byte[], Cursor>>();

                    foreach (var key in ckeys)
                    {
                        var cursor = CreateCursor(tx0, key);
                        cursors.Add(new Tuple<byte[], Cursor>(key, cursor));
                    }

                    // create keyrefs
                    var keyrefs = new List<Tuple<byte[], TreeRef>>();
                    foreach (var key in refkeys)
                    {
                        var keyref = tx0.EnsureTreeRef(TrackingScope.TransactionChain, key);
                        keyrefs.Add(new Tuple<byte[], TreeRef>(key, keyref));
                    }

                    using (var tx1 = tx0.BeginChildTransaction())
                    {
                        foreach (var key in keys)
                        {
                            tx1.Insert(null, key, key);
                        }

                        using (var tx2 = tx1.BeginChildTransaction())
                        {
                            foreach (var key in keys)
                            {
                                if (key.Last() % 2 == 0)
                                {
                                    tx2.Delete(null, key);
                                }
                            }
                        }
                    }

                    // validate cursors
                    ValidateCursors(tx0, cursors);

                    // validate keyrefs
                    ValidateCursors(tx0, keyrefs.Select(x => new Tuple<byte[], Cursor>(x.Item1, x.Item2.Cursor)).ToList());

                    tx0.Commit();
                }
                catch (Exception ex)
                {
                    tx0.Rollback();
                    throw;
                }
            }
        }

        private static Cursor CreateCursor(Transaction tx, byte[] key)
        {
            var keyspan = new ReadOnlySpan<byte>(key);
            var cursor = tx.GetCursorEx(null, InternalTrackingScope.TransactionChain);
            if (!cursor.SetPositionEx(CursorPositions.Key, ref keyspan))
            {
                throw new NotSupportedException("Key not found.");
            }
            return cursor;
        }

        private void ValidateCursors(Transaction tx, List<Tuple<byte[], Cursor>> cursors)
        {
            //Console.WriteLine("Validating...");
            // validate cursors
            cursors.ForEach(x =>
            {
                try
                {
                    KeyValium.KvDebug.ValidateCursor(x.Item2, null, x.Item1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0}]: {1}", TestBench.Tools.GetHexString(x.Item1), ex.Message);

                    var keyspan = new ReadOnlySpan<byte>(x.Item1);

                    var cc = tx.GetCursor(null, InternalTrackingScope.None);
                    cc.SetPositionEx(CursorPositions.Key, ref keyspan);

                    var ret = KeyValium.KvDebug.CompareCursors(cc, x.Item2);
                    Assert.True(ret.Item1, "Cursor mismatch!");
                    Console.WriteLine(ret);
                }
            });
        }


        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}
