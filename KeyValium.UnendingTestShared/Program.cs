using KeyValium.TestBench;
using KeyValium.TestBench.Shared;

namespace KeyValium.UnendingTestShared
{
    internal class Program
    {
        static int Main(string[] args)
        {
            ThreadPool.GetMaxThreads(out var maxt, out var maxcpt);
            ThreadPool.SetMaxThreads(256, maxcpt);

            if (args.Length < 1)
            {
                Console.WriteLine("Missing command line parameter.");
                Console.WriteLine("Ready.");
                Console.ReadLine();

                return -1;
            }
            else
            {
                try
                {
                    var ti = KvJson.Load<SharedTestInfo>(args[0]);

                    TestManager.Run(ti);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    Console.WriteLine("Ready.");
                    Console.ReadLine();
                    return -1;
                }
            }

            return 0;
        }
    }
}
