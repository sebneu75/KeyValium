using KeyValium.Recovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestSharedController
{
    internal class Controller
    {
        static List<string> Folders = new List<string>()
        {
            @"\\thesource\!KeyValium-OpLocks",
            @"\\thesource\!KeyValium-No-OpLocks",
            @"\\Madhouse\!KeyValium",
            @"\\zbox\!KeyValium",
        };

        static List<string> Computers = new List<string>()
        {
            @"madhouse",
            @"zbox"
        };

        public Controller()
        {

        }

        public string DatabasePath
        {
            get
            {
                return Folders[0];
            }
        }

        public string DatabaseFile
        {
            get
            {
                return Path.Combine(DatabasePath, "sharedtest.kvlm");
            }
        }

        public void Run()
        {
            var cycle = 0;

            while (true)
            {
                cycle++;

                Console.WriteLine("Starting Cycle {0}...", cycle);

                var files = CreateData(cycle);

                foreach (var file in files)
                {
                    Console.WriteLine("Created Job file: {0}", file);
                }

                WaitForDelete(files);

                try
                {
                    VerifyDatabase();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Verification of database failed!");
                    Console.WriteLine(ex);
                    break;
                }

                Console.WriteLine("-----------------------");
            }
        }

        private void VerifyDatabase()
        {
            Console.WriteLine("Verifying database...");

            using (var db = Database.Open(DatabaseFile))
            {
                var result = Verification.VerifyDatabase(db);
                result.ThrowOnError();
            }

            Console.WriteLine("Database verified successfully.");
        }

        private void WaitForDelete(List<string> files)
        {
            Console.WriteLine("Waiting for Jobs to finish...");

            foreach (var file in files)
            {
                while (File.Exists(file))
                {
                    Thread.Sleep(1000);
                }
            }

            // check errors
            foreach (var file in files)
            {
                if (File.Exists(file + ".error"))
                {
                    Console.WriteLine("***********************");
                    Console.WriteLine("Job {0} failed:", file);

                    var error = File.ReadAllLines(file + ".error");
                    Console.WriteLine(error);

                    Console.WriteLine("***********************");
                }
                FileStream fs;
                
            }

            Console.WriteLine("Jobs have ended.");
        }

        private List<string> CreateData(int cycle)
        {
            var files = new List<string>();

            foreach (var item in Computers)
            {
                var list = CreateRandomData(item);
                var filename = Path.Combine(DatabasePath, string.Format("{0}-{1:000000000000}.kvjob", item, cycle));
                using (var writer = new StreamWriter(filename + ".tmp"))
                {
                    writer.WriteLine(DatabaseFile);
                    list.ForEach(x => writer.WriteLine(string.Join("|", x)));
                }

                files.Add(filename);
            }

            // rename files
            foreach (var file in files)
            {
                File.Move(file + ".tmp", file);
            }

            return files;
        }

        private List<string[]> CreateRandomData(string computer)
        {
            var ret = new List<string[]>();

            var rnd = new Random();

            for (int i = 0; i < 50; i++)
            {
                ret.Add(CreateRandomEntry(computer, rnd));
            }

            return ret;
        }

        private string[] CreateRandomEntry(string computer, Random rnd)
        {
            var ret = new string[3];

            // Dictionary to use
            ret[0] = (rnd.Next(3) + 1).ToString();

            // Key
            ret[1] = (rnd.Next(1000)).ToString("0000");

            // Value
            ret[2] = GetRandomString(computer, rnd, 100, 3500);

            return ret;
        }

        private string GetRandomString(string prefix, Random rnd, int minlen, int maxlen)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var len = rnd.Next(maxlen - minlen) + minlen + 1;

            var sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }

            return sb.ToString();
        }
    }
}
