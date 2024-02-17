namespace KeyValium.Inspector.Controls
{
    partial class BenchmarkView
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BenchmarkView));
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbLoadData = new System.Windows.Forms.ToolStripButton();
            tsbNewBenchmark = new System.Windows.Forms.ToolStripButton();
            panel1 = new System.Windows.Forms.Panel();
            listBenchmarks = new System.Windows.Forms.ListBox();
            splitter1 = new System.Windows.Forms.Splitter();
            panelChart = new System.Windows.Forms.Panel();
            plot = new ScottPlot.FormsPlot();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            panelChart.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbLoadData, tsbNewBenchmark });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(933, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbLoadData
            // 
            tsbLoadData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbLoadData.Image = (System.Drawing.Image)resources.GetObject("tsbLoadData.Image");
            tsbLoadData.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbLoadData.Name = "tsbLoadData";
            tsbLoadData.Size = new System.Drawing.Size(23, 22);
            tsbLoadData.Text = "toolStripButton1";
            tsbLoadData.Click += tsbLoadData_Click;
            // 
            // tsbNewBenchmark
            // 
            tsbNewBenchmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbNewBenchmark.Image = (System.Drawing.Image)resources.GetObject("tsbNewBenchmark.Image");
            tsbNewBenchmark.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbNewBenchmark.Name = "tsbNewBenchmark";
            tsbNewBenchmark.Size = new System.Drawing.Size(23, 22);
            tsbNewBenchmark.Text = "New Benchmark...";
            tsbNewBenchmark.Click += tsbNewBenchmark_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(listBenchmarks);
            panel1.Dock = System.Windows.Forms.DockStyle.Left;
            panel1.Location = new System.Drawing.Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(200, 544);
            panel1.TabIndex = 1;
            // 
            // listBenchmarks
            // 
            listBenchmarks.Dock = System.Windows.Forms.DockStyle.Fill;
            listBenchmarks.FormattingEnabled = true;
            listBenchmarks.ItemHeight = 15;
            listBenchmarks.Location = new System.Drawing.Point(0, 0);
            listBenchmarks.Name = "listBenchmarks";
            listBenchmarks.Size = new System.Drawing.Size(200, 544);
            listBenchmarks.Sorted = true;
            listBenchmarks.TabIndex = 0;
            listBenchmarks.SelectedIndexChanged += listBenchmarks_SelectedIndexChanged;
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(200, 25);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(3, 544);
            splitter1.TabIndex = 2;
            splitter1.TabStop = false;
            // 
            // panelChart
            // 
            panelChart.Controls.Add(plot);
            panelChart.Dock = System.Windows.Forms.DockStyle.Fill;
            panelChart.Location = new System.Drawing.Point(203, 25);
            panelChart.Name = "panelChart";
            panelChart.Size = new System.Drawing.Size(730, 544);
            panelChart.TabIndex = 3;
            // 
            // plot
            // 
            plot.Dock = System.Windows.Forms.DockStyle.Fill;
            plot.Location = new System.Drawing.Point(0, 0);
            plot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            plot.Name = "plot";
            plot.Size = new System.Drawing.Size(730, 544);
            plot.TabIndex = 0;
            plot.Visible = false;
            // 
            // BenchmarkView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panelChart);
            Controls.Add(splitter1);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Name = "BenchmarkView";
            Size = new System.Drawing.Size(933, 569);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panelChart.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelChart;
        private System.Windows.Forms.ToolStripButton tsbLoadData;
        private System.Windows.Forms.ListBox listBenchmarks;
        private ScottPlot.FormsPlot plot;
        private System.Windows.Forms.ToolStripButton tsbNewBenchmark;
    }
}
