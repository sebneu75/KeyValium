using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class PageMapHeader : UserControl
    {
        public PageMapHeader()
        {
            InitializeComponent();
        }

        internal void ShowHeader(PageMap pm)
        {
            if (pm == null)
            {
                ClearHeader();
            }
            else
            {
                txtPageType.Text = Display.Format(pm.PageType);
                txtPageSize.Text = Display.FormatNumber(pm.PageSize);

                txtMagic.Text = Display.FormatHex(pm.Magic);
                txtTid.Text = Display.FormatNumber(pm.Tid);

                txtKeyCount.Text = Display.FormatNumber(pm.KeyCount);
                txtLow.Text = Display.FormatNumber(pm.Low);
                txtHigh.Text = Display.FormatNumber(pm.High);

                txtContentSize.Text = Display.FormatNumber(pm.ContentSize);
                txtFreeSpace.Text = Display.FormatNumber(pm.FreeSpace);
                txtUsedSpace.Text = Display.FormatNumber(pm.UsedSpace);
            }
        }

        private void ClearHeader()
        {
            txtContentSize.Text = "";
            txtFreeSpace.Text = "";
            txtHigh.Text = "";
            txtKeyCount.Text = "";
            txtLow.Text = "";
            txtMagic.Text = "";
            txtPageSize.Text = "";
            txtPageType.Text = "";
            txtTid.Text = "";
            txtUsedSpace.Text = "";
        }
    }
}
