using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector.Benchmarks
{
    internal class BenchmarkResult
    {
        public BenchmarkResult() 
        { 
        
        }

        public string Name
        { 
            get; 
            set; 
        }

        public double Mean
        {
            get;
            set;
        }

        public double Median
        {
            get;
            set;
        }

        public double Min
        {
            get;
            set;
        }

        public double Max
        {
            get;
            set;
        }

        public double Ops
        {
            get;
            set;
        }
    }
}
