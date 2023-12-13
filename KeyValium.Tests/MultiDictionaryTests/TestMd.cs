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
            using (var md = KvMultiDictionary.Open(pdb.Description.DbFilename, pdb.Description.Options))
            {
                {
                    var s1 = new KvSerializer();
                    var x1 = new byte[1] { 0x80 };
                    var str1 = s1.Deserialize<string>(x1);
                }

                var s = new KvJsonSerializer();
                var x = 0x80;
                var bytes = s.Serialize(x);

                var str=Encoding.UTF8.GetString(bytes);

                var d1 = md.EnsureDictionary<string, string>("Dict1", false);
                var d2 = md.EnsureDictionary<ulong, string>("Dict2", false);
                var d3 = md.EnsureDictionary<ulong, Dummy>("Dict3", false);

                var list = md.GetDictionaries();

                //for (ulong i = 0; i < 1000; i++)
                //{
                //    d2.Upsert(i, string.Format("{0}{0}{0}{0}{0}{0}{0}{0}", i));
                //}

                //for (ulong i = 0; i < 1000; i++)
                //{
                //    var x1 = d2.Get(i);
                //    Console.WriteLine(x1);
                //}

                //return;

                for (ulong i = 0; i < 100; i++)
                {
                    var dummy = Dummy.GetRandomDummy();
                    d3.Upsert(dummy.Id, dummy);
                }

                for (ulong i = 1; i <= 100; i++)
                {
                    var x1 = d3.Get(i);
                    Console.WriteLine(x1);
                }

                //d1.Upsert("Key", "Value");
                //var val1 = d1.Get("Key");
                //d1.Upsert("", "Value2");
                //var val2 = d1.Get("");
                //var val3 = d1.Get(null);
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




