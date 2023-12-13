namespace KeyValium.Inspector.Controls
{
    partial class BlockView
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
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 250;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.UseAnimation = false;
            this.toolTip.UseFading = false;
            // 
            // timer
            // 
            this.timer.Interval = 500;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // BlockView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.DoubleBuffered = true;
            this.Name = "BlockView";
            this.Size = new System.Drawing.Size(710, 539);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FileMapView_Paint);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.BlockView_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BlockView_MouseDown);
            this.MouseEnter += new System.EventHandler(this.BlockView_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.BlockView_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BlockView_MouseMove);
            this.Resize += new System.EventHandler(this.FileMapView_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer timer;
    }
}
