using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Performance
{
    internal class PerformanceInfo
    {
        public string Name;

        public double TotalTicks;

        public double MaxTicks = long.MinValue;

        public double MinTicks = long.MaxValue;

        public long Count;

        public List<double> Values=new List<double>();

        public double AverageTicks
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }

                return TotalTicks / (double)Count;
            }
        }

        public double Rel
        {
            get
            {
                return Math.Log10((MaxTicks) / (Math.Max(MinTicks, 1)));
            }
        }

        public double TicksToNs(double ticks)
        {
            return (1000000000.0 / Stopwatch.Frequency) * ticks ;
        }

        public void AddValue(double ticks)
        {
            Count++;
            TotalTicks += ticks;
            MaxTicks = Math.Max(MaxTicks, ticks);
            MinTicks = Math.Min(MinTicks, ticks);
            Values.Add(ticks);
        }

        public override string ToString()
        {
            return string.Format("Name: {0} Count: {1} Min: {2:0.0}ns Max: {3:0.0}ns Rel: {4:0.0} Average: {5:0.0}ns", Name, Count,
                TicksToNs(MinTicks), TicksToNs(MaxTicks), Rel, TicksToNs(AverageTicks));
        }
    }
}
