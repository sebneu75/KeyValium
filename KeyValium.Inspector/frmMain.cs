using KeyValium.Exceptions;
using KeyValium.Inspector.Controls;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
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
    public partial class frmMain : Form, IMainView
    {
        public frmMain()
        {
            InitializeComponent();
        }

        #region IMainView

        public IPropertyView PropertyView
        {
            get
            {
                return propertyView;
            }
        }

        public IFileMapView FileMapView
        {
            get
            {
                return fileMapView;
            }
        }

        public IPageMapView PageMapView
        {
            get
            {
                return pageMapView;
            }
        }

        public IBenchmarkView BenchmarkView
        {
            get
            {
                return benchmarkView;
            }
        }

        public IFreeSpaceView FreeSpaceView
        {
            get
            {
                return freeSpaceView;
            }
        }

        public MainPresenter Presenter
        {
            get;
            set;
        }

        public void SelectMeta(MetaInfo meta)
        {
            if (meta.Index == 0)
            {
                tsbMeta0.Checked = true;
            }
            else if (meta.Index == 1)
            {
                tsbMeta1.Checked = true;
            }
        }

        public void SetMetas(IReadOnlyList<MetaInfo> metainfos, int? active)
        {
            if (metainfos == null)
            {
                tsbMeta0.Enabled = false;
                tsbMeta0.Checked = false;
                tsbMeta1.Enabled = false;
                tsbMeta1.Checked = false;
            }
            else

            {
                tsbMeta0.Enabled = true;
                tsbMeta1.Enabled = true;

                tsbMeta0.Tag = metainfos[0];
                tsbMeta1.Tag = metainfos[1];

                tsbMeta0.Text = metainfos[0].ToString();
                tsbMeta1.Text = metainfos[1].ToString();

                if (active.HasValue)
                {
                    if (active.Value == 0)
                    {
                        tsbMeta0.Checked = true;
                    }
                    else if (active.Value == 1)
                    {
                        tsbMeta1.Checked = true;
                    }
                }
            }
        }

        public void ActivatePageMap()
        {
            tabMain.SelectedTab = tabPageMap;
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                this.Text = "KeyValium.Inspector";
            }
            else
            {
                this.Text = "KeyValium.Inspector - " + title;
            }
        }

        public void UpdateProgress(ulong current, ulong total)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateProgress(current, total));
            }

            var val = (double)current / (double)total * 1000.0;

            try
            {
                this.pbLoading.Value = (int)val;
            }
            catch (Exception ex)
            {
                // TODO handling
            }
        }

        public void ShowLoadingPanel()
        {
            pbLoading.Value = 0;
            panelLoading.Visible = true;
        }

        public void HideLoadingPanel()
        {
            panelLoading.Visible = false;
        }

        #endregion

        private void tsbOpenDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                var result = openFileDialog.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    string password = null;
                    string keyfile = null;

                    while (true)
                    {
                        try
                        {
                            Presenter.CancelLoadFileMap();
                            Presenter.InspectDatabase(openFileDialog.FileName, password, keyfile);
                            break;
                        }
                        catch (KeyValiumException ex)
                        {
                            if (ex.ErrorCode == ErrorCodes.InvalidFileFormat)
                            {
                                var dlg = new frmPassword();
                                dlg.DbFile = openFileDialog.FileName;
                                result = dlg.ShowDialog(this);
                                if (result == DialogResult.OK)
                                {
                                    password = dlg.Password;
                                    keyfile = dlg.Keyfile;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Presenter.CloseDatabase();
            }
        }

        private void cmdCancelLoading_Click(object sender, EventArgs e)
        {
            Presenter.CancelLoadFileMap();
        }

        private void tsbMeta0_CheckedChanged(object sender, EventArgs e)
        {
            if (tsbMeta0.Checked)
            {
                tsbMeta0.BackColor = Color.LightBlue;
                tsbMeta1.Checked = false;
                Presenter.Model.ActiveMeta = tsbMeta0.Tag as MetaInfo;
            }
            else
            {
                tsbMeta0.BackColor = SystemColors.Control;
            }
        }

        private void tsbMeta1_CheckedChanged(object sender, EventArgs e)
        {
            if (tsbMeta1.Checked)
            {
                tsbMeta1.BackColor = Color.LightBlue;
                tsbMeta0.Checked = false;
                Presenter.Model.ActiveMeta = tsbMeta1.Tag as MetaInfo;
            }
            else
            {
                tsbMeta1.BackColor = SystemColors.Control;
            }
        }
    }
}
