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
    public class TestNullValues : IDisposable
    {
        public TestNullValues()
        {
            pdb = new PreparedKeyValium("NullValues");
        }

        PreparedKeyValium pdb;

        [Fact]
        public void NullValues()
        {
            var items = new List<Tuple<byte[], byte[]>>();

            items.Add(new Tuple<byte[], byte[]>(null, null));
            items.Add(new Tuple<byte[], byte[]>(null, new byte[0]));
            items.Add(new Tuple<byte[], byte[]>(new byte[0], null));
            items.Add(new Tuple<byte[], byte[]>(new byte[0], new byte[0]));

            pdb.CreateNewDatabase(false, false);

            Assert.False(pdb.Database == null);

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                foreach (var item in items)
                {
                    if (tx.Exists(null, item.Item1))
                    {
                        Assert.True(tx.Delete(null, item.Item1), "Key not deleted!");
                    }

                    tx.Insert(null, item.Item1, item.Item2);

                    ReadKey(tx, item.Item1, null, true);

                    Assert.True(tx.Delete(null, item.Item1), "Key not deleted!");

                    ReadKey(tx, item.Item1, null, false);

                    tx.Insert(null, item.Item1, item.Item2);

                    ReadKey(tx, item.Item1, null, true);

                    tx.Update(null, item.Item1, item.Item2);

                    ReadKey(tx, item.Item1, null, true);

                    tx.Upsert(null, item.Item1, item.Item2);

                    ReadKey(tx, item.Item1, null, true);
                }

                tx.Commit();
            }
        }

        private void ReadKey(Transaction tx, byte[] key, byte[] expectedval, bool shouldexist)
        {
            if (shouldexist)
            {
                Assert.True(tx.Exists(null, key), "Key should exist!");

                var val = tx.Get(null, key);

                Assert.True(MemoryExtensions.SequenceEqual<byte>(expectedval, val.Value), "Value mismatch!");
            }
            else 
            {
                Assert.False(tx.Exists(null, key), "Key should not exist!");
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
                    KvDebug.ValidateCursor(x.Item2, null, x.Item1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0}]: {1}", TestBench.Tools.GetHexString(x.Item1), ex.Message);

                    var keyspan = new ReadOnlySpan<byte>(x.Item1);
                    var cc = tx.GetCursor(null, InternalTrackingScope.None);
                    cc.SetPositionEx(CursorPositions.Key, ref keyspan);

                    var ret = KvDebug.CompareCursors(cc, x.Item2);
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



