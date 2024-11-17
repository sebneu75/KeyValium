using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyValium.Inspector
{
    public partial class frmPassword : Form
    {
        public frmPassword()
        {
            InitializeComponent();

            DbFile = null;
        }

        private string _dbfile;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DbFile
        {
            get
            {
                return _dbfile;
            }
            set
            {
                _dbfile = value;
                this.Text = string.Format("Enter Password for {0}", _dbfile ?? "");
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Password
        {
            get
            {
                return txtPassword.Text;
            }
            set
            {
                txtPassword.Text = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Keyfile
        {
            get
            {
                var ret = txtKeyfile.Text;
                return string.IsNullOrWhiteSpace(ret) ? null : ret;
            }
            set
            {
                txtKeyfile.Text = value;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
