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
    public sealed class TestCursorAdjustment : IDisposable
    {
        public TestCursorAdjustment()
        {
            // TODO
            //foreach (var pagesize in PageSizes)
            //{
            //    var td = new TestDescription(Name + "-" + pagesize.ToString());
            //    td.Options.PageSize = pagesize;
            //    td.MinKeySize = 8;
            //    td.MaxKeySize = -1;
            //    td.MinValueSize = 16;
            //    td.MaxValueSize = -1;
            //    td.KeyCount = 10000;
            //    td.CommitSize = 1000;
            //    //td.GenStrategy = Util.KeyGenStrategy.Random;
            //    //td.OrderInsert = Util.KeyOrder.Random;
            //    //td.OrderRead = Util.KeyOrder.Random;
            //    //td.OrderDelete = Util.KeyOrder.Ascending;

            //    yield return td;
            //}

            var td = new TestDescription(nameof(TestCursorAdjustment))
            {
                PageSize = 4096,
                MinKeySize = 8,
                MaxKeySize = -1,
                MinValueSize = 16,
                MaxValueSize = -1,
                KeyCount = 10000,
                CommitSize = 1000,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Random,
                OrderRead = KeyOrder.Random,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_CursorAdjustment()
        {
            pdb.CreateNewDatabase(false, false);

            //var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);

            var keycount = 100;
            var cursorcount = 20;

            var cursors = new List<Tuple<byte[], Cursor>>();

            // create keys
            var items = pdb.Description.GenerateKeys(0, keycount);

            // pick some random keys
            items = KeyValueGenerator.Order(items, KeyOrder.Random);

            var ckeys = items.Take(cursorcount).ToList();
            items = items.Skip(cursorcount).ToList();

            var tx = pdb.Database.BeginWriteTransaction();

            try
            {
                // insert cursor keys
                ckeys.ForEach(x => tx.Insert(null, x.Key, x.Value));

                // create cursors to some keys
                ckeys.ForEach(x =>
                {
                    var cursor = tx.GetCursorEx(null, InternalTrackingScope.TransactionChain);
                    var keyspan = new ReadOnlySpan<byte>(x.Key);
                    if (!cursor.SetPositionEx(CursorPositions.Key, ref keyspan))
                    {
                        throw new NotSupportedException("Key not found.");
                    }

                    cursors.Add(new Tuple<byte[], Cursor>(x.Key, cursor));
                });

                // validate cursors
                cursors.ForEach(x => KeyValium.KvDebug.ValidateCursor(x.Item2, null, x.Item1));

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("Cycle {0}", i);

                    //
                    // insert keys
                    //
                    Console.WriteLine("Inserting...");
                    items = KeyValueGenerator.Order(items, KeyOrder.Random);
                    items.ForEach(x =>
                    {
                        tx.Insert(null, x.Key, x.Value);
                        ValidateCursors(tx, cursors);
                    });

                    //
                    // delete keys
                    //
                    Console.WriteLine("Deleting...");
                    items = KeyValueGenerator.Order(items, KeyOrder.Random);
                    items.ForEach(x =>
                    {
                        tx.Delete(null, x.Key);
                        ValidateCursors(tx, cursors);
                    });
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                throw;
            }
        }

        private void ValidateCursors(Transaction tx, List<Tuple<byte[], Cursor>> cursors)
        {
            //Console.WriteLine("Validating...");
            // validate cursors
            cursors.ForEach(x =>
            {
                try
                {
                    // TODO fix
                    KeyValium.KvDebug.ValidateCursor(x.Item2, null, x.Item1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0}]: {1}", TestBench.Tools.GetHexString(x.Item1), ex.Message);

                    var keyspan = new ReadOnlySpan<byte>(x.Item1);
                    var cc = tx.GetCursor(null, InternalTrackingScope.None);                    
                    cc.SetPositionEx(CursorPositions.Key, ref keyspan);

                    var ret = KeyValium.KvDebug.CompareCursors(cc, x.Item2);
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
