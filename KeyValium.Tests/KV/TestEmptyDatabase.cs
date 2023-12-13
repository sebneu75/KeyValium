using KeyValium.Pages;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.KV
{
    public class TestEmptyDatabase : IDisposable
    {
        public TestEmptyDatabase()
        {
            pdb = new PreparedKeyValium("EmptyDatabase");
        }

        PreparedKeyValium pdb;

        [Fact]
        public void IterateEmptyDb()
        {
            pdb.CreateNewDatabase(false, false);

            Assert.False(pdb.Database == null);

            using (var tx = pdb.Database.BeginReadTransaction())
            {
                var forward = tx.GetIterator(null, true);

                Assert.False(forward.MoveNext());

                var backward = tx.GetIterator(null, false);

                Assert.False(backward.MoveNext());
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }

    //private void CompareComparers(List<KeyValuePair<byte[], byte[]>> list)
    //{
    //    var comp1 = new KeyComparer();

    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        for (int k = 0; k < list.Count; k++)
    //        {
    //            var key1 = list[i].Key;
    //            var key2 = list[k].Key;

    //            var r1 = comp1.Compare(key1, key2);
    //            var r2 = UniversalComparer.CompareBytes(key1, key2);

    //            if (r1 != r2)
    //            {
    //                Console.WriteLine("Hossa");
    //            }
    //        }
    //    }
    //}

}
