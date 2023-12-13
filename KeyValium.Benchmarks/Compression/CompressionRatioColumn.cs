using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Compression
{
    public class CompressionRatioColumn : IColumn
    {
        public string Id { get; }
        public string ColumnName { get; }

        public CompressionRatioColumn()
        {
            ColumnName = "CompRatio";
            Id = "CompRatio";
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => $"Custom '{ColumnName}' ratio column";
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            if (benchmarkCase.Descriptor.WorkloadMethod.Name==nameof(BenchCompression.Compress))
            {
                var ratio = BenchCompression.GetCompressionRatio(benchmarkCase.Parameters);
                return string.Format("{0:#.00%}", ratio);
            }

            return "";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            return GetValue(summary, benchmarkCase);
        }
        public override string ToString() => ColumnName;

    }
}
