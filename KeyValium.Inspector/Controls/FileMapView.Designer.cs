namespace KeyValium.Inspector.Controls
{
    partial class FileMapView
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
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            blockView = new BlockView();
            bvLegend = new BlockViewLegend();
            panelRight = new System.Windows.Forms.Panel();
            splitter1 = new System.Windows.Forms.Splitter();
            panelRight.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(760, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // blockView
            // 
            blockView.AutoScroll = true;
            blockView.AutoScrollMinSize = new System.Drawing.Size(0, 378392);
            blockView.BlockMargin = new System.Windows.Forms.Padding(1);
            blockView.BlockSize = new System.Drawing.Size(12, 12);
            blockView.Dock = System.Windows.Forms.DockStyle.Fill;
            blockView.FileMap = null;
            blockView.InnerMargin = new System.Windows.Forms.Padding(4);
            blockView.Location = new System.Drawing.Point(0, 25);
            blockView.Name = "blockView";
            blockView.Size = new System.Drawing.Size(565, 402);
            blockView.TabIndex = 1;
            blockView.ShowPage += blockView_ShowPage;
            // 
            // bvLegend
            // 
            bvLegend.Dock = System.Windows.Forms.DockStyle.Top;
            bvLegend.Location = new System.Drawing.Point(0, 0);
            bvLegend.Name = "bvLegend";
            bvLegend.Size = new System.Drawing.Size(192, 307);
            bvLegend.TabIndex = 2;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(bvLegend);
            panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            panelRight.Location = new System.Drawing.Point(568, 25);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(192, 402);
            panelRight.TabIndex = 3;
            // 
            // splitter1
            // 
            splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            splitter1.Location = new System.Drawing.Point(565, 25);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(3, 402);
            splitter1.TabIndex = 4;
            splitter1.TabStop = false;
            // 
            // FileMapView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(blockView);
            Controls.Add(splitter1);
            Controls.Add(panelRight);
            Controls.Add(toolStrip1);
            Name = "FileMapView";
            Size = new System.Drawing.Size(760, 427);
            panelRight.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private BlockView blockView;
        private BlockViewLegend bvLegend;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Splitter splitter1;
    }
}
