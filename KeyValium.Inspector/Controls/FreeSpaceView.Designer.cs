namespace KeyValium.Inspector.Controls
{
    partial class FreeSpaceView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            tslEntryCount = new System.Windows.Forms.ToolStripLabel();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            tslPageCount = new System.Windows.Forms.ToolStripLabel();
            grid = new DataGridViewEx();
            colFirstPage = new System.Windows.Forms.DataGridViewLinkColumn();
            colLastPage = new System.Windows.Forms.DataGridViewLinkColumn();
            colTid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPageCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLocation = new System.Windows.Forms.DataGridViewLinkColumn();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabel1, tslEntryCount, toolStripSeparator3, toolStripLabel2, tslPageCount });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(760, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(73, 22);
            toolStripLabel1.Text = "Entry Count:";
            // 
            // tslEntryCount
            // 
            tslEntryCount.Name = "tslEntryCount";
            tslEntryCount.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(66, 22);
            toolStripLabel2.Text = "Free Pages:";
            // 
            // tslPageCount
            // 
            tslPageCount.Name = "tslPageCount";
            tslPageCount.Size = new System.Drawing.Size(0, 22);
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToOrderColumns = true;
            grid.AllowUserToResizeColumns = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            grid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colFirstPage, colLastPage, colTid, colPageCount, colLocation });
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            grid.EnableHeadersVisualStyles = false;
            grid.Location = new System.Drawing.Point(0, 25);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 25;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            grid.Size = new System.Drawing.Size(760, 402);
            grid.TabIndex = 1;
            grid.CellContentClick += grid_CellContentClick;
            // 
            // colFirstPage
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N0";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colFirstPage.DefaultCellStyle = dataGridViewCellStyle2;
            colFirstPage.HeaderText = "FirstPage";
            colFirstPage.Name = "colFirstPage";
            colFirstPage.ReadOnly = true;
            colFirstPage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colFirstPage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colFirstPage.Width = 80;
            // 
            // colLastPage
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N0";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colLastPage.DefaultCellStyle = dataGridViewCellStyle3;
            colLastPage.HeaderText = "LastPage";
            colLastPage.Name = "colLastPage";
            colLastPage.ReadOnly = true;
            colLastPage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colLastPage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colLastPage.Width = 79;
            // 
            // colTid
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N0";
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colTid.DefaultCellStyle = dataGridViewCellStyle4;
            colTid.HeaderText = "Tid";
            colTid.Name = "colTid";
            colTid.ReadOnly = true;
            colTid.Width = 48;
            // 
            // colPageCount
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N0";
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colPageCount.DefaultCellStyle = dataGridViewCellStyle5;
            colPageCount.HeaderText = "PageCount";
            colPageCount.Name = "colPageCount";
            colPageCount.ReadOnly = true;
            colPageCount.Width = 91;
            // 
            // colLocation
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Format = "x4";
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colLocation.DefaultCellStyle = dataGridViewCellStyle6;
            colLocation.HeaderText = "Location";
            colLocation.Name = "colLocation";
            colLocation.ReadOnly = true;
            colLocation.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            colLocation.Width = 59;
            // 
            // FreeSpaceView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(grid);
            Controls.Add(toolStrip1);
            Name = "FreeSpaceView";
            Size = new System.Drawing.Size(760, 427);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel tslEntryCount;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel tslPageCount;
        private DataGridViewEx grid;
        private System.Windows.Forms.DataGridViewLinkColumn colFirstPage;
        private System.Windows.Forms.DataGridViewLinkColumn colLastPage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCount;
        private System.Windows.Forms.DataGridViewLinkColumn colLocation;
    }
}
