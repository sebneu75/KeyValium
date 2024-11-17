using KeyValium.Inspector.Benchmarks;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using KeyValium.TestBench;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class BenchmarkView : UserControl, IBenchmarkView
    {
        public BenchmarkView()
        {
            InitializeComponent();

            //CreateChart();

            //panelChart.Controls.Add(Chart);
        }

        //private void CreateChart()
        //{
        //    Chart = new Chart();
        //    Chart.Dock = DockStyle.Fill;
        //}

        #region IBenchmark

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BenchmarkPresenter Presenter
        {
            get;
            set;
        }

        #endregion

        //public Chart Chart
        //{
        //    get;
        //    private set;
        //}

        private void tsbLoadData_Click(object sender, EventArgs e)
        {
            //_benchmarks = Presenter.LoadBenchmarks();

            //listBenchmarks.Items.Clear();

            //var items = _benchmarks.GroupBy(x => x.ParameterName).Select(x => x.Key).ToArray();

            //listBenchmarks.Items.AddRange(items);
        }

        //private List<TestDescription> _benchmarks;

        private void listBenchmarks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBenchmarks.SelectedItem != null)
            {
                var name = listBenchmarks.SelectedItem.ToString();
                //UpdateChart(name);
            }

        }

        private void tsbNewBenchmark_Click(object sender, EventArgs e)
        {
            TestDescription.WorkingPath = Path.GetTempPath();

            //var config = ManualConfig.CreateEmpty()
            //    .AddJob(Job.Default // Adding first job
            //    .WithRuntime(CoreRuntime.Core80) // .NET Framework 4.7.2
            //    .WithPlatform(Platform.X64) // Run as x64 application
            //    .WithJit(Jit.RyuJit) // Use default RyuJIT
            //    .WithGcServer(false) // Use Server GC                
            //    ).WithOption(ConfigOptions.DisableLogFile, true)
            //    .WithOption(ConfigOptions.KeepBenchmarkFiles, false); ;

            ///*
            //    .AddJob(Job.Default // Adding second job
            //    .AsBaseline() // It will be marked as baseline
            //    .WithEnvironmentVariable("Key", "Value") // Setting an environment variable
            //    .WithWarmupCount(0) // Disable warm-up stage
            //);
            //*/

            //var summary = BenchmarkRunner.Run<BenchKeyValium>(config, null);

            //var results = new List<BenchmarkResult>();

            //for (int i = 0; i < summary.BenchmarksCases.Length; i++)
            //{
            //    var bc = summary.BenchmarksCases[i];

            //    var br = new BenchmarkResult();

            //    br.Name = bc.Descriptor.Categories[0];
            //    br.Max = Get(summary, i, "Max");
            //    br.Mean = Get(summary, i, "Mean");
            //    br.Median = Get(summary, i, "Median");
            //    br.Min = Get(summary, i, "Min");
            //    br.Ops = Get(summary, i, "Op/s");

            //    results.Add(br);
            //}


            //var plt = plot.Plot;
            //plt.Clear();

            //double[] values = results.Select(x => x.Ops / 1000000.0).ToArray();

            //var positions = new double[results.Count];
            //for (int i = 0; i < results.Count; i++)
            //{
            //    positions[i] = i;
            //}
            //string[] labels = results.Select(x => x.Name).ToArray(); ;
            //var bar = plt.AddBar(values, positions);
            //bar.ValueFormatter = (x) => x.ToString("#0.00");
            //bar.ShowValuesAboveBars = true;
            //plt.XTicks(positions, labels);
            //plt.XAxis.TickLabelStyle(rotation: 30.0f);
            //plt.SetAxisLimits(yMin: 0);
            //plt.XLabel("Operation");
            //plt.YLabel("Millions per second");

            //plot.Refresh();

            //plot.Visible = true;

            return;
        }

        //private double Get(Summary summary, int caseindex, string name)
        //{
        //    var colindex = -1;

        //    for (int i = 0; i < summary.Table.ColumnCount; i++)
        //    {
        //        if (summary.Table.Columns[i].Header == name)
        //        {
        //            colindex = i;
        //            break;
        //        }
        //    }

        //    if (colindex < 0)
        //    {
        //        throw new ArgumentException("Column not found!");
        //    }

        //    var val = summary.Table.FullContent[caseindex][colindex];

        //    var mul = 1.0;

        //    if (val.EndsWith("ns"))
        //    {
        //        val = val.Substring(0, val.Length - 2).Trim();
        //        mul = 1.0;
        //    }
        //    else if (val.EndsWith("μs"))
        //    {
        //        val = val.Substring(0, val.Length - 2).Trim();
        //        mul = 1000.0;
        //    }
        //    else if (val.EndsWith("µs"))
        //    {
        //        val = val.Substring(0, val.Length - 2).Trim();
        //        mul = 1000.0;
        //    }
        //    else if (val.EndsWith("ms"))
        //    {
        //        val = val.Substring(0, val.Length - 2).Trim();
        //        mul = 1000000.0;
        //    }
        //    else if (val.EndsWith("s"))
        //    {
        //        val = val.Substring(0, val.Length - 1).Trim();
        //        mul = 1000000000.0;
        //    }

        //    return double.Parse(val, CultureInfo.InvariantCulture) * mul;
        //}

        //private void UpdateChart(string name)
        //{
        //    Chart.Series.Clear();

        //    Chart.ChartAreas.Clear();
        //    Chart.Legends.Clear();
        //    Chart.ChartAreas.Add("Default");

        //    //Chart.ChartAreas[0].Area3DStyle = new ChartArea3DStyle();
        //    //Chart.ChartAreas[0].Area3DStyle.Enable3D = true;
        //    //Chart.ChartAreas[0].Area3DStyle.WallWidth=0;

        //    Chart.ChartAreas[0].AxisX.MinorTickMark.Enabled = true;
        //    Chart.ChartAreas[0].AxisX.MajorTickMark.Enabled = true;
        //    Chart.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
        //    Chart.ChartAreas[0].AxisX.IsMarginVisible = true;
        //    Chart.ChartAreas[0].AxisX.LabelStyle.Interval = 1.0;
        //    Chart.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = 0.5;
        //    Chart.ChartAreas[0].AxisX.Title = "Action";

        //    Chart.ChartAreas[0].AxisY.IsLogarithmic = true;
        //    Chart.ChartAreas[0].AxisY.Title = "kOps";

        //    var l = Chart.Legends.Add("1");
        //    l.Title = name;

        //    //var tds = _benchmarks.Where(x => x.ParameterName == name).ToList();

        //    //var paramgroups = tds.GroupBy(x => x.ParameterValue).OrderBy(x => int.Parse(x.Key)).ToList();

        //    //foreach (var paramgroup in paramgroups)
        //    //{
        //    //    var serie = Chart.Series.Add(paramgroup.Key);
        //    //    serie.ChartType = SeriesChartType.Column;
        //    //    serie.IsValueShownAsLabel = false;
        //    //    serie.LabelBackColor = Color.White;
        //    //    //serie.Color = Color.Green;

        //    //    var measurements = paramgroup.SelectMany(x => x.Measure.Measurements).SelectMany(x => x.Results).ToList();
        //    //    var measuregroups = measurements.GroupBy(x => x.Title).ToList();

        //    //    foreach (var measuregroup in measuregroups)
        //    //    {
        //    //        var avg = measuregroup.Average(x => x.KiloOperationsPerSecond);
        //    //        var num = serie.Points.AddXY(measuregroup.Key, avg);
        //    //        serie.Points[num].ToolTip = string.Format("{0}", avg);
        //    //    }
        //    //}



        //    //var lookup = new Dictionary<string, string>();

        //    //var allgroups = Requests.GroupBy((x) => x.Method).OrderBy((x) => x.Key);

        //    //int i = 0;
        //    //foreach (var group1 in allgroups)
        //    //{
        //    //    lookup.Add(group1.Key, string.IsNullOrWhiteSpace(group1.Key) ? " - " : group1.Key);
        //    //    i++;
        //    //}

        //    //foreach (var group1 in allgroups)
        //    //{
        //    //    var num = serie1.Points.AddXY(lookup[group1.Key], group1.Count());
        //    //    serie1.Points[num].ToolTip = string.Format("Insgesamt: {0}", group1.Count());
        //    //}

        //    //var programs = Requests.GroupBy((x) => x.Program).OrderBy((x) => x.Key);
        //    //foreach (var program in programs)
        //    //{
        //    //    var serie = Chart.Series.Add(program.Key ?? " - ");
        //    //    serie.ChartType = SeriesChartType.Column;
        //    //    serie.IsValueShownAsLabel = true;
        //    //    serie.LabelBackColor = Color.White;

        //    //    var groups = program.GroupBy((x) => x.Method);
        //    //    foreach (var group1 in allgroups)
        //    //    {
        //    //        var x = lookup[group1.Key];
        //    //        var tmp = groups.Where((q) => q.Key == group1.Key).ToList();
        //    //        var y = tmp.Count > 0 ? tmp[0].Count() : 0;

        //    //        var num = serie.Points.AddXY(x, y == 0 ? (object)null : y);
        //    //        serie.Points[num].ToolTip = string.Format("{0}: {1}", program.Key ?? " - ", y == 0 ? "" : y.ToString());

        //    //        //Console.WriteLine(x.ToString() + " - " + y.ToString());
        //    //    }
        //    //}
        //}
    }
}
