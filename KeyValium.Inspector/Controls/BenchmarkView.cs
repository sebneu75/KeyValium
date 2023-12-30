using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
