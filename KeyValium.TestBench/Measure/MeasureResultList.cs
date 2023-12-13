using System;
using System.Collections.Generic;

namespace KeyValium.TestBench.Measure
{
    public class MeasureResultList
    {
        public MeasureResultList()
        {
            Results = new List<MeasureResult>();
            Timestamp = DateTime.Now;
        }

        public List<MeasureResult> Results
        {
            get;
            set;
        }

        public DateTime Timestamp
        {
            get;
            set;
        }
    }
}
