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
    public partial class PageMapFooter : UserControl
    {
        public PageMapFooter()
        {
            InitializeComponent();
        }

        internal void ShowBytes(byte[] bytes)
        {
            mvBytes.Bytes = bytes;
        }
    }
}
