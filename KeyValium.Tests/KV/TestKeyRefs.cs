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
    public sealed class TestKeyRefs : IDisposable
    {
        public TestKeyRefs()
        {
            var td = new TestDescription(nameof(TestKeyRefs))
            {
                PageSize = 4096,
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

        [Fact]
        public void Test_KeyRefs()
        {
            pdb.CreateNewDatabase(false, false);

            var key1 = new byte[] { 1, 1, 1, 1 };
            var key2 = new byte[] { 2, 2, 2, 2 };
            var key3 = new byte[] { 3, 3, 3, 3 };
            var key4 = new byte[] { 4, 4, 4, 4 };
            var key5 = new byte[] { 5, 5, 5, 5 };

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                tx.Insert(null, key1, (byte[])null);
                tx.Insert(null, key2, (byte[])null);
                tx.Insert(null, key3, (byte[])null);
                tx.Insert(null, key4, (byte[])null);
                tx.Insert(null, key5, (byte[])null);

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                var keyref = tx.EnsureTreeRef(TrackingScope.TransactionChain, 
                    new ReadOnlyMemory<byte>(key1), new ReadOnlyMemory<byte>(key2), 
                    new ReadOnlyMemory<byte>(key3), new ReadOnlyMemory<byte>(key4), 
                    new ReadOnlyMemory<byte>(key5));

                tx.Commit();
            }

            using (var tx = pdb.Database.BeginWriteTransaction())
            {
                var key = new byte[1] { 255 };
                var val = new byte[256];

                for (int i = 0; i < 256; i++)
                {
                    val[i] = (byte)(i & 0xff);
                }

                tx.Upsert(null, key, val);

                key[0]--;
                for (int i = 0; i < 256; i++)
                {
                    val[i] +=3;
                }

                tx.Upsert(null, key, val);

                key[0]--;
                for (int i = 0; i < 8; i++)
                {
                    for(int k=0; k < 8; k++)
                    {
                        val[i*8+k]= (byte)(0x92+i);
                    }                    
                }

                tx.Upsert(null, key, val);

                tx.Commit();
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}
