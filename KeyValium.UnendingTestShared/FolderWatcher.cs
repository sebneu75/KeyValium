using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestShared
{
    internal class FolderWatcher
    {
        static List<string> Folders = new List<string>()
        {
            @"\\thesource\!KeyValium-OpLocks",
            @"\\thesource\!KeyValium-No-OpLocks",
            @"\\Madhouse\!KeyValium",
        };

        const int WaitTime = 1000;

        public FolderWatcher(string id)
        {
            Id = id;
        }

        private readonly string Id;

        public string Pattern
        {
            get
            {
                var eid = "";

                if (!string.IsNullOrEmpty(Id))
                {
                    eid = "-" + Id;
                }

                return string.Format("{0}{1}*.kvjob", Environment.MachineName.ToLowerInvariant(), eid);
            }
        }

        public void Watch()
        {
            foreach (var folder in Folders)
            {
                Console.WriteLine("Watching folder {0}...", folder);
            }

            while (true)
            {
                foreach (var folder in Folders)
                {
                    try
                    {
                        var files = Directory.GetFiles(folder, Pattern);
                        foreach (var file in files)
                        {
                            DoWork(file);

                            Console.WriteLine("-----------------------");
                        }

                        if (files.Length == 0)
                        {
                            Thread.Sleep(WaitTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Thread.Sleep(WaitTime * 5);
                    }
                }
            }
        }

        private void DoWork(string file)
        {
            try
            {
                Console.WriteLine("Processing file {0}...", file);
                Worker.Run(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Processing done.");
        }
    }
}
