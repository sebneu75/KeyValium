using KeyValium.Frontends.Serializers;
using KeyValium.Options;
using KeyValium.Recovery;
using KeyValium.TestBench.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestSharedController
{
    internal class DatabaseController
    {
        public DatabaseController(SharedTestInfo ti)
        {
            TestInfo = ti;
            TestInfoList = TestInfo.SplitByProcess();
        }

        SharedTestInfo TestInfo;

        List<SharedTestInfo> TestInfoList;

        Database LocalDb;

        internal void Run()
        {
            // create local database
            if (File.Exists(TestInfo.LocalDbFilename))
            {
                File.Delete(TestInfo.LocalDbFilename);
            }

            var options = new DatabaseOptions();
            options.CreateIfNotExists = true;

            LocalDb = Database.Open(TestInfo.LocalDbFilename);

            var cycle = 0;

            while (true)
            {
                cycle++;

                Console.WriteLine(new string('-', 47));
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
            }
        }

        #region Verification

        private void VerifyDatabase()
        {
            VerifyDatabaseIntegrity();
            UpdateLocalDatabase();
        }

        private void VerifyDatabaseIntegrity()
        {
            Console.WriteLine("Verifying database integrity...");

            var options = new DatabaseOptions();
            options.CreateIfNotExists = false;

            using (var db = Database.Open(TestInfo.UncDbFilename))
            {
                var result = Verification.VerifyDatabase(db);
                result.ThrowOnError();
            }

            Console.WriteLine("Database integrity verified successfully.");
        }

        private void UpdateLocalDatabase()
        {
            Console.WriteLine("Verifying database content...");

            if (!File.Exists(TestInfo.UncWriteTxLog))
            {
                return;
            }

            var ser = new KvJsonSerializer();

            var options = new DatabaseOptions();
            options.CreateIfNotExists = true;

            using (var reader = new StreamReader(TestInfo.UncWriteTxLog))
            {
                Transaction? tx = null;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var vals = line.Split(' ');
                    if (vals.Length < 5) continue;

                    if (vals[2] == "Begin")
                    {
                        if (tx != null)
                        {
                            // deal with aborted transactions
                            tx.Rollback();
                            tx.Dispose();
                            tx = null;
                        }

                        var tid = ulong.Parse(vals[4]);
                        tx = LocalDb.BeginWriteTransaction();
                        if (tx.Tid != tid)
                        {
                            throw new Exception("Tid mismatch");
                        }
                    }
                    else if (vals[2] == "End")
                    {
                        tx.Commit();
                        tx.Dispose();
                        tx = null;
                    }
                    else if (vals[2] == "Upsert")
                    {
                        var key = ser.Serialize(vals[3], false);
                        var val = ser.Serialize(vals[4], true);
                        tx.Upsert(null, key, val);
                    }
                }
            }

            File.Delete(TestInfo.WriteTxLog);

            // compare content
            CompareContent();

            Console.WriteLine("Database content verified successfully.");

            return;

            // compare files
            //var buf1 = new byte[1024 * 1024];
            //var buf2 = new byte[1024 * 1024];

            //using (var reader1 = new FileStream(DatabaseFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            //using (var reader2 = new FileStream(LocalDatabaseFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            //{
            //    if (reader1.Length != reader2.Length)
            //    {
            //        throw new Exception("Filesize not equal");
            //    }

            //    while (reader1.Position < reader1.Length)
            //    {
            //        var len1 = reader1.Read(buf1);
            //        var len2 = reader2.Read(buf2);

            //        var span1 = buf1.AsSpan().Slice(0, len1);
            //        var span2 = buf2.AsSpan().Slice(0, len2);
            //        var result = span1.SequenceEqual(span2);
            //        if (!result)
            //        {
            //            // that is because recycled pages are used
            //            throw new Exception("Files not identical");
            //        }
            //    }
            //}
        }

        private void CompareContent()
        {
            var ser = new KvJsonSerializer();

            var items1 = ReadKeys(TestInfo.UncDbFilename, ser);
            var items2 = ReadKeys(LocalDb, ser);

            var keys1 = items1.Keys.ToHashSet();
            var keys2 = items2.Keys.ToHashSet();

            var copy1 = keys1.ToHashSet();
            var copy2 = keys2.ToHashSet();

            copy1.ExceptWith(copy2);

            if (copy1.Count > 0)
            {
                throw new Exception("Different keys");
            }

            copy1 = keys1.ToHashSet();
            copy2 = keys2.ToHashSet();

            copy2.ExceptWith(copy1);

            if (copy2.Count > 0)
            {
                throw new Exception("Different keys");
            }

            if (keys1.Count != keys2.Count)
            {
                throw new Exception("Count mismatch");
            }

            foreach (var key in keys1)
            {
                if (items1[key] != items2[key])
                {
                    throw new Exception("Value mismatch");
                }
            }

            foreach (var key in keys2)
            {
                if (items1[key] != items2[key])
                {
                    throw new Exception("Value mismatch");
                }
            }
        }

        #endregion

        #region Database Helpers

        private Dictionary<string, string> ReadKeys(string dbfile, KvJsonSerializer ser)
        {
            using (var db = Database.Open(dbfile))
            {
                return ReadKeys(db, ser);
            }
        }
        private Dictionary<string, string> ReadKeys(Database db, KvJsonSerializer ser)
        {
            var ret = new Dictionary<string, string>();

            using (var tx = db.BeginReadTransaction())
            {
                foreach (var item in tx.GetIterator(null, true))
                {
                    var key = ser.Deserialize<string>(item.Value.Key, false);
                    var val = ser.Deserialize<string>(item.Value.Value, true);

                    ret.Add(key, val);
                }
            }

            return ret;
        }

        #endregion

        private void WaitForDelete(List<string> files)
        {
            Console.WriteLine("Waiting for Jobs to finish...");

            Thread.Sleep(5000);

            foreach (var file in files)
            {
                while (File.Exists(file))
                {
                    Thread.Sleep(500);
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
                    Console.WriteLine(string.Join("\n", error));

                    Console.WriteLine("***********************");
                }
            }

            Console.WriteLine("Jobs have ended.");
        }

        private List<string> CreateData(int cycle)
        {
            var files = new List<string>();

            foreach (var item in TestInfoList)
            {
                var filename = item.GetControlFileName(cycle);

                using (var writer = new StreamWriter(filename + ".tmp"))
                {
                    writer.WriteLine(filename);
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

        //private List<string[]> CreateRandomData(string computer)
        //{
        //    var ret = new List<string[]>();

        //    var rnd = new Random();

        //    for (int i = 0; i < 50; i++)
        //    {
        //        ret.Add(CreateRandomEntry(computer, rnd));
        //    }

        //    return ret;
        //}

        //private string[] CreateRandomEntry(string computer, Random rnd)
        //{
        //    var ret = new string[3];

        //    // Dictionary to use
        //    ret[0] = (rnd.Next(3) + 1).ToString();

        //    // Key
        //    ret[1] = (rnd.Next(1000)).ToString("0000");

        //    // Value
        //    ret[2] = (rnd.Next(1000)).ToString("0000");
        //    //ret[2] = GetRandomString(computer, rnd, 100, 3500);

        //    return ret;
        //}

        //private string GetRandomString(string prefix, Random rnd, int minlen, int maxlen)
        //{
        //    var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        //    var len = rnd.Next(maxlen - minlen) + minlen + 1;

        //    var sb = new StringBuilder();
        //    for (int i = 0; i < len; i++)
        //    {
        //        sb.Append(chars[rnd.Next(chars.Length)]);
        //    }

        //    return sb.ToString();
        //}
    }
}
