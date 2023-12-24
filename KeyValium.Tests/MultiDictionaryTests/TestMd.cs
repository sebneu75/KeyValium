using KeyValium.Cursors;
using KeyValium.Frontends.Serializers;
using KeyValium.Frontends;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.MultiDictionaryTests
{
    public sealed class TestMd : IDisposable
    {
        public TestMd()
        {
            pdb = new PreparedKeyValium("TestMD");
        }

        readonly PreparedKeyValium pdb;

        [Fact]
        public void TestValueRef()
        {
            if (File.Exists(pdb.Description.DbFilename))
            {
                File.Delete(pdb.Description.DbFilename);
            }

            using (var md = KvMultiDictionary.Open(pdb.Description.DbFilename, pdb.Description.Options))
            {
                ulong count = 1000;

                var d1 = md.EnsureDictionary<string, string>("Dict1");
                var d2 = md.EnsureDictionary<ulong, string>("Dict2");
                var d3 = md.EnsureDictionary<ulong, Dummy>("Dict3");
                var d4 = md.EnsureDictionary<ulong, byte[]>("Dict4");

                var list1 = new Dictionary<string, string>();
                var list2 = new Dictionary<ulong, string>();
                var list3 = new Dictionary<ulong, Dummy>();

                for (ulong i = 0; i < count; i++)
                {
                    list1.Add(i.ToString(), string.Format("{0}{0}{0}{0}{0}{0}{0}{0}", i));
                    list2.Add(i, string.Format("{0}{0}{0}{0}{0}{0}{0}{0}", i));

                    var dummy = Dummy.GetRandomDummy();
                    list3.Add(dummy.Id, dummy);
                }

                var list = md.GetDictionaryInfos();

                foreach (var item in list1)
                {
                    d1[item.Key] = item.Value;
                }

                foreach (var item in list2)
                {
                    d2[item.Key] = item.Value;
                }

                d3.DoInTransaction(() =>
                {
                    foreach (var item in list3)
                    {
                        d3[item.Key] = item.Value;
                    }
                });

                d3.DoInTransaction(() =>
                {
                    for (ulong i = 1; i <= 1000; i++)
                    {
                        var x1 = d3[i];
                        Console.WriteLine(x1);
                    }
                });

                Assert.True(d1.LongCount == count, "Count mismatch!");
                foreach (var pair in d1)
                {
                    Assert.True(list1[pair.Key] == pair.Value);
                    Assert.True(list1.Remove(pair.Key));
                }
                Assert.True(list1.Count == 0);

                foreach (var key in d2.Keys)
                {
                    Assert.True(list2.ContainsKey(key));
                }

                foreach (var val in d2.Values)
                {
                    Assert.True(list2.ContainsValue(val));
                }

                Assert.True(d2.LongCount == count, "Count mismatch!");
                foreach (var pair in d2)
                {
                    Assert.True(list2[pair.Key] == pair.Value);
                    Assert.True(list2.Remove(pair.Key));
                }
                Assert.True(list2.Count == 0);

                d4 = md.EnsureDictionary<ulong, byte[]>("Dict4");

                for (int i = 1; i <= 100; i++)
                {
                    var bytes2 = new byte[i + 100];
                    for (int k = 0; k < bytes2.Length; k++)
                    {
                        bytes2[k] = (byte)(i + k);
                    }
                    d4[(ulong)i] = bytes2;
                }
            }
        }

        public void Dispose()
        {
            pdb.Dispose();
        }
    }

    class Dummy
    {
        public ulong Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public DateTime Born
        {
            get;
            set;
        }

        public long Count
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1} {2} {3}", Id, Name, Born, Count);
        }

        private static ulong IdCounter = 0;

        public static Dummy GetRandomDummy()
        {
            var ret = new Dummy();
            ret.Id = Interlocked.Increment(ref IdCounter);
            ret.Name = "ABC";
            ret.Count = 125;
            ret.Born = DateTime.Now;

            return ret;
        }
    }
}




