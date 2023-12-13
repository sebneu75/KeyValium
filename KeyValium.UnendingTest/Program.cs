using KeyValium.Inspector;
using KeyValium.TestBench;
using System.Text;

namespace KeyValium.UnendingTest
{
    class Program
    {
        public const string WORKINGPATH = @"b:\!KeyValium";

        public static string DUMPFILE = Path.Combine(WORKINGPATH, "dump");

        static void Main(string[] args)
        {
            Directory.CreateDirectory(WORKINGPATH);
            Directory.CreateDirectory(Path.Combine(WORKINGPATH, "Errors"));
            Directory.CreateDirectory(Path.Combine(WORKINGPATH, "Successes"));

            TestDescription.WorkingPath = WORKINGPATH;

#if DEBUG
            // replay failed tests
            EndlessTest.ReplayAll();
            EndlessTest.Run();
#endif
            EndlessTest.ReplayAll();
            EndlessTest.Run();

            Console.WriteLine("Ready.");
            Console.ReadLine();
        }

        private static void DumpUnicodeCategories()
        {
            var enc = Encoding.Default;
            for (int i = 0; i < 256; i++)
            {
                var ch = enc.GetString(new byte[] { (byte)i })[0];
                Console.WriteLine("{0:X2}: {1}", i, char.GetUnicodeCategory(ch));
            }
        }
    }
}
