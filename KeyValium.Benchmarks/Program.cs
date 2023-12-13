using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;
using KeyValium.Benchmarks.Collections;
using KeyValium.Benchmarks.KV;
using KeyValium.Benchmarks.Memory;
using KeyValium.Benchmarks.Misc;
using KeyValium.Benchmarks.Performance;
using KeyValium.Benchmarks.Threading;
using BenchmarkDotNet.Reports;

namespace KeyValium.Benchmarks
{
    class Program
    {
        public const string WORKINGPATH = @"c:\!KeyValium";

        public static string DUMPFILE = Path.Combine(WORKINGPATH, "dump");

        static void Main(string[] args)
        {
            //TestDescription.WorkingPath = WORKINGPATH;

            Directory.CreateDirectory(WORKINGPATH);

             IConfig config = null;

            // uncomment the following line to debug
#if DEBUG
            config = new DebugInProcessConfig();
#endif
            Summary summary = null;

            //summary = BenchmarkRunner.Run(typeof(Program).Assembly);
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());

            //summary = BenchmarkRunner.Run<BenchCompression>(config, null);

            //summary = BenchmarkRunner.Run<BenchMemZeroCopy>(config,null);
            //summary = BenchmarkRunner.Run<BenchSerialize>(config, args);
            //summary = BenchmarkRunner.Run<BenchTypeDef>(config, args);
            //summary = BenchmarkRunner.Run<BenchLocks>(config, args);
            //summary = BenchmarkRunner.Run<BenchHashSet>(config, args);
            //summary = BenchmarkRunner.Run<BenchKeyComparers>(config, args);
            //summary = BenchmarkRunner.Run<BenchPage>(config, args);
            //summary = BenchmarkRunner.Run<BenchInsert>(config, args);

            //summary = BenchmarkRunner.Run<BenchLmdbVsKeyValium>(config, args);

            //summary = BenchmarkRunner.Run<BenchKeyValium>(config, args);

            summary = BenchmarkRunner.Run<BenchMemMove>(config, args);
            //summary = BenchmarkRunner.Run<BenchMemCopy>(config, args);

            //summary = BenchmarkRunner.Run<BenchMemZero>(config, args);

            //summary = BenchmarkRunner.Run<BenchRead>(config, args);
            //summary = BenchmarkRunner.Run<BenchUpdate>(config, args);

            //summary = BenchmarkRunner.Run<BenchCommitRollback>(config, args);

            //summary = BenchmarkRunner.Run<BenchAllocation>(config, args);
            //summary = BenchmarkRunner.Run<BenchInterlocked>(config, args);

            //summary = BenchmarkRunner.Run<BenchCollections>(config, args);

            //summary = BenchmarkRunner.Run<BenchPrimitives>(config, args);


            //summary = BenchmarkRunner.Run<BenchGet>(config, args);

            //summary = BenchmarkRunner.Run<BenchPageRangeList>(config, args);
        }
    }
}
