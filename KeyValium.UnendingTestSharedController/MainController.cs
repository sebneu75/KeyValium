using KeyValium.Frontends.Serializers;
using KeyValium.Options;
using KeyValium.Recovery;
using KeyValium.TestBench;
using KeyValium.TestBench.Shared;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KeyValium.UnendingTestSharedController
{
    internal class MainController
    {
        public MainController()
        {
            var pd = LoadPrivateData();
            TestInfo = GetTestInfo(pd);
        }

        #region TestInfo management

        public SharedTestInfo TestInfo
        {
            get;
            private set;
        }

        private SharedTestInfo GetTestInfo(PrivateData pd)
        {
            var sharingmode = InternalSharingModes.SharedLocal;

            var sti = new SharedTestInfo();

            if (pd == null)
            {
                sti.NetworkPath = @"\\server\share";
                sti.Machines.Add(new MachineInfo() { Name = "localhost", LocalPath = @"c:\\KeyValium", RemotePath = @"\\localhost\c$\keyvalium" });
                sti.ToolsSourceDirectory = @"c:\tools";
            }
            else
            {
                sti.NetworkPath = pd.NetworkPath;
                sti.Machines.AddRange(pd.Machines);
                sti.ToolsSourceDirectory = pd.SourceDirectory;
            }

            sti.ProcessCount = 3;

            var dbi1 = new DatabaseInfo()
            {
                Instances = 2,
                Filename = "sharednetwork1.kvlm",
                SharingMode = sharingmode,
                Readers = 3
            };

            var dbi2 = new DatabaseInfo()
            {
                Instances = 2,
                Filename = "sharednetwork2.kvlm",
                SharingMode = sharingmode,
                Readers = 3
            };

            var dbi3 = new DatabaseInfo()
            {
                Instances = 2,
                Filename = "sharednetwork3.kvlm",
                SharingMode = sharingmode,
                Readers = 3
            };

            sti.DatabaseInfos.Add(dbi1);
            sti.DatabaseInfos.Add(dbi2);
            sti.DatabaseInfos.Add(dbi3);

            return sti;
        }

        private PrivateData LoadPrivateData()
        {
            var filename = @"private\private.json";

            Console.WriteLine("Loading private data...");

            if (File.Exists(filename))
            {
                return KvJson.Load<PrivateData>(filename);
            }
            else
            {
                Console.WriteLine("No private data found.");
            }

            return null;
        }

        #endregion


        //public string DatabasePath
        //{
        //    get
        //    {
        //        return TestInfo. ControlFolder;
        //    }
        //}

        //public string DatabaseFile
        //{
        //    get
        //    {
        //        return Path.Combine(DatabasePath, "sharedtest.kvlm");
        //    }
        //}


        public void Run()
        {
            // copy binaries (release or debug)
            CopyBinaries();

            Console.WriteLine("Clearing directory {0} ...", TestInfo.NetworkPath);

            // clear work directory
            if (Directory.Exists(TestInfo.NetworkPath))
            {
                Directory.Delete(TestInfo.NetworkPath, true);
            }

            Directory.CreateDirectory(TestInfo.NetworkPath);

            Console.WriteLine("Starting binaries...");

            // start binaries
            StartBinaries();

            var tasks = new List<Task>();

            // create one controller per database
            foreach (var ti in TestInfo.SplitByDatabase())
            {
                var dbc = new DatabaseController(ti);

                var task = Task.Run(() => dbc.Run());
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }

        private void StartBinaries()
        {
            var temp = TestInfo.SplitByProcess();

            foreach (var sti in temp)
            {
                StartBinariesInternal(sti);
            }

            foreach (var sti in temp)
            {
                // rename startfile
                if (sti.Machine.ProcStartFile != null && File.Exists(sti.Machine.ProcStartFile + ".tmp"))
                {
                    File.Move(sti.Machine.ProcStartFile + ".tmp", sti.Machine.ProcStartFile);
                }
            }
        }

        private void StartBinariesInternal(SharedTestInfo info)
        {
            var path = info.Machine.LocalPath + "\\KeyValium.UnendingTestShared.exe";
            var argfile = string.Format("{0}-{1}.args", info.Machine.Name, info.Token);
            argfile = Path.Combine(info.NetworkPath, argfile);

            KvJson.Save(info, argfile);

            if (info.Machine.ProcStartFile == null)
            {
                var psi = new ProcessStartInfo(path);
                psi.Arguments = argfile;
                psi.UseShellExecute = true;
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                var result = Process.Start(psi);
            }
            else
            {
                try
                {
                    using (var writer = new StreamWriter(info.Machine.ProcStartFile + ".tmp", true))
                    {
                        writer.WriteLine(path);
                        writer.WriteLine(argfile);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        #region Copying

        private void CopyBinaries()
        {
            foreach (var machine in TestInfo.Machines)
            {
                Console.WriteLine("Copying binaries to {0} ... ", machine.RemotePath);

                try
                {
                    CopyBinariesTo(machine);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void CopyBinariesTo(MachineInfo machine)
        {
            if (Directory.Exists(machine.RemotePath))
            {
                Directory.Delete(machine.RemotePath, true);
            }

            CopyDirectory(TestInfo.ToolsSourceDirectory, machine.RemotePath);

            Directory.CreateDirectory(Path.Combine(machine.RemotePath, "Data"));
        }

        private void CopyDirectory(string sourcedir, string targetdir)
        {
            Directory.CreateDirectory(targetdir);

            foreach (var file in Directory.GetFiles(sourcedir))
            {
                File.Copy(file, Path.Combine(targetdir, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(sourcedir))
            {
                CopyDirectory(dir, Path.Combine(targetdir, Path.GetFileName(dir)));
            }
        }

        #endregion
    }
}
