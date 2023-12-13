using KeyValium;
using KeyValium.Cursors;
using KeyValium.Exceptions;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.KV
{
    public sealed class TestSuspendedKeyRef : IDisposable
    {
        public TestSuspendedKeyRef()
        {
            var td = new TestDescription(nameof(TestKeyRefs))
            {
                PageSize = 256,
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 48,
                MaxValueSize = 48,
                KeyCount = 1000,
                CommitSize = 100,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        /// <summary>
        /// checks that the keyref is available after suspension and resurrection
        /// </summary>
        /// <exception cref="Exception"></exception>
        [Fact]
        public void Test_KeyRef1()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            TreeRef keyref = null;

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                keyref = tx.EnsureTreeRef(TrackingScope.Database, key3);
                //var x = keyref.PageNumber;
                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }
        }

        /// <summary>
        /// checks that the keyref is gone after rollback
        /// </summary>
        /// <exception cref="Exception"></exception>
        [Fact]
        public void Test_KeyRef2()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            TreeRef keyref = null;

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                keyref = tx.EnsureTreeRef(TrackingScope.Database, key3);
                //var x = keyref.PageNumber;
                tx.Rollback();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                Assert.Throws<ObjectDisposedException>(()=> keyref.Validate(tx));

                tx.Commit();
            }
        }

        /// <summary>
        /// checks that the keyref is promoted upwards the transaction chain
        /// </summary>
        /// <exception cref="Exception"></exception>
        [Fact]
        public void Test_KeyRef3()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            TreeRef keyref = null;

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                using (var txchild = tx.BeginChildTransaction())
                {
                    keyref = txchild.EnsureTreeRef(TrackingScope.Database, key3);
                    //var x = keyref.PageNumber;
                    txchild.Commit();
                }

                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }
        }

        /// <summary>
        /// checks that the keyref is restored after rollback of child transaction
        /// </summary>
        /// <exception cref="Exception"></exception>
        [Fact]
        public void Test_KeyRef4()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            TreeRef keyref = null;

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                keyref = tx.EnsureTreeRef(TrackingScope.Database, key3);

                using (var txchild = tx.BeginChildTransaction())
                {
                    txchild.Delete(null, key3);

                    Assert.Throws<KeyValiumException>(() => keyref.Validate(txchild));

                    //var x = keyref.PageNumber;
                    txchild.Rollback();
                }

                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }
        }

        /// <summary>
        /// checks that the keyref is restored after rollback of root transaction
        /// </summary>
        /// <exception cref="Exception"></exception>
        [Fact]
        public void Test_KeyRef5()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            TreeRef keyref = null;

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                keyref = tx.EnsureTreeRef(TrackingScope.Database, key3);

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                ValidateKeyRef(tx, keyref, key3);
                tx.Delete(null, key3);

                Assert.Throws<KeyValiumException>(() => keyref.Validate(tx));

                tx.Rollback();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                ValidateKeyRef(tx, keyref, key3);

                tx.Commit();
            }
        }

        private void ValidateKeyRef(Transaction tx, TreeRef keyref, params ReadOnlyMemory<byte>[] keys)
        {
            keyref.Validate(tx);

            var keyref2 = tx.GetTreeRef(TrackingScope.Database, keys);

            // validate cursors
            var result = KeyValium.KvDebug.CompareCursors(keyref.Cursor, keyref2.Cursor);

            if (!result.Item1)
            {
                throw new Exception(result.Item2);
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}
