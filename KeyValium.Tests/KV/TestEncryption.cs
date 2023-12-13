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
    public sealed class TestEncryption : IDisposable
    {
        public TestEncryption()
        {
            pdb = new PreparedKeyValium("Encryption");
        }

        PreparedKeyValium pdb;

        [Fact]
        public void TestEncryptedDb()
        {
            var kv1 = new KeyValuePair<byte[], byte[]>(Encoding.UTF8.GetBytes("Key1"), Encoding.UTF8.GetBytes("Value1"));
            var kv2 = new KeyValuePair<byte[], byte[]>(Encoding.UTF8.GetBytes("Key2"), Encoding.UTF8.GetBytes("Value2"));
            var kv3 = new KeyValuePair<byte[], byte[]>(Encoding.UTF8.GetBytes("Key3"), Encoding.UTF8.GetBytes("Value3"));
            var kv4 = new KeyValuePair<byte[], byte[]>(Encoding.UTF8.GetBytes("Key4"), Encoding.UTF8.GetBytes("Value4"));
            var kv5 = new KeyValuePair<byte[], byte[]>(Encoding.UTF8.GetBytes("Key5"), Encoding.UTF8.GetBytes("Value5"));

            var dbfile = pdb.Description.DbFilename;
            if (File.Exists(dbfile))
            {
                File.Delete(dbfile);
            }

            pdb.Description.Options.Password = "123";

            using (var db = Database.Open(dbfile, pdb.Description.Options))
            {
                using (var tx = db.BeginWriteTransaction())
                {
                    tx.Insert(null, kv1.Key, kv1.Value);
                    tx.Insert(null, kv2.Key, kv2.Value);

                    tx.Commit();
                }
            }

            using (var db = Database.Open(dbfile, pdb.Description.Options))
            {
                using (var tx = db.BeginReadTransaction())
                {
                    var val1 = tx.Get(null, kv1.Key);
                    var val2 = tx.Get(null, kv2.Key);

                    Assert.True(Tools.BytesEqual(val1.Value, kv1.Value));
                    Assert.True(Tools.BytesEqual(val2.Value, kv2.Value));

                    tx.Commit();
                }
            }

            // set wrong password
            pdb.Description.Options.Password = "124";

            Assert.Throws<KeyValiumException>(() =>
                {
                    using (var db = Database.Open(dbfile, pdb.Description.Options))
                    {
                    }
                });

            // set no password
            pdb.Description.Options.Password = null;

            Assert.Throws<KeyValiumException>(() =>
            {
                using (var db = Database.Open(dbfile, pdb.Description.Options))
                {
                }
            });

        }

        private string GetString(Transaction tx, byte[] key)
        {
            var val = tx.Get(null, key);
            var str = Encoding.UTF8.GetString(val.Value);

            return str;
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}



