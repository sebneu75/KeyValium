using KeyValium.Collections;
using KeyValium.TestBench;
using KeyValium.TestBench.Shared;
using KeyValium.UnendingTestSharedController.Private;
using System.Runtime.InteropServices;

namespace KeyValium.UnendingTestSharedController
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TestJson();

            var c = new MainController();

            c.Run();

            // save database and logs

            Console.WriteLine("Ready.");
            Console.ReadLine();
        }

        private static void TestJson()
        {
            var x = new SharedTestInfo()
            {
                DatabaseInfos = { new DatabaseInfo() { Filename = "test.kvlm", SharingMode = InternalSharingModes.Exclusive, Instances = 3, Readers = 4 } },
                Machines = { new MachineInfo() { LocalPath = "1", Name = "2", ProcStartFile = "3", RemotePath = "4" } },
                ProcessCount = 4,
            };

            KvJson.Save(x, "test.json");

            var y = KvJson.Load<SharedTestInfo>("test.json");
        }
    }
}
