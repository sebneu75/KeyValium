using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KeyValium.TestBench.Measure
{
    public class Measurer
    {
        public Measurer()
        {
            Measurements = new List<MeasureResultList>();
        }

        public MeasureResult MeasureTime(string title, int cycle, long items, Action action)
        {
            EnsureCurrentMeasurement();

            var sw = new Stopwatch();
            sw.Restart();
            action.Invoke();
            sw.Stop();

            var result = new MeasureResult() { Title = title, Cycle = cycle, OperationCount = items, Ticks = sw.ElapsedTicks };
            CurrentMeasurement.Results.Add(result);

            return result;
        }

        private void EnsureCurrentMeasurement()
        {
            if (CurrentMeasurement == null)
            {
                CurrentMeasurement = new MeasureResultList();
            }
        }

        public MeasureResultList CurrentMeasurement
        {
            get;
            set;
        }

        public List<MeasureResultList> Measurements
        {
            get;
            set;
        }

        public void PrintLastResult()
        {
            PrintLastResult("kop/s", x => x.KiloOperationsPerSecond, true);
            PrintLastResult("µs/op", x => x.MicroSecondsPerOperation);
        }

        public TestDescription TestDescription
        {
            get;
            set;
        }

        private void PrintLastResult(string title, Func<MeasureResult, double> selector, bool first = false)
        {
            var measure = Measurements.LastOrDefault();
            if (measure == null)
            {
                Console.WriteLine("No MeasurementResultList found!");
                return;
            }

            if (first)
            {
                Console.WriteLine();
                Console.WriteLine("*** {0}", TestDescription.Name);
                Console.WriteLine(new string('-', 132));
            }


            var groups = measure.Results.GroupBy(x => x.Title).OrderBy(x => x.Key).ToList();

            var header = string.Format("|{0,-20}|{1,16}|{2,16}{3,10}|{4,16}{5,10}|{6,16}{7,10}|",
                title, "Operations", "Minimum", "%", "Maximum", "%", "Average", "%");
            Console.WriteLine(header);

            Console.WriteLine(new string('-', 132));

            foreach (var group in groups)
            {
                var firstmin = GetFirstValue(group.Key, selector, x => x.Min());
                var min = group.Select(x => selector(x)).Min();
                var firstmax = GetFirstValue(group.Key, selector, x => x.Max());
                var max = group.Select(x => selector(x)).Max();
                var firstavg = GetFirstValue(group.Key, selector, x => x.Average());
                var avg = group.Select(x => selector(x)).Average();

                var count = group.Sum(x => x.OperationCount);

                var line = string.Format("|{0,-20}|{1,16}|{2,16:#0.000}{3,10:+#0.0%;-#0.0%;#0.0%}|{4,16:#0.000}{5,10:+#0.0%;-#0.0%;#0.0%}|{6,16:#0.000}{7,10:+#0.0%;-#0.0%;#0.0%}|",
                    group.Key, count, min, GetPercentage(firstmin, min), max, GetPercentage(firstmax, max), avg, GetPercentage(firstavg, avg));

                Console.WriteLine(line);
            }

            Console.WriteLine(new string('-', 132));
        }

        private double? GetPercentage(double? firstmin, double min)
        {
            if (firstmin.HasValue && firstmin.Value != 0.0)
            {
                return min / firstmin.Value - 1.0;
            }

            return null;
        }

        private double? GetFirstValue(string title, Func<MeasureResult, double> selector, Func<IEnumerable<double>, double> agg)
        {
            var first = Measurements.Skip(3).FirstOrDefault();
            if (first == null)
            {
                return null;
            }

            var items = first.Results.Where(x => x.Title == title).Select(x => selector(x)).ToList();
            if (items.Count() == 0)
            {
                return null;
            }

            return agg(items);
        }

        internal void Finish()
        {
            if (CurrentMeasurement != null)
            {
                Measurements.Add(CurrentMeasurement);
                CurrentMeasurement = null;
            }
        }
    }
}
