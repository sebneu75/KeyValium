
namespace KeyValium.Inspector
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbOpenDatabase = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            tsbMeta0 = new System.Windows.Forms.ToolStripButton();
            tsbMeta1 = new System.Windows.Forms.ToolStripButton();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            tabMain = new System.Windows.Forms.TabControl();
            tabProperties = new System.Windows.Forms.TabPage();
            propertyView = new Controls.PropertyView();
            tabFileMap = new System.Windows.Forms.TabPage();
            fileMapView = new Controls.FileMapView();
            tabPageMap = new System.Windows.Forms.TabPage();
            pageMapView = new Controls.PageMapView();
            tabFreespace = new System.Windows.Forms.TabPage();
            freeSpaceView = new Controls.FreeSpaceView();
            tabBenchmark = new System.Windows.Forms.TabPage();
            benchmarkView = new Controls.BenchmarkView();
            panelLoading = new System.Windows.Forms.Panel();
            cmdCancelLoading = new System.Windows.Forms.Button();
            pbLoading = new System.Windows.Forms.ProgressBar();
            label14 = new System.Windows.Forms.Label();
            toolStrip1.SuspendLayout();
            tabMain.SuspendLayout();
            tabProperties.SuspendLayout();
            tabFileMap.SuspendLayout();
            tabPageMap.SuspendLayout();
            tabFreespace.SuspendLayout();
            tabBenchmark.SuspendLayout();
            panelLoading.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new System.Drawing.Point(0, 605);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1128, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbOpenDatabase, toolStripSeparator1, toolStripLabel1, tsbMeta0, tsbMeta1 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(1128, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbOpenDatabase
            // 
            tsbOpenDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbOpenDatabase.Image = Properties.Resources.ZoomHS;
            tsbOpenDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbOpenDatabase.Name = "tsbOpenDatabase";
            tsbOpenDatabase.Size = new System.Drawing.Size(23, 22);
            tsbOpenDatabase.Text = "Open Database...";
            tsbOpenDatabase.Click += tsbOpenDatabase_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(73, 22);
            toolStripLabel1.Text = "Active Meta:";
            // 
            // tsbMeta0
            // 
            tsbMeta0.BackColor = System.Drawing.SystemColors.Control;
            tsbMeta0.CheckOnClick = true;
            tsbMeta0.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbMeta0.Image = (System.Drawing.Image)resources.GetObject("tsbMeta0.Image");
            tsbMeta0.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbMeta0.Name = "tsbMeta0";
            tsbMeta0.Size = new System.Drawing.Size(23, 22);
            tsbMeta0.CheckedChanged += tsbMeta0_CheckedChanged;
            // 
            // tsbMeta1
            // 
            tsbMeta1.CheckOnClick = true;
            tsbMeta1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbMeta1.Image = (System.Drawing.Image)resources.GetObject("tsbMeta1.Image");
            tsbMeta1.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbMeta1.Name = "tsbMeta1";
            tsbMeta1.Size = new System.Drawing.Size(23, 22);
            tsbMeta1.CheckedChanged += tsbMeta1_CheckedChanged;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "KeyValium-Databases|*.kvlm|All files|*.*";
            openFileDialog.SupportMultiDottedExtensions = true;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabProperties);
            tabMain.Controls.Add(tabFileMap);
            tabMain.Controls.Add(tabPageMap);
            tabMain.Controls.Add(tabFreespace);
            tabMain.Controls.Add(tabBenchmark);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Location = new System.Drawing.Point(0, 25);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new System.Drawing.Size(1128, 532);
            tabMain.TabIndex = 2;
            // 
            // tabProperties
            // 
            tabProperties.Controls.Add(propertyView);
            tabProperties.Location = new System.Drawing.Point(4, 24);
            tabProperties.Name = "tabProperties";
            tabProperties.Padding = new System.Windows.Forms.Padding(3);
            tabProperties.Size = new System.Drawing.Size(1120, 504);
            tabProperties.TabIndex = 0;
            tabProperties.Text = "Properties";
            tabProperties.UseVisualStyleBackColor = true;
            // 
            // propertyView
            // 
            propertyView.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyView.Location = new System.Drawing.Point(3, 3);
            propertyView.Name = "propertyView";
            propertyView.Presenter = null;
            propertyView.Size = new System.Drawing.Size(1114, 498);
            propertyView.TabIndex = 0;
            // 
            // tabFileMap
            // 
            tabFileMap.Controls.Add(fileMapView);
            tabFileMap.Location = new System.Drawing.Point(4, 24);
            tabFileMap.Name = "tabFileMap";
            tabFileMap.Padding = new System.Windows.Forms.Padding(3);
            tabFileMap.Size = new System.Drawing.Size(1120, 504);
            tabFileMap.TabIndex = 1;
            tabFileMap.Text = "File Map";
            tabFileMap.UseVisualStyleBackColor = true;
            // 
            // fileMapView
            // 
            fileMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            fileMapView.Location = new System.Drawing.Point(3, 3);
            fileMapView.Name = "fileMapView";
            fileMapView.Presenter = null;
            fileMapView.Size = new System.Drawing.Size(1114, 498);
            fileMapView.TabIndex = 0;
            // 
            // tabPageMap
            // 
            tabPageMap.Controls.Add(pageMapView);
            tabPageMap.Location = new System.Drawing.Point(4, 24);
            tabPageMap.Name = "tabPageMap";
            tabPageMap.Padding = new System.Windows.Forms.Padding(3);
            tabPageMap.Size = new System.Drawing.Size(1120, 504);
            tabPageMap.TabIndex = 2;
            tabPageMap.Text = "Page";
            tabPageMap.UseVisualStyleBackColor = true;
            // 
            // pageMapView
            // 
            pageMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            pageMapView.Location = new System.Drawing.Point(3, 3);
            pageMapView.MaxPagenumber = null;
            pageMapView.Name = "pageMapView";
            pageMapView.PageMap = null;
            pageMapView.Presenter = null;
            pageMapView.Size = new System.Drawing.Size(1114, 498);
            pageMapView.TabIndex = 0;
            // 
            // tabFreespace
            // 
            tabFreespace.Controls.Add(freeSpaceView);
            tabFreespace.Location = new System.Drawing.Point(4, 24);
            tabFreespace.Name = "tabFreespace";
            tabFreespace.Padding = new System.Windows.Forms.Padding(3);
            tabFreespace.Size = new System.Drawing.Size(1120, 504);
            tabFreespace.TabIndex = 4;
            tabFreespace.Text = "Free Space";
            tabFreespace.UseVisualStyleBackColor = true;
            // 
            // freeSpaceView
            // 
            freeSpaceView.Dock = System.Windows.Forms.DockStyle.Fill;
            freeSpaceView.Location = new System.Drawing.Point(3, 3);
            freeSpaceView.Name = "freeSpaceView";
            freeSpaceView.Presenter = null;
            freeSpaceView.Size = new System.Drawing.Size(1114, 498);
            freeSpaceView.TabIndex = 0;
            // 
            // tabBenchmark
            // 
            tabBenchmark.Controls.Add(benchmarkView);
            tabBenchmark.Location = new System.Drawing.Point(4, 24);
            tabBenchmark.Name = "tabBenchmark";
            tabBenchmark.Padding = new System.Windows.Forms.Padding(3);
            tabBenchmark.Size = new System.Drawing.Size(1120, 504);
            tabBenchmark.TabIndex = 3;
            tabBenchmark.Text = "Benchmarks";
            tabBenchmark.UseVisualStyleBackColor = true;
            // 
            // benchmarkView
            // 
            benchmarkView.Dock = System.Windows.Forms.DockStyle.Fill;
            benchmarkView.Location = new System.Drawing.Point(3, 3);
            benchmarkView.Name = "benchmarkView";
            benchmarkView.Presenter = null;
            benchmarkView.Size = new System.Drawing.Size(1114, 498);
            benchmarkView.TabIndex = 0;
            // 
            // panelLoading
            // 
            panelLoading.BackColor = System.Drawing.SystemColors.Info;
            panelLoading.Controls.Add(cmdCancelLoading);
            panelLoading.Controls.Add(pbLoading);
            panelLoading.Controls.Add(label14);
            panelLoading.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelLoading.Location = new System.Drawing.Point(0, 557);
            panelLoading.Name = "panelLoading";
            panelLoading.Size = new System.Drawing.Size(1128, 48);
            panelLoading.TabIndex = 34;
            panelLoading.Visible = false;
            // 
            // cmdCancelLoading
            // 
            cmdCancelLoading.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cmdCancelLoading.Location = new System.Drawing.Point(1040, 13);
            cmdCancelLoading.Name = "cmdCancelLoading";
            cmdCancelLoading.Size = new System.Drawing.Size(76, 23);
            cmdCancelLoading.TabIndex = 2;
            cmdCancelLoading.Text = "Cancel";
            cmdCancelLoading.UseVisualStyleBackColor = true;
            cmdCancelLoading.Click += cmdCancelLoading_Click;
            // 
            // pbLoading
            // 
            pbLoading.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pbLoading.Location = new System.Drawing.Point(117, 17);
            pbLoading.Maximum = 1000;
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(917, 15);
            pbLoading.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            pbLoading.TabIndex = 1;
            pbLoading.Value = 678;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label14.Location = new System.Drawing.Point(3, 17);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(108, 15);
            label14.TabIndex = 0;
            label14.Text = "Loading File Map...";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1128, 627);
            Controls.Add(tabMain);
            Controls.Add(panelLoading);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Name = "frmMain";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "KeyValium.Inspector";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            tabMain.ResumeLayout(false);
            tabProperties.ResumeLayout(false);
            tabFileMap.ResumeLayout(false);
            tabPageMap.ResumeLayout(false);
            tabFreespace.ResumeLayout(false);
            tabBenchmark.ResumeLayout(false);
            panelLoading.ResumeLayout(false);
            panelLoading.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbOpenDatabase;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabProperties;
        private Controls.PropertyView propertyView;
        private System.Windows.Forms.TabPage tabFileMap;
        private Controls.FileMapView fileMapView;
        private System.Windows.Forms.TabPage tabPageMap;
        private Controls.PageMapView pageMapView;
        private System.Windows.Forms.TabPage tabBenchmark;
        private System.Windows.Forms.TabPage tabFreespace;
        private Controls.FreeSpaceView freeSpaceView;
        private Controls.BenchmarkView benchmarkView;
        private System.Windows.Forms.Panel panelLoading;
        private System.Windows.Forms.Button cmdCancelLoading;
        private System.Windows.Forms.ProgressBar pbLoading;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton tsbMeta0;
        private System.Windows.Forms.ToolStripButton tsbMeta1;
    }
}

