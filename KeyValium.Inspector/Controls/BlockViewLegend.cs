using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class BlockViewLegend : UserControl
    {
        public BlockViewLegend()
        {
            InitializeComponent();

            SetColors();
        }

        private void SetColors()
        {
            lblHeaderColor.BackColor = BlockViewColors.Header;
            lblMetaColor.BackColor = BlockViewColors.Meta;
            lblDataIndexColor.BackColor = BlockViewColors.DataIndex;
            lblDataLeafColor.BackColor = BlockViewColors.DataLeaf;
            lblDataOverflowColor.BackColor = BlockViewColors.DataOverflow;
            lblDataOverflowContColor.BackColor = BlockViewColors.DataOverFlowCont;
            lblFsIndexColor.BackColor = BlockViewColors.FsIndex;
            lblFsLeafColor.BackColor = BlockViewColors.Fsleaf;
            lblFsColor.BackColor = BlockViewColors.Fs;
            lblFsInUseColor.BackColor = BlockViewColors.FsInUse;
            lblUnknownColor.BackColor = BlockViewColors.Unknown;
        }
    }
}
