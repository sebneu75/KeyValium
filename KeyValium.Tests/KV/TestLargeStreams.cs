using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Tests.KV
{
    public sealed class TestLargeStreams : IDisposable
    {
        public TestLargeStreams()
        {
            var td = new TestDescription(nameof(TestLargeStreams))
            {
                PageSize = 4096,
                MinKeySize = 16,
                MaxKeySize = 16,
                MinValueSize = 16,
                MaxValueSize = 16,
                KeyCount = 16,
                CommitSize = 1,
                GenStrategy = KeyGenStrategy.Random,
                OrderInsert = KeyOrder.Random,
                OrderRead = KeyOrder.Random,
                OrderDelete = KeyOrder.Random
            };

            pdb = new PreparedKeyValium(td);
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void Test_LargeStreams()
        {
            var filenames = new List<string>();

            var sizemb = 3072;

            for (var i = 0; i < pdb.Description.KeyCount; i++)
            {
                var name = Path.Combine(TestDescription.WorkingPath, string.Format("largefile{0:00}.dat", i));
                EnsureTestFile(name, sizemb + i * 128);
                filenames.Add(name);
            }

            pdb.CreateNewDatabase(false, false);
            foreach (var name in filenames)
            {
                using (var reader = new FileStream(name, FileMode.Open))
                {
                    using (var tx = pdb.Database.BeginWriteTransaction())
                    {
                        tx.Insert(null, Encoding.UTF8.GetBytes(name), reader, reader.Length);
                        tx.Commit();
                    }
                }
            }


            pdb.OpenDatabase();


            foreach (var name in filenames)
            {
                var buffer1 = new byte[1024 * 1024].AsSpan();
                var buffer2 = new byte[1024 * 1024].AsSpan();

                using (var reader = new FileStream(name, FileMode.Open))
                {
                    using (var tx = pdb.Database.BeginReadTransaction())
                    {
                        var val = tx.Get(null, Encoding.UTF8.GetBytes(name));
                        var dbstream = val.ValueStream;

                        Assert.Equal(reader.Length, dbstream.Length);

                        var mb = reader.Length / 1024 / 1024;

                        for (int i = 0; i < mb; i++)
                        {
                            var r1 = reader.Read(buffer1);
                            var r2 = dbstream.Read(buffer2);

                            Assert.Equal(r1, r2);

                            Assert.True(buffer1.SequenceEqual(buffer2));
                        }

                        tx.Commit();
                    }
                }
            }
        }

        private void EnsureTestFile(string path, int sizemb)
        {
            if (File.Exists(path))
            {
                return;
            }

            var rnd = new Random(sizemb);
            var buffer = new byte[1024 * 1024];

            using (var writer = new FileStream(path, FileMode.CreateNew))
            {
                for (int i = 0; i < sizemb; i++)
                {
                    rnd.NextBytes(buffer);
                    writer.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }
}

