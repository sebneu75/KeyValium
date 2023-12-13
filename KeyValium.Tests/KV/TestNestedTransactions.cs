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
    public sealed class TestNestedTransactions : IDisposable
    {
        public TestNestedTransactions()
        {
            pdb = new PreparedKeyValium("NestedTransactions");
        }

        PreparedKeyValium pdb;

        [Fact]
        public void NestedTransactions()
        {
            pdb.CreateNewDatabase(false, false);

            var key = Encoding.UTF8.GetBytes("Key");
            var value1 = Encoding.UTF8.GetBytes("111");
            var value2 = Encoding.UTF8.GetBytes("222");
            var value3 = Encoding.UTF8.GetBytes("333");
            var value4 = Encoding.UTF8.GetBytes("444");
            var value5 = Encoding.UTF8.GetBytes("555");

            using (var tx0 = pdb.Database.BeginWriteTransaction())
            {
                try
                {
                    tx0.Insert(null, key, value1);

                    Console.WriteLine("tx0: {0}", GetString(tx0, key));
                    using (var tx1 = tx0.BeginChildTransaction())
                    {
                        tx1.Update(null, key, value2);
                        Console.WriteLine("tx1: {0}", GetString(tx1, key));

                        using (var tx2 = tx1.BeginChildTransaction())
                        {
                            tx2.Update(null, key, value3);
                            Console.WriteLine("tx2: {0}", GetString(tx2, key));

                            tx2.Commit();
                        }

                        Console.WriteLine("tx1: {0}", GetString(tx1, key));

                        tx1.Rollback();
                    }

                    Console.WriteLine("tx0: {0}", GetString(tx0, key));

                    tx0.Commit();
                }
                catch (Exception ex)
                {
                    tx0.Rollback();
                    throw;
                }
            }
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



