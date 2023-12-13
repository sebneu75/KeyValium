using KeyValium.Cursors;
using KeyValium.Pages.Entries;
using KeyValium.TestBench;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Tests.Misc
{
    public sealed class TestMisc : IDisposable
    {
        public TestMisc()
        {
        }

        [Fact]
        public void TestEntryExtern()
        {
            var entry1 = new EntryExtern(1, 2, 3);
            var entry2 = entry1;

            var x1 = Tools.GetHexString(entry1.Key);
            var x2 = Tools.GetHexString(entry2.Key);

            BinaryPrimitives.WriteUInt64BigEndian(entry1.FsSpan, 111);

            var val1 = BinaryPrimitives.ReadUInt64BigEndian(entry1.Key);
            var val2 = BinaryPrimitives.ReadUInt64BigEndian(entry2.Key);

            Assert.NotEqual(val1, val2);
        }

        public void Dispose()
        {
        }
    }
}



