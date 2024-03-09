using System.Runtime.InteropServices;

namespace KeyValium.UnendingTestSharedController
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = new Controller();
            c.Run();

            Console.WriteLine("Ready.");
            Console.ReadLine();
        }
    }
}
