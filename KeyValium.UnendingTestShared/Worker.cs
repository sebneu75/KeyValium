using KeyValium.Frontends.MultiDictionary;
using KeyValium.Frontends.Serializers;
using KeyValium.Logging;
using KeyValium.Options;
using KeyValium.TestBench.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestShared
{
    internal class Worker
    {
        public Worker(SharedTestInfo ti)
        {
            TestInfo = ti;
            _logger = new TxLogger(ti);

            using (var p = Process.GetCurrentProcess())
            {
                _procid = p.Id;
            }
        }

        readonly SharedTestInfo TestInfo;
        readonly TxLogger _logger;
        readonly int _procid;

        KvJsonSerializer _serializer = new KvJsonSerializer();

        public void Run()
        {
            try
            {
                var tasks = new List<Task>();

                // create database instances
                for (int i = 0; i < TestInfo.DatabaseInfo.Instances; i++)
                {
                    Console.WriteLine("Starting database instance {0}:{1}", TestInfo.DatabaseInfo.Filename, i);

                    var temp = i;
                    var task = Task.Run(() => DoWork(temp));
                    tasks.Add(task);
                }

                // wait for end
                Task.WaitAll(tasks.ToArray());

                ShowErrors(tasks, "Database instances");
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex);

                    //using (var writer = new StreamWriter(_jobfile + ".error"))
                    //{
                    //    writer.WriteLine(ex);
                    //}
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2);
                }
            }
        }

        private void ShowErrors(List<Task> tasks, string msg)
        {
            Console.WriteLine("{0} finished - Tasks: {1}", msg, tasks.Count);

            foreach (var task in tasks)
            {
                Console.WriteLine("Task {0}: {1}", task.Id, task.Status);
                if (task.Exception != null)
                {
                    Console.WriteLine(task.Exception);
                }

                Console.WriteLine("-------------------------------------------");
            }
        }

        private void DoWork(int dbinstance)
        {
            Console.WriteLine("Database Instance {0}:{1} started.", TestInfo.DatabaseInfo.Filename, dbinstance);

            // create database instance
            var options = new DatabaseOptions();
            options.InternalSharingMode = TestInfo.DatabaseInfo.SharingMode;
            options.LogTopics = Logging.LogTopics.Database | Logging.LogTopics.Transaction | Logging.LogTopics.Lock | Logging.LogTopics.Meta;

            // TODO test with cache
            options.CacheSizeMB = 0;

            Database db = null;

            var threadname = string.Format("{0}-{1}-{2}-DB{3}", TestInfo.Machine.Name, _procid, Path.GetFileName(TestInfo.DbFilename), dbinstance);

            try
            {
                Logger.CreateInstance(TestInfo.DbFilename, options.LogLevel, options.LogTopics);
                Logger.SetThreadName(threadname);

                db = Database.Open(TestInfo.DbFilename, options);

                var tasks = new List<Task>();

                // create writers
                for (int i = 0; i < TestInfo.DatabaseInfo.Writers; i++)
                {
                    var temp = i;
                    var task = Task.Run(() => RunWriter(db, dbinstance, temp));
                    tasks.Add(task);
                }

                // create readers
                for (int i = 0; i < TestInfo.DatabaseInfo.Readers; i++)
                {
                    var temp = i;
                    var task = Task.Run(() => RunReader(db, dbinstance, temp));
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());

                ShowErrors(tasks, string.Format("Transactions for Database instance {0}", dbinstance));
            }
            catch (Exception ex)
            {
                WriteError(threadname, ex);
            }
            finally
            {
                db?.Dispose();
            }
        }

        void RunReader(Database db, int dbinstance, int readernum)
        {
            var threadname = string.Format("{0}-{1}-{2}-DB{3}-Reader{4}", TestInfo.Machine.Name, _procid, Path.GetFileName(TestInfo.DbFilename), dbinstance, readernum);
            Logger.SetThreadName(threadname);

            var rnd = new Random();

            var txcount = rnd.Next(10, 20);

            for (int i = 0; i < txcount; i++)
            {
                try
                {
                    using (var tx = db.BeginReadTransaction())
                    {
                        _logger.LogReadTxStart(tx, threadname);

                        var items = CreateRandomData(100, 200);

                        foreach (var item in items)
                        {
                            var key = _serializer.Serialize(item.Key, false);

                            var valinfo = tx.Get(null, key);

                            var val = "n/a";

                            if (valinfo.IsValid)
                            {
                                val = _serializer.Deserialize<string>(valinfo.Value, true);
                            }

                            _logger.LogGet(item.Key, val, threadname);
                        }

                        _logger.LogReadTxEnd(tx, threadname);

                        tx.Commit();
                    }
                }
                catch (Exception ex)
                {
                    WriteError(threadname, ex);
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        void RunWriter(Database db, int dbinstance, int writernum)
        {
            var threadname = string.Format("{0}-{1}-{2}-DB{3}-Writer{4}", TestInfo.Machine.Name, _procid, Path.GetFileName(TestInfo.DbFilename), dbinstance, writernum);
            Logger.SetThreadName(threadname);

            var rnd = new Random();

            var txcount = rnd.Next(10, 20);

            for (int i = 0; i < txcount; i++)
            {
                try
                {
                    using (var tx = db.BeginWriteTransaction())
                    {
                        _logger.LogTxStart(tx);

                        var items = CreateRandomData(100, 200);

                        foreach (var item in items)
                        {
                            var key = _serializer.Serialize(item.Key, false);
                            var val = _serializer.Serialize(item.Value, true);

                            tx.Upsert(null, key, val);
                            _logger.LogUpsert(item.Key, item.Value);
                        }

                        _logger.LogTxEnd(tx);

                        tx.Commit();
                    }
                }
                catch (Exception ex)
                {
                    WriteError(threadname, ex);
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        private void WriteError(string threadname, Exception ex)
        {
            var path = Path.Combine(Path.GetDirectoryName(TestInfo.WriteTxLog), threadname + ".error");
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine(ex);
                writer.WriteLine("--- End of Exception --------------------");
            }
        }

        private static void DoWorkOld(string dbfilename, string logfilename, List<string[]> lines)
        {
            var rnd = new Random();

            var options = new DatabaseOptions();
            options.InternalSharingMode = InternalSharingModes.SharedNetwork;
            options.LogTopics = Logging.LogTopics.Database | Logging.LogTopics.Transaction | Logging.LogTopics.Lock | Logging.LogTopics.Meta;
            //options.CacheSizeMB = 0;

            using (var md = KvMultiDictionary.Open(dbfilename, options))
            {
                using (var dict1 = md.EnsureDictionary<string, string>("Dictionary1"))
                using (var dict2 = md.EnsureDictionary<string, string>("Dictionary2"))
                using (var dict3 = md.EnsureDictionary<string, string>("Dictionary3"))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i];

                        switch (line[0])
                        {
                            case "1":
                                dict1[line[1]] = line[2];
                                break;
                            case "2":
                                dict2[line[1]] = line[2];
                                break;
                            case "3":
                                dict3[line[1]] = line[2];
                                break;
                            default:
                                break;
                        }

                        if ((i + 1) % 1 == 0)
                        {
                            Console.WriteLine("Processed line {0}/{1}.", i + 1, lines.Count);
                        }

                        // random delay
                        //Thread.Sleep(rnd.Next(1000) + 50);
                    }
                }
            }
        }


        private static List<KeyValuePair<string, string>> CreateRandomData(int min, int max)
        {
            var ret = new List<KeyValuePair<string, string>>();
            var rnd = new Random();

            var count = rnd.NextInt64(min, max);

            for (int i = 0; i < count; i++)
            {
                var key = (rnd.Next(100000)).ToString("00000");
                var val = (rnd.Next(100000)).ToString("00000");

                ret.Add(new KeyValuePair<string, string>(key, val));
            }

            return ret;
        }
    }
}
