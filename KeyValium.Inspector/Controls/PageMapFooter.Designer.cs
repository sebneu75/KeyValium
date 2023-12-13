namespace KeyValium.Inspector.Controls
{
    partial class PageMapFooter
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
            mvBytes = new MultiView();
            SuspendLayout();
            // 
            // mvBytes
            // 
            mvBytes.Bytes = null;
            mvBytes.Dock = System.Windows.Forms.DockStyle.Fill;
            mvBytes.Location = new System.Drawing.Point(0, 0);
            mvBytes.Name = "mvBytes";
            mvBytes.Size = new System.Drawing.Size(818, 118);
            mvBytes.TabIndex = 0;
            mvBytes.Title = "";
            // 
            // PageMapFooter
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(mvBytes);
            Name = "PageMapFooter";
            Size = new System.Drawing.Size(818, 118);
            ResumeLayout(false);
        }

        #endregion

        private MultiView mvBytes;
    }
}
