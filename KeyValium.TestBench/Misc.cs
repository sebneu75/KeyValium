using KeyValium.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    internal class Misc
    {
        private void CompareComparers(List<KeyValuePair<byte[], byte[]>> list)
        {
            var comp1 = new KeyComparer();

            for (int i = 0; i < list.Count; i++)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    var key1 = list[i].Key;
                    var key2 = list[k].Key;

                    var r1 = comp1.Compare(key1, key2);
                    var r2 = UniversalComparer.CompareBytes(key1, key2);

                    if (r1 != r2)
                    {
                        Console.WriteLine("Hossa");
                    }
                }
            }
        }

        private static void ShowLimits()
        {
            var pagesize = 65536;
            var headersize = 32;

            var minkeysleaf = 2;
            var minkeysindex = 3;

            do
            {
                Console.WriteLine("Pagesize: {0}", pagesize);

                var maxkeysize = 0;
                switch (pagesize)
                {
                    case 128:
                        maxkeysize = pagesize / 16;
                        break;
                    case 256:
                        maxkeysize = pagesize / 8;
                        break;
                    default:
                        maxkeysize = pagesize / 4;
                        break;
                }

                // 64 - Headersize
                // 16 - Size of additional metadata ()
                var maxentrysizeleaf = (ushort)((pagesize - headersize - (minkeysleaf * 16)) / minkeysleaf);
                var maxkeysizeindex = (ushort)((pagesize - headersize - (minkeysindex * 2) - (minkeysindex + 1) * 8) / minkeysindex);

                var contentsize = pagesize - headersize; // -headersize

                //var indexsize = MinKeysPerIndexPage * MaxKeySize + MinKeysPerIndexPage * 2 + (MinKeysPerIndexPage + 1) * 8;

                //Console.WriteLine("            Page Size: {0}", pagesize);
                Console.WriteLine("         Content Size: {0}", contentsize);
                Console.WriteLine("     Max Keysize Leaf: {0}", maxkeysize);
                Console.WriteLine("  Max Entry size Leaf: {0}", maxentrysizeleaf);
                Console.WriteLine("    Max Keysize Index: {0}", maxkeysizeindex);

                pagesize >>= 1;
            }
            while (pagesize >= 128);
        }
    }
}
