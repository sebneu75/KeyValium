using KeyValium.TestBench.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestShared
{
    internal class TestManager
    {
        internal static void Run(SharedTestInfo ti)
        {
            Console.SetWindowSize(60, 30);

            // start one Thread per Database
            var tis = ti.SplitByDatabase();

            var tasks = new List<Task>();

            foreach (var t in tis)
            {
                var task = Task.Run(() => HandleDatabase(t));
                tasks.Add(task);

            }

            Task.WaitAll(tasks.ToArray());
        }

        internal static void HandleDatabase(SharedTestInfo ti)
        {
                var watcher = new FolderWatcher(ti);
                watcher.Watch();
        }
    }
}

