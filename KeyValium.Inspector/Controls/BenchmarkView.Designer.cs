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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbLoadData = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listBenchmarks = new System.Windows.Forms.ListBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panelChart = new System.Windows.Forms.Panel();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbLoadData});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(933, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbLoadData
            // 
            this.tsbLoadData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLoadData.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadData.Image")));
            this.tsbLoadData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadData.Name = "tsbLoadData";
            this.tsbLoadData.Size = new System.Drawing.Size(23, 22);
            this.tsbLoadData.Text = "toolStripButton1";
            this.tsbLoadData.Click += new System.EventHandler(this.tsbLoadData_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.listBenchmarks);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 544);
            this.panel1.TabIndex = 1;
            // 
            // listBenchmarks
            // 
            this.listBenchmarks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBenchmarks.FormattingEnabled = true;
            this.listBenchmarks.ItemHeight = 15;
            this.listBenchmarks.Location = new System.Drawing.Point(0, 0);
            this.listBenchmarks.Name = "listBenchmarks";
            this.listBenchmarks.Size = new System.Drawing.Size(200, 544);
            this.listBenchmarks.Sorted = true;
            this.listBenchmarks.TabIndex = 0;
            this.listBenchmarks.SelectedIndexChanged += new System.EventHandler(this.listBenchmarks_SelectedIndexChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 544);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // panelChart
            // 
            this.panelChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelChart.Location = new System.Drawing.Point(203, 25);
            this.panelChart.Name = "panelChart";
            this.panelChart.Size = new System.Drawing.Size(730, 544);
            this.panelChart.TabIndex = 3;
            // 
            // BenchmarkView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelChart);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "BenchmarkView";
            this.Size = new System.Drawing.Size(933, 569);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelChart;
        private System.Windows.Forms.ToolStripButton tsbLoadData;
        private System.Windows.Forms.ListBox listBenchmarks;
    }
}
