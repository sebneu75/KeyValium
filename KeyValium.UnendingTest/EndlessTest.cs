using KeyValium.TestBench;
using KeyValium.TestBench.ActionProviders;
using KeyValium.TestBench.Measure;
using KeyValium.TestBench.Runners;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace KeyValium.UnendingTest
{
    internal class EndlessTest
    {
        private static TestDescription DefaultDescription
        {
            get
            {
                //var td = new TestDescription("Endless");
                //td.Options.MetaPages = 4;
                //td.Options.PageSize = 4096;
                //td.Options.Shared = false;
                //td.Options.CacheSizeDatabaseMB = 256;
                //td.MinKeySize = 2;
                //td.MaxKeySize = 1024;
                //td.MinValueSize = 0;
                //td.MaxValueSize = 10240;
                //td.KeyCount = 10000;
                //td.CommitSize = 1000;
                //td.TxCount = 5000;
                //td.GenStrategy = KeyGenStrategy.Random;
                //td.OrderInsert = KeyOrder.Random;
                //td.OrderRead = KeyOrder.Random;
                //td.OrderDelete = KeyOrder.Random;

                var td = new TestDescription("Endless");
                td.Options.PageSize = 256;
                td.Options.SharingMode = SharingModes.Exclusive;
                td.Options.CacheSizeMB = 256;
                td.Options.FlushToDisk = false;
                td.Options.ValidationMode = PageValidationMode.All;
                td.MinKeySize = 2;
                td.MaxKeySize = 16;
                td.MinValueSize = 0;
                td.MaxValueSize = (int)td.Options.PageSize * 8 - 32;
                td.KeyCount = 16;
                td.CommitSize = 1000;
                td.TxCount = 1000;
                td.GenStrategy = KeyGenStrategy.Random;
                td.OrderInsert = KeyOrder.Random;
                td.OrderRead = KeyOrder.Random;
                td.OrderDelete = KeyOrder.Random;
                //td.Options.Password = "123";

                //var td = new TestDescription("Endless");
                //td.Options.PageSize = 65536;
                //td.Options.SharingMode = SharingModes.Exclusive;
                //td.Options.CacheSizeMB = 256;
                //td.Options.FlushToDisk = false;
                //td.Options.ValidationMode = PageValidationMode.All;
                //td.MinKeySize = 2;
                //td.MaxKeySize = 48;
                //td.MinValueSize = 0;
                //td.MaxValueSize = (int)td.Options.PageSize * 8 - 32;
                //td.KeyCount = 1600;
                //td.CommitSize = 1000;
                //td.TxCount = 1000;
                //td.GenStrategy = KeyGenStrategy.Random;
                //td.OrderInsert = KeyOrder.Random;
                //td.OrderRead = KeyOrder.Random;
                //td.OrderDelete = KeyOrder.Random;

                //var td = new TestDescription("Endless");
                //td.Options.PageSize = 512;
                //td.Options.SharingMode = SharingModes.Exclusive;
                //td.Options.CacheSizeMB = 256;
                //td.Options.FlushToDisk = false;
                //td.Options.ValidationMode = PageValidationMode.All;
                //td.MinKeySize = 2;
                //td.MaxKeySize = 16;
                //td.MinValueSize = 0;
                //td.MaxValueSize = (int)td.Options.PageSize * 8 - 32;
                //td.KeyCount = 1600;
                //td.CommitSize = 1000;
                //td.TxCount = 1000;
                //td.GenStrategy = KeyGenStrategy.Random;
                //td.OrderInsert = KeyOrder.Random;
                //td.OrderRead = KeyOrder.Random;
                //td.OrderDelete = KeyOrder.Random;

                return td;
            }
        }

#if DEBUG
        const int ThreadCount = 1;
#else
        const int ThreadCount = 16;
#endif

        public static void Run()
        {
            var tasks = new List<Task>();
            var counter = 0;

            while (true)
            {
                var temp = counter;

                tasks.Add(Task.Run(() =>
                {
                    var runner = new EndlessRunner();
                    var provider = new RandomActionprovider(GetCopy(DefaultDescription, temp));
                    runner.Run(provider);
                }));

                counter++;

                if (counter >= ThreadCount)
                {
                    var done = Task.WaitAny(tasks.ToArray());
                    tasks.RemoveAt(done);
                }

                Thread.Sleep(10);
            }
        }

        private static TestDescription GetCopy(TestDescription td, int counter)
        {
            var ret = td.Copy();

            ret.Token = string.Format("({0:000000}-{1:yyyyMMddHHmmssffffff})", counter, DateTime.Now);

            return ret;
        }

        public static void ReplayAll()
        {
            var actionlogs = Directory.GetFiles(TestDescription.ErrorPath, "*.actions").OrderBy(x => x).ToList();

            foreach (var actionlog in actionlogs)
            {
                var dbfile = Path.Combine(Path.GetDirectoryName(actionlog), Path.GetFileNameWithoutExtension(actionlog));
                var tdfile = dbfile + ".td";
                var logfile = dbfile + ".log";

                TestDescription td = null;

                if (File.Exists(tdfile))
                {
                    td = TestDescription.Load(tdfile);
                }
                else
                {
                    td = GetCopy(DefaultDescription, 0);
                }

                var backups = GetBackups(dbfile);
                var backup = GetLatestBackup(backups, out var tid);

                if (backup != null)
                {
                    File.Copy(backup, td.DbFilename, true);
                }

                var runner = new EndlessRunner();
                var provider = new LogActionProvider(td, actionlog, tid);

                if (runner.Run(provider))
                {
                    Tools.WriteSuccess("{0} SUCCESS", actionlog);

                    var files = new List<string>() { actionlog, dbfile, tdfile, logfile };
                    files.AddRange(backups);
                    MoveToSuccess(files);
                }
                else
                {
                    Tools.WriteError(null, "{0} FAIL", actionlog);
                }
            }
        }

        private static string GetLatestBackup(List<string> backups, out ulong tid)
        {
            tid = 0;

            var ret = backups.OrderBy(x => x).ToList().LastOrDefault();

            if (ret != null)
            {
                var f1 = Path.GetFileNameWithoutExtension(ret);
                var number = Path.GetFileNameWithoutExtension(f1);

                ulong.TryParse(number, out tid);
            }

            return ret;
        }

        private static List<string> GetBackups(string dbfile)
        {
            var path = Path.GetDirectoryName(dbfile);
            var fname = Path.GetFileNameWithoutExtension(dbfile);
            var ext = Path.GetExtension(dbfile);

            var backups = Directory.GetFiles(path, fname + ".*" + ext);

            return backups.ToList();
        }


        private static void MoveToSuccess(List<string> files)
        {
            var targetpath = TestDescription.SuccessPath;
            Directory.CreateDirectory(targetpath);

            foreach (var file in files)
            {
                if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                {
                    var newfile = Path.Combine(targetpath, Path.GetFileName(file));
                    File.Move(file, newfile);
                }
            }
        }
    }
}

