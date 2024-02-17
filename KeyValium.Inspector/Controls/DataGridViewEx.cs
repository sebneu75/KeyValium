using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    internal class DataGridViewEx : DataGridView
    {
        public DataGridViewEx()
        {
            var colheaderstyle = new DataGridViewCellStyle();

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToOrderColumns = false;
            AllowUserToResizeColumns = false;
            AllowUserToResizeRows = false;
            AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Single;
            colheaderstyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            colheaderstyle.BackColor = System.Drawing.SystemColors.ControlLight;
            colheaderstyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            colheaderstyle.ForeColor = System.Drawing.SystemColors.WindowText;
            colheaderstyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            colheaderstyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            colheaderstyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            ColumnHeadersDefaultCellStyle = colheaderstyle;
            EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            EnableHeadersVisualStyles = false;
            MultiSelect = false;
            ReadOnly = true;
            RowTemplate.Height = 25;
            SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;

            DoubleBuffered = true;
        }

        DataGridViewAutoSizeColumnsMode _oldcolumnmode = DataGridViewAutoSizeColumnsMode.AllCells;
        DataGridViewAutoSizeRowsMode _oldrowmode = DataGridViewAutoSizeRowsMode.AllCells;

        public void BeginUpdate()
        {
            SuspendLayout();

            _oldcolumnmode = AutoSizeColumnsMode;
            _oldrowmode = AutoSizeRowsMode;

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        }

        public void EndUpdate()
        {
            AutoSizeColumnsMode = _oldcolumnmode;
            AutoSizeRowsMode = _oldrowmode;

            ResumeLayout(true);
        }
    }
}
