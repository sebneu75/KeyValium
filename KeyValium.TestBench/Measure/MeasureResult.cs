using System.Diagnostics;

namespace KeyValium.TestBench.Measure
{
    public class MeasureResult
    {
        public string Title
        {
            get;
            set;
        }

        public int Cycle
        {
            get;
            set;
        }

        public long OperationCount
        {
            get;
            set;
        }

        public long Ticks
        {
            get;
            set;
        }

        public double KiloOperationsPerSecond
        {
            get
            {
                var seconds = (double)Ticks / Stopwatch.Frequency;
                var ops = OperationCount / seconds / 1000.0;

                return ops;
            }
        }

        public double MicroSecondsPerOperation
        {
            get
            {
                var seconds = (double)Ticks / Stopwatch.Frequency;
                if (OperationCount == 0)
                {
                    return seconds * 1000000.0;
                }

                var mysperitem = seconds / OperationCount * 1000000.0;

                return mysperitem;
            }
        }

        public override string ToString()
        {
            if (OperationCount <= 0)
            {
                return string.Format("{0}: {1} ticks ", Title, Ticks);
            }

            var tickspersecond = Stopwatch.Frequency;
            var ms = (double)Ticks / Stopwatch.Frequency * 1000.0;
            var mysperitem = ms / OperationCount * 1000.0;
            var itemspers = OperationCount / ms * 1000.0;

            return string.Format("{0} ({1} item(s)): {2:0.000}ms ({3:0.000}µs per item / {4:0.000} items/second)", Title, OperationCount, ms, mysperitem, itemspers);
        }
    }
}
