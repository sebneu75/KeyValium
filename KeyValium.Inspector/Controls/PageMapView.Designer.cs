namespace KeyValium.Inspector.Controls
{
    partial class PageMapView
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
            tabViews = new System.Windows.Forms.TabControl();
            tabEntries = new System.Windows.Forms.TabPage();
            entryListView = new EntryListView();
            splitter1 = new System.Windows.Forms.Splitter();
            pageMapFooter = new PageMapFooter();
            tabHexView = new System.Windows.Forms.TabPage();
            hexView = new HexView();
            pageMapHeader = new PageMapHeader();
            tsPage = new System.Windows.Forms.ToolStrip();
            tsbBackward = new System.Windows.Forms.ToolStripButton();
            tsbForward = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            txtPagenumber = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbPrevious = new System.Windows.Forms.ToolStripButton();
            tsbNext = new System.Windows.Forms.ToolStripButton();
            tabViews.SuspendLayout();
            tabEntries.SuspendLayout();
            tabHexView.SuspendLayout();
            tsPage.SuspendLayout();
            SuspendLayout();
            // 
            // tabViews
            // 
            tabViews.Controls.Add(tabEntries);
            tabViews.Controls.Add(tabHexView);
            tabViews.Dock = System.Windows.Forms.DockStyle.Fill;
            tabViews.Location = new System.Drawing.Point(0, 116);
            tabViews.Name = "tabViews";
            tabViews.SelectedIndex = 0;
            tabViews.Size = new System.Drawing.Size(778, 465);
            tabViews.TabIndex = 6;
            // 
            // tabEntries
            // 
            tabEntries.Controls.Add(entryListView);
            tabEntries.Controls.Add(splitter1);
            tabEntries.Controls.Add(pageMapFooter);
            tabEntries.Location = new System.Drawing.Point(4, 24);
            tabEntries.Name = "tabEntries";
            tabEntries.Padding = new System.Windows.Forms.Padding(3);
            tabEntries.Size = new System.Drawing.Size(770, 437);
            tabEntries.TabIndex = 0;
            tabEntries.Text = "Entries";
            tabEntries.UseVisualStyleBackColor = true;
            // 
            // entryListView
            // 
            entryListView.Dock = System.Windows.Forms.DockStyle.Fill;
            entryListView.Location = new System.Drawing.Point(3, 3);
            entryListView.Name = "entryListView";
            entryListView.Size = new System.Drawing.Size(764, 269);
            entryListView.TabIndex = 0;
            // 
            // splitter1
            // 
            splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            splitter1.Location = new System.Drawing.Point(3, 272);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(764, 3);
            splitter1.TabIndex = 2;
            splitter1.TabStop = false;
            // 
            // pageMapFooter
            // 
            pageMapFooter.BackColor = System.Drawing.SystemColors.Control;
            pageMapFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            pageMapFooter.Location = new System.Drawing.Point(3, 275);
            pageMapFooter.Name = "pageMapFooter";
            pageMapFooter.Size = new System.Drawing.Size(764, 159);
            pageMapFooter.TabIndex = 1;
            // 
            // tabHexView
            // 
            tabHexView.Controls.Add(hexView);
            tabHexView.Location = new System.Drawing.Point(4, 24);
            tabHexView.Name = "tabHexView";
            tabHexView.Size = new System.Drawing.Size(770, 437);
            tabHexView.TabIndex = 1;
            tabHexView.Text = "Hex";
            tabHexView.UseVisualStyleBackColor = true;
            // 
            // hexView
            // 
            hexView.AutoScroll = true;
            hexView.BackColor = System.Drawing.SystemColors.Control;
            hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            hexView.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            hexView.Location = new System.Drawing.Point(0, 0);
            hexView.Margin = new System.Windows.Forms.Padding(0);
            hexView.Name = "hexView";
            hexView.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            hexView.PageMap = null;
            hexView.Size = new System.Drawing.Size(770, 437);
            hexView.TabIndex = 0;
            // 
            // pageMapHeader
            // 
            pageMapHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pageMapHeader.Location = new System.Drawing.Point(0, 25);
            pageMapHeader.Name = "pageMapHeader";
            pageMapHeader.Size = new System.Drawing.Size(778, 91);
            pageMapHeader.TabIndex = 7;
            // 
            // tsPage
            // 
            tsPage.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsPage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbBackward, tsbForward, toolStripSeparator1, toolStripLabel1, txtPagenumber, toolStripSeparator2, tsbPrevious, tsbNext });
            tsPage.Location = new System.Drawing.Point(0, 0);
            tsPage.Name = "tsPage";
            tsPage.Size = new System.Drawing.Size(778, 25);
            tsPage.TabIndex = 8;
            tsPage.Text = "toolStrip1";
            // 
            // tsbBackward
            // 
            tsbBackward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbBackward.Image = Properties.Resources.NavBack;
            tsbBackward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbBackward.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbBackward.Name = "tsbBackward";
            tsbBackward.Size = new System.Drawing.Size(23, 22);
            tsbBackward.Text = "Navigate Backward";
            tsbBackward.Click += tsbBackward_Click;
            // 
            // tsbForward
            // 
            tsbForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbForward.Image = Properties.Resources.NavForward;
            tsbForward.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbForward.Name = "tsbForward";
            tsbForward.Size = new System.Drawing.Size(23, 22);
            tsbForward.Text = "Navigate Forward";
            tsbForward.Click += tsbForward_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(75, 22);
            toolStripLabel1.Text = "Pagenumber";
            // 
            // txtPagenumber
            // 
            txtPagenumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtPagenumber.Name = "txtPagenumber";
            txtPagenumber.ReadOnly = true;
            txtPagenumber.Size = new System.Drawing.Size(160, 25);
            txtPagenumber.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbPrevious
            // 
            tsbPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbPrevious.Image = Properties.Resources.GoRtlHS;
            tsbPrevious.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPrevious.Name = "tsbPrevious";
            tsbPrevious.Size = new System.Drawing.Size(23, 22);
            tsbPrevious.Text = "Previous Page";
            tsbPrevious.Click += tsbPrevious_Click;
            // 
            // tsbNext
            // 
            tsbNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbNext.Image = Properties.Resources.GoLtrHS;
            tsbNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbNext.Name = "tsbNext";
            tsbNext.Size = new System.Drawing.Size(23, 22);
            tsbNext.Text = "Next Page";
            tsbNext.Click += tsbNext_Click;
            // 
            // PageMapView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabViews);
            Controls.Add(pageMapHeader);
            Controls.Add(tsPage);
            DoubleBuffered = true;
            Name = "PageMapView";
            Size = new System.Drawing.Size(778, 581);
            tabViews.ResumeLayout(false);
            tabEntries.ResumeLayout(false);
            tabHexView.ResumeLayout(false);
            tsPage.ResumeLayout(false);
            tsPage.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TabControl tabViews;
        private System.Windows.Forms.TabPage tabEntries;
        private System.Windows.Forms.TabPage tabHexView;
        private PageMapHeader pageMapHeader;
        private EntryListView entryListView;
        private HexView hexView;
        private PageMapFooter pageMapFooter;
        private System.Windows.Forms.ToolStrip tsPage;
        private System.Windows.Forms.ToolStripButton tsbBackward;
        private System.Windows.Forms.ToolStripButton tsbForward;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox txtPagenumber;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbPrevious;
        private System.Windows.Forms.ToolStripButton tsbNext;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.Splitter splitter1;
    }
}
