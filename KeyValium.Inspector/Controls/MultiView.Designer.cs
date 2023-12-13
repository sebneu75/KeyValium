namespace KeyValium.Inspector.Controls
{
    partial class MultiView
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
            cmbDisplayAs = new System.Windows.Forms.ComboBox();
            txtBytes = new System.Windows.Forms.TextBox();
            lblTitle = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // cmbDisplayAs
            // 
            cmbDisplayAs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cmbDisplayAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDisplayAs.FormattingEnabled = true;
            cmbDisplayAs.Items.AddRange(new object[] { "Hexdump", "UTF-8", "ANSI", "ASCII", "UTF-16 (LE)", "UTF-16 (BE)", "Integer (LE)", "Integer (BE)" });
            cmbDisplayAs.Location = new System.Drawing.Point(419, 5);
            cmbDisplayAs.Name = "cmbDisplayAs";
            cmbDisplayAs.Size = new System.Drawing.Size(132, 23);
            cmbDisplayAs.TabIndex = 7;
            cmbDisplayAs.SelectedIndexChanged += cmbDisplayAs_SelectedIndexChanged;
            // 
            // txtBytes
            // 
            txtBytes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtBytes.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtBytes.Location = new System.Drawing.Point(73, 3);
            txtBytes.MaxLength = 262144;
            txtBytes.Multiline = true;
            txtBytes.Name = "txtBytes";
            txtBytes.ReadOnly = true;
            txtBytes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtBytes.Size = new System.Drawing.Size(340, 82);
            txtBytes.TabIndex = 6;
            // 
            // lblTitle
            // 
            lblTitle.Location = new System.Drawing.Point(3, 4);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(64, 23);
            lblTitle.TabIndex = 5;
            lblTitle.Text = "Title";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MultiView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(cmbDisplayAs);
            Controls.Add(txtBytes);
            Controls.Add(lblTitle);
            Name = "MultiView";
            Size = new System.Drawing.Size(554, 88);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbDisplayAs;
        private System.Windows.Forms.TextBox txtBytes;
        private System.Windows.Forms.Label lblTitle;
    }
}
