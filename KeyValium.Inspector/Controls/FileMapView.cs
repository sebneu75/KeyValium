using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class FileMapView : UserControl, IFileMapView
    {
        public FileMapView()
        {
            InitializeComponent();
        }

        #region IFileMapView

        public FileMapPresenter Presenter
        {
            get;
            set;
        }

        public void SetMetaInfo(MetaInfo mi)
        {
            blockView.SelectedMetaIndex = mi == null ? (short)0 : (short)mi.Index;
        }

        public void SetFileMap(FileMap map)
        {
            blockView.FileMap = map;
        }

        public void ClearFileMap()
        {
            blockView.SelectedMetaIndex = 0;
            blockView.FileMap = null;
        }

        #endregion

        private void blockView_ShowPage(object sender, ShowPageEventArgs e)
        {
            Presenter.ShowPage(e.PageNumber);
        }
    }
}
