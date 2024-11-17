using KeyValium.TestBench.Shared;
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
        const int WaitTime = 1000;

        public FolderWatcher(SharedTestInfo ti)
        {
            TestInfo = ti;
        }

        private readonly SharedTestInfo TestInfo;

        public void Watch()
        {
            Console.WriteLine("Watching folder {0} for {1} ...", TestInfo.NetworkPath, TestInfo.ControlFilePattern);

            while (true)
            {
                try
                {
                    var files = Directory.GetFiles(TestInfo.NetworkPath, TestInfo.ControlFilePattern);
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

        private void DoWork(string file)
        {
            try
            {
                Console.WriteLine("Processing file {0}...", file);
                var worker = new Worker(TestInfo);
                worker.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                File.Delete(file);
            }
            
            Console.WriteLine("Processing done.");
        }
    }
}
