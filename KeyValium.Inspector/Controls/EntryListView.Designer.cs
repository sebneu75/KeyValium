namespace KeyValium.Inspector.Controls
{
    partial class EntryListView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            grid = new DataGridViewEx();
            colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colFlags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colKeyLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colValueLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colOverflowPage = new System.Windows.Forms.DataGridViewLinkColumn();
            colSubTree = new System.Windows.Forms.DataGridViewLinkColumn();
            colGlobalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLocalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLeftBranch = new System.Windows.Forms.DataGridViewLinkColumn();
            colRightBranch = new System.Windows.Forms.DataGridViewLinkColumn();
            colFirstPage = new System.Windows.Forms.DataGridViewLinkColumn();
            colLastPage = new System.Windows.Forms.DataGridViewLinkColumn();
            colTid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPageCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
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
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colIndex, colOffset, colFlags, colKeyLength, colKey, colValueLength, colValue, colOverflowPage, colSubTree, colGlobalCount, colLocalCount, colLeftBranch, colRightBranch, colFirstPage, colLastPage, colTid, colPageCount });
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            grid.EnableHeadersVisualStyles = false;
            grid.Location = new System.Drawing.Point(0, 0);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            grid.Size = new System.Drawing.Size(671, 470);
            grid.TabIndex = 0;
            grid.CellContentClick += grid_CellContentClick;
            grid.SelectionChanged += grid_SelectionChanged;
            // 
            // colIndex
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N0";
            dataGridViewCellStyle2.NullValue = null;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colIndex.DefaultCellStyle = dataGridViewCellStyle2;
            colIndex.Frozen = true;
            colIndex.HeaderText = "Index";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            colIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colIndex.Width = 42;
            // 
            // colOffset
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N0";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colOffset.DefaultCellStyle = dataGridViewCellStyle3;
            colOffset.Frozen = true;
            colOffset.HeaderText = "Offset";
            colOffset.Name = "colOffset";
            colOffset.ReadOnly = true;
            colOffset.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            colOffset.Width = 64;
            // 
            // colFlags
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.NullValue = null;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colFlags.DefaultCellStyle = dataGridViewCellStyle4;
            colFlags.HeaderText = "Flags";
            colFlags.Name = "colFlags";
            colFlags.ReadOnly = true;
            colFlags.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            colFlags.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colFlags.Width = 40;
            // 
            // colKeyLength
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Format = "N0";
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colKeyLength.DefaultCellStyle = dataGridViewCellStyle5;
            colKeyLength.HeaderText = "KeyLength";
            colKeyLength.Name = "colKeyLength";
            colKeyLength.ReadOnly = true;
            colKeyLength.Width = 88;
            // 
            // colKey
            // 
            colKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            colKey.DefaultCellStyle = dataGridViewCellStyle6;
            colKey.HeaderText = "Key";
            colKey.Name = "colKey";
            colKey.ReadOnly = true;
            // 
            // colValueLength
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle7.Format = "N0";
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colValueLength.DefaultCellStyle = dataGridViewCellStyle7;
            colValueLength.HeaderText = "ValueLength";
            colValueLength.Name = "colValueLength";
            colValueLength.ReadOnly = true;
            colValueLength.Width = 97;
            // 
            // colValue
            // 
            colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            colValue.DefaultCellStyle = dataGridViewCellStyle8;
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            colValue.ReadOnly = true;
            // 
            // colOverflowPage
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle9.Format = "N0";
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colOverflowPage.DefaultCellStyle = dataGridViewCellStyle9;
            colOverflowPage.HeaderText = "OverflowPage";
            colOverflowPage.Name = "colOverflowPage";
            colOverflowPage.ReadOnly = true;
            colOverflowPage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colOverflowPage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colOverflowPage.Width = 106;
            // 
            // colSubTree
            // 
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle10.Format = "N0";
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colSubTree.DefaultCellStyle = dataGridViewCellStyle10;
            colSubTree.HeaderText = "SubTree";
            colSubTree.Name = "colSubTree";
            colSubTree.ReadOnly = true;
            colSubTree.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colSubTree.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colSubTree.Width = 73;
            // 
            // colGlobalCount
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle11.Format = "N0";
            colGlobalCount.DefaultCellStyle = dataGridViewCellStyle11;
            colGlobalCount.HeaderText = "GlobalCount";
            colGlobalCount.Name = "colGlobalCount";
            colGlobalCount.ReadOnly = true;
            colGlobalCount.Width = 99;
            // 
            // colLocalCount
            // 
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle12.Format = "N0";
            colLocalCount.DefaultCellStyle = dataGridViewCellStyle12;
            colLocalCount.HeaderText = "LocalCount";
            colLocalCount.Name = "colLocalCount";
            colLocalCount.ReadOnly = true;
            colLocalCount.Width = 93;
            // 
            // colLeftBranch
            // 
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle13.Format = "N0";
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colLeftBranch.DefaultCellStyle = dataGridViewCellStyle13;
            colLeftBranch.HeaderText = "LeftBranch";
            colLeftBranch.Name = "colLeftBranch";
            colLeftBranch.ReadOnly = true;
            colLeftBranch.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colLeftBranch.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colLeftBranch.Width = 89;
            // 
            // colRightBranch
            // 
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle14.Format = "N0";
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colRightBranch.DefaultCellStyle = dataGridViewCellStyle14;
            colRightBranch.HeaderText = "RightBranch";
            colRightBranch.Name = "colRightBranch";
            colRightBranch.ReadOnly = true;
            colRightBranch.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colRightBranch.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colRightBranch.Width = 97;
            // 
            // colFirstPage
            // 
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle15.Format = "N0";
            dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colFirstPage.DefaultCellStyle = dataGridViewCellStyle15;
            colFirstPage.HeaderText = "FirstPage";
            colFirstPage.Name = "colFirstPage";
            colFirstPage.ReadOnly = true;
            colFirstPage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colFirstPage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colFirstPage.Width = 80;
            // 
            // colLastPage
            // 
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle16.Format = "N0";
            dataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colLastPage.DefaultCellStyle = dataGridViewCellStyle16;
            colLastPage.HeaderText = "LastPage";
            colLastPage.Name = "colLastPage";
            colLastPage.ReadOnly = true;
            colLastPage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colLastPage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colLastPage.Width = 79;
            // 
            // colTid
            // 
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle17.Format = "N0";
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colTid.DefaultCellStyle = dataGridViewCellStyle17;
            colTid.HeaderText = "Tid";
            colTid.Name = "colTid";
            colTid.ReadOnly = true;
            colTid.Width = 48;
            // 
            // colPageCount
            // 
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle18.Format = "N0";
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            colPageCount.DefaultCellStyle = dataGridViewCellStyle18;
            colPageCount.HeaderText = "PageCount";
            colPageCount.Name = "colPageCount";
            colPageCount.ReadOnly = true;
            colPageCount.Width = 91;
            // 
            // EntryListView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(grid);
            Name = "EntryListView";
            Size = new System.Drawing.Size(671, 470);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridViewEx grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFlags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKeyLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValueLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewLinkColumn colOverflowPage;
        private System.Windows.Forms.DataGridViewLinkColumn colSubTree;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlobalCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalCount;
        private System.Windows.Forms.DataGridViewLinkColumn colLeftBranch;
        private System.Windows.Forms.DataGridViewLinkColumn colRightBranch;
        private System.Windows.Forms.DataGridViewLinkColumn colFirstPage;
        private System.Windows.Forms.DataGridViewLinkColumn colLastPage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCount;
    }
}
