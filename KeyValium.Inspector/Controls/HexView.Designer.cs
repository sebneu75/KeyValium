namespace KeyValium.Inspector.Controls
{
    partial class HexView
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
            components = new System.ComponentModel.Container();
            toolTip = new System.Windows.Forms.ToolTip(components);
            timer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // timer
            // 
            timer.Interval = 500;
            timer.Tick += timer_Tick;
            // 
            // HexView
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            AutoScroll = true;
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Margin = new System.Windows.Forms.Padding(12, 8, 12, 8);
            Name = "HexView";
            Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            Size = new System.Drawing.Size(505, 300);
            FontChanged += HexView_FontChanged;
            Paint += HexView_Paint;
            MouseClick += HexView_MouseClick;
            MouseDoubleClick += HexView_MouseDoubleClick;
            MouseEnter += HexView_MouseEnter;
            MouseLeave += HexView_MouseLeave;
            MouseMove += HexView_MouseMove;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer timer;
    }
}
