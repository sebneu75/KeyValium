using KeyValium.TestBench;
using KeyValium.TestBench.Runners;
using System;
using System.IO;
using System.Text;

namespace KeyValium.Tests
{
    class Program
    {
        public const string WORKINGPATH = @"c:\!KeyValium";

        public static string DUMPFILE = Path.Combine(WORKINGPATH, "dump");

        static void Main(string[] args)
        {
            Directory.CreateDirectory(WORKINGPATH);
            TestDescription.WorkingPath = WORKINGPATH;
        }
    }
}
