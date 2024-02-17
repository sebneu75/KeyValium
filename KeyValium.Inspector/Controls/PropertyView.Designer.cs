namespace KeyValium.Inspector.Controls
{
    partial class PropertyView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            pgProps = new System.Windows.Forms.PropertyGrid();
            label2 = new System.Windows.Forms.Label();
            label17 = new System.Windows.Forms.Label();
            dgViewMeta = new DataGridViewEx();
            colMeta0 = new System.Windows.Forms.DataGridViewLinkColumn();
            colMeta1 = new System.Windows.Forms.DataGridViewLinkColumn();
            dgViewPageCounts = new DataGridViewEx();
            colPageCountsMeta0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colSizeMeta0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPageCountsMeta0P = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPageCountsMeta1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colSizeMeta1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPageCountsMeta1P = new System.Windows.Forms.DataGridViewTextBoxColumn();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            dgViewUnusedPages = new DataGridViewEx();
            colUnusedSpaceMeta0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colUnusedSpaceMeta0P = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colUnusedSpaceMeta1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colUnusedSpaceMeta1P = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgViewMeta).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgViewPageCounts).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgViewUnusedPages).BeginInit();
            SuspendLayout();
            // 
            // pgProps
            // 
            pgProps.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pgProps.Location = new System.Drawing.Point(3, 23);
            pgProps.Name = "pgProps";
            tableLayoutPanel1.SetRowSpan(pgProps, 7);
            pgProps.Size = new System.Drawing.Size(440, 580);
            pgProps.TabIndex = 41;
            pgProps.ToolbarVisible = false;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label2.Location = new System.Drawing.Point(449, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(441, 20);
            label2.TabIndex = 31;
            label2.Text = "Meta Information";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            label17.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label17.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label17.Location = new System.Drawing.Point(449, 208);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(441, 20);
            label17.TabIndex = 37;
            label17.Text = "Page Counts (Data and Freespace)";
            label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgViewMeta
            // 
            dgViewMeta.AllowUserToAddRows = false;
            dgViewMeta.AllowUserToDeleteRows = false;
            dgViewMeta.AllowUserToOrderColumns = true;
            dgViewMeta.AllowUserToResizeColumns = false;
            dgViewMeta.AllowUserToResizeRows = false;
            dgViewMeta.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgViewMeta.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgViewMeta.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgViewMeta.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgViewMeta.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgViewMeta.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colMeta0, colMeta1 });
            dgViewMeta.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgViewMeta.EnableHeadersVisualStyles = false;
            dgViewMeta.Location = new System.Drawing.Point(449, 23);
            dgViewMeta.MultiSelect = false;
            dgViewMeta.Name = "dgViewMeta";
            dgViewMeta.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgViewMeta.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgViewMeta.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgViewMeta.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgViewMeta.ShowCellToolTips = false;
            dgViewMeta.ShowEditingIcon = false;
            dgViewMeta.Size = new System.Drawing.Size(441, 162);
            dgViewMeta.TabIndex = 38;
            dgViewMeta.CellContentClick += dgViewMeta_CellContentClick;
            // 
            // colMeta0
            // 
            colMeta0.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N0";
            colMeta0.DefaultCellStyle = dataGridViewCellStyle2;
            colMeta0.HeaderText = "Meta 0";
            colMeta0.Name = "colMeta0";
            colMeta0.ReadOnly = true;
            colMeta0.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // colMeta1
            // 
            colMeta1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N0";
            colMeta1.DefaultCellStyle = dataGridViewCellStyle3;
            colMeta1.HeaderText = "Meta 1";
            colMeta1.Name = "colMeta1";
            colMeta1.ReadOnly = true;
            colMeta1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dgViewPageCounts
            // 
            dgViewPageCounts.AllowUserToAddRows = false;
            dgViewPageCounts.AllowUserToDeleteRows = false;
            dgViewPageCounts.AllowUserToOrderColumns = true;
            dgViewPageCounts.AllowUserToResizeColumns = false;
            dgViewPageCounts.AllowUserToResizeRows = false;
            dgViewPageCounts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgViewPageCounts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgViewPageCounts.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgViewPageCounts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dgViewPageCounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgViewPageCounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colPageCountsMeta0, colSizeMeta0, colPageCountsMeta0P, colPageCountsMeta1, colSizeMeta1, colPageCountsMeta1P });
            dgViewPageCounts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgViewPageCounts.EnableHeadersVisualStyles = false;
            dgViewPageCounts.Location = new System.Drawing.Point(449, 231);
            dgViewPageCounts.MultiSelect = false;
            dgViewPageCounts.Name = "dgViewPageCounts";
            dgViewPageCounts.ReadOnly = true;
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgViewPageCounts.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            dgViewPageCounts.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgViewPageCounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgViewPageCounts.ShowCellToolTips = false;
            dgViewPageCounts.ShowEditingIcon = false;
            dgViewPageCounts.Size = new System.Drawing.Size(441, 162);
            dgViewPageCounts.TabIndex = 39;
            // 
            // colPageCountsMeta0
            // 
            colPageCountsMeta0.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Format = "N0";
            colPageCountsMeta0.DefaultCellStyle = dataGridViewCellStyle6;
            colPageCountsMeta0.HeaderText = "Meta 0";
            colPageCountsMeta0.Name = "colPageCountsMeta0";
            colPageCountsMeta0.ReadOnly = true;
            colPageCountsMeta0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colSizeMeta0
            // 
            colSizeMeta0.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle7.Format = "N2";
            colSizeMeta0.DefaultCellStyle = dataGridViewCellStyle7;
            colSizeMeta0.HeaderText = "Size (MB)";
            colSizeMeta0.Name = "colSizeMeta0";
            colSizeMeta0.ReadOnly = true;
            colSizeMeta0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colSizeMeta0.Width = 62;
            // 
            // colPageCountsMeta0P
            // 
            colPageCountsMeta0P.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle8.Format = "N2";
            colPageCountsMeta0P.DefaultCellStyle = dataGridViewCellStyle8;
            colPageCountsMeta0P.HeaderText = "%";
            colPageCountsMeta0P.Name = "colPageCountsMeta0P";
            colPageCountsMeta0P.ReadOnly = true;
            colPageCountsMeta0P.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colPageCountsMeta0P.Width = 23;
            // 
            // colPageCountsMeta1
            // 
            colPageCountsMeta1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle9.Format = "N0";
            colPageCountsMeta1.DefaultCellStyle = dataGridViewCellStyle9;
            colPageCountsMeta1.HeaderText = "Meta 1";
            colPageCountsMeta1.Name = "colPageCountsMeta1";
            colPageCountsMeta1.ReadOnly = true;
            colPageCountsMeta1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colSizeMeta1
            // 
            colSizeMeta1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle10.Format = "N2";
            colSizeMeta1.DefaultCellStyle = dataGridViewCellStyle10;
            colSizeMeta1.HeaderText = "Size (MB)";
            colSizeMeta1.Name = "colSizeMeta1";
            colSizeMeta1.ReadOnly = true;
            colSizeMeta1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colSizeMeta1.Width = 62;
            // 
            // colPageCountsMeta1P
            // 
            colPageCountsMeta1P.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle11.Format = "N2";
            colPageCountsMeta1P.DefaultCellStyle = dataGridViewCellStyle11;
            colPageCountsMeta1P.HeaderText = "%";
            colPageCountsMeta1P.Name = "colPageCountsMeta1P";
            colPageCountsMeta1P.ReadOnly = true;
            colPageCountsMeta1P.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colPageCountsMeta1P.Width = 23;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 1, 0);
            tableLayoutPanel1.Controls.Add(pgProps, 0, 1);
            tableLayoutPanel1.Controls.Add(dgViewMeta, 1, 1);
            tableLayoutPanel1.Controls.Add(label17, 1, 3);
            tableLayoutPanel1.Controls.Add(dgViewPageCounts, 1, 4);
            tableLayoutPanel1.Controls.Add(label3, 1, 6);
            tableLayoutPanel1.Controls.Add(dgViewUnusedPages, 1, 7);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.Size = new System.Drawing.Size(893, 606);
            tableLayoutPanel1.TabIndex = 32;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(440, 20);
            label1.TabIndex = 42;
            label1.Text = "Database Properties";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label3.Location = new System.Drawing.Point(449, 416);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(441, 20);
            label3.TabIndex = 43;
            label3.Text = "Unused Space in Pages";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgViewUnusedPages
            // 
            dgViewUnusedPages.AllowUserToAddRows = false;
            dgViewUnusedPages.AllowUserToDeleteRows = false;
            dgViewUnusedPages.AllowUserToOrderColumns = true;
            dgViewUnusedPages.AllowUserToResizeColumns = false;
            dgViewUnusedPages.AllowUserToResizeRows = false;
            dgViewUnusedPages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgViewUnusedPages.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgViewUnusedPages.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgViewUnusedPages.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
            dgViewUnusedPages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgViewUnusedPages.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colUnusedSpaceMeta0, colUnusedSpaceMeta0P, colUnusedSpaceMeta1, colUnusedSpaceMeta1P });
            dgViewUnusedPages.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgViewUnusedPages.EnableHeadersVisualStyles = false;
            dgViewUnusedPages.Location = new System.Drawing.Point(449, 439);
            dgViewUnusedPages.MultiSelect = false;
            dgViewUnusedPages.Name = "dgViewUnusedPages";
            dgViewUnusedPages.ReadOnly = true;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgViewUnusedPages.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
            dgViewUnusedPages.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgViewUnusedPages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgViewUnusedPages.ShowCellToolTips = false;
            dgViewUnusedPages.ShowEditingIcon = false;
            dgViewUnusedPages.Size = new System.Drawing.Size(441, 164);
            dgViewUnusedPages.TabIndex = 44;
            // 
            // colUnusedSpaceMeta0
            // 
            colUnusedSpaceMeta0.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle14.Format = "N2";
            colUnusedSpaceMeta0.DefaultCellStyle = dataGridViewCellStyle14;
            colUnusedSpaceMeta0.HeaderText = "Meta 0 Size (MB)";
            colUnusedSpaceMeta0.Name = "colUnusedSpaceMeta0";
            colUnusedSpaceMeta0.ReadOnly = true;
            colUnusedSpaceMeta0.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colUnusedSpaceMeta0P
            // 
            colUnusedSpaceMeta0P.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle15.Format = "N2";
            colUnusedSpaceMeta0P.DefaultCellStyle = dataGridViewCellStyle15;
            colUnusedSpaceMeta0P.HeaderText = "%";
            colUnusedSpaceMeta0P.Name = "colUnusedSpaceMeta0P";
            colUnusedSpaceMeta0P.ReadOnly = true;
            colUnusedSpaceMeta0P.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colUnusedSpaceMeta0P.Width = 23;
            // 
            // colUnusedSpaceMeta1
            // 
            colUnusedSpaceMeta1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle16.Format = "N2";
            colUnusedSpaceMeta1.DefaultCellStyle = dataGridViewCellStyle16;
            colUnusedSpaceMeta1.HeaderText = "Meta 1 Size (MB)";
            colUnusedSpaceMeta1.Name = "colUnusedSpaceMeta1";
            colUnusedSpaceMeta1.ReadOnly = true;
            colUnusedSpaceMeta1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colUnusedSpaceMeta1P
            // 
            colUnusedSpaceMeta1P.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle17.Format = "N2";
            colUnusedSpaceMeta1P.DefaultCellStyle = dataGridViewCellStyle17;
            colUnusedSpaceMeta1P.HeaderText = "%";
            colUnusedSpaceMeta1P.Name = "colUnusedSpaceMeta1P";
            colUnusedSpaceMeta1P.ReadOnly = true;
            colUnusedSpaceMeta1P.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colUnusedSpaceMeta1P.Width = 23;
            // 
            // PropertyView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            Name = "PropertyView";
            Size = new System.Drawing.Size(893, 606);
            ((System.ComponentModel.ISupportInitialize)dgViewMeta).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgViewPageCounts).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgViewUnusedPages).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgProps;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label17;
        private DataGridViewEx dgViewMeta;
        private System.Windows.Forms.DataGridViewLinkColumn colMeta0;
        private System.Windows.Forms.DataGridViewLinkColumn colMeta1;
        private DataGridViewEx dgViewPageCounts;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCountsMeta0;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSizeMeta0;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCountsMeta0P;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCountsMeta1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSizeMeta1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageCountsMeta1P;
        private System.Windows.Forms.Label label3;
        private DataGridViewEx dgViewUnusedPages;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnusedSpaceMeta0;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnusedSpaceMeta0P;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnusedSpaceMeta1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnusedSpaceMeta1P;
    }
}
