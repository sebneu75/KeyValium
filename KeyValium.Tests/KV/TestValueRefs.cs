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
    public sealed class TestValueRefs : IDisposable
    {
        public TestValueRefs()
        {
            var td = new TestDescription("ValueRefs")
            {
                MinKeySize = 8,
                MaxKeySize = 8,
                MinValueSize = 1 * 1024 * 1024,
                MaxValueSize = 10 * 1024 * 1024,
                KeyCount = 1000,
                CommitSize = 32,
                GenStrategy = KeyGenStrategy.Sequential,
                OrderInsert = KeyOrder.Ascending,
                OrderRead = KeyOrder.Ascending,
                OrderDelete = KeyOrder.Ascending
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void TestValueRef()
        {
            pdb.CreateNewDatabase(false, false);

            // create keys
            var items = pdb.Description.GenerateKeys(0, pdb.Description.KeyCount);

            var tx = pdb.Database.BeginWriteTransaction();

            try
            {
                //
                // insert keys
                //
                Console.WriteLine("Inserting...");
                items = KeyValueGenerator.Order(items, KeyOrder.Random);
                items.ForEach(x =>
                {
                    tx.Insert(null, x.Key, x.Value);
                });

                Console.WriteLine("Validating...");
                items.ForEach(x =>
                {
                    var val = tx.Get(null, x.Key);

                    var bytes = val.Value;

                    Assert.True(TestBench.Tools.BytesEqual(x.Value, bytes), "Values not equal!");
                });

                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}



