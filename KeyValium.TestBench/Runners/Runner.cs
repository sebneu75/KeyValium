using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Runners
{
    internal class Runner
    {
        public Runner()
        {

        }

        public void RunTests(int count = 1)
        {
            RunItems(GetTests(false).Cast<RunnerBase>().ToList(), count);
        }

        public void RunTests(string filter, int count = 1)
        {
            var name = filter.ToLowerInvariant();

            var items = GetTests(true).Cast<RunnerBase>().Where(x => x.Name.ToLowerInvariant().Contains(name)).ToList();

            for (int i = 0; i < count; i++)
            {
                RunItems(items, count);
            }
        }

        public void RunBenchmarks(int count = 1)
        {
            RunItems(GetBenchmarks().Cast<RunnerBase>().ToList(), count);
        }

        public void RunBenchmarks(string filter, int count = 1)
        {
            var name = filter.ToLowerInvariant();

            var items = GetBenchmarks().Cast<RunnerBase>().Where(x => x.Name.ToLowerInvariant().Contains(name)).ToList();

            RunItems(items, count);
        }

        private void RunItems(List<RunnerBase> items, int count)
        {
            var successes = new List<string>();
            var failures = new List<Tuple<string, Exception>>();

            var opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = 16;

            foreach (var item in items)
            {
                try
                {
                    item.Run(count);
                    successes.Add(item.Name);
                }
                catch (Exception ex)
                {
                    failures.Add(new Tuple<string, Exception>(item.DisplayName, ex));
                }

                Console.WriteLine("***********************");
            }

            //Parallel.ForEach(items, opt, item =>
            //{
            //    try
            //    {
            //        Console.WriteLine("Item '{0}': Running...", item.Name);

            //        item.Run(count);

            //        successes.Add(item.Name);
            //    }
            //    catch (Exception ex)
            //    {
            //        failures.Add(new Tuple<string, Exception>(item.Name, ex));
            //    }

            //    Console.WriteLine("***********************");
            //});

            foreach (var item in failures)
            {
                Tools.WriteError(item.Item2,"Item '{0}': FAIL", item.Item1);
            }

            foreach (var item in successes)
            {
                Tools.WriteSuccess("Item '{0}': SUCCESS", item);
            }
        }


        public List<TestBase> GetTests(bool endless)
        {
            var ret = new List<TestBase>();

            var basetype = typeof(TestBase);

            var subclasses = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));

            foreach (var subclass in subclasses)
            {
                var test = Activator.CreateInstance(subclass) as TestBase;
                if (endless || !test.IsEndless)
                {
                    ret.Add(test);
                }
            }

            return ret.OrderBy(x => x.Name).ToList();
        }

        public List<BenchmarkBase> GetBenchmarks()
        {
            var ret = new List<BenchmarkBase>();

            var basetype = typeof(BenchmarkBase);

            var subclasses = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));

            foreach (var subclass in subclasses)
            {
                ret.Add(Activator.CreateInstance(subclass) as BenchmarkBase);
            }

            return ret.OrderBy(x => x.Name).ToList();
        }
    }
}
