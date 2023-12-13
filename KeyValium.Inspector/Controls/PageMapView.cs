using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Events;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class PageMapView : UserControl, IPageMapView
    {
        public PageMapView()
        {
            InitializeComponent();

            entryListView.PageNumberClicked += entryListView_PageNumberClicked;
            entryListView.SelectedBytesChanged += EntryListView_SelectedBytesChanged;

            UpdatePageMap();
        }

        private void EntryListView_SelectedBytesChanged(object sender, ShowBytesEventArgs e)
        {
            pageMapFooter.ShowBytes(e.Bytes);
        }

        private void entryListView_PageNumberClicked(object sender, ShowPageEventArgs e)
        {
            Presenter.ShowPage(e.PageNumber);
        }

        #region IPageMapView

        public PageMapPresenter Presenter
        {
            get;
            set;
        }

        public KvPagenumber? MaxPagenumber
        {
            get;
            set;
        }

        #endregion

        private PageMap _pagemap;

        private void tsbBackward_Click(object sender, EventArgs e)
        {
            Presenter.ShowPage(Presenter.History.MoveBackward());
        }

        private void tsbForward_Click(object sender, EventArgs e)
        {
            Presenter.ShowPage(Presenter.History.MoveForward());
        }

        private void tsbPrevious_Click(object sender, EventArgs e)
        {
            Presenter.ShowPage(_pagemap.PageNumber - 1);
        }

        private void tsbNext_Click(object sender, EventArgs e)
        {
            Presenter.ShowPage(_pagemap.PageNumber + 1);
        }

        [Browsable(false)]
        public PageMap PageMap
        {
            get
            {
                return _pagemap;
            }
            set
            {
                if (_pagemap != value)
                {
                    _pagemap = value;
                    UpdatePageMap();
                }
            }
        }

        private void UpdatePageMap()
        {
            if (_pagemap == null)
            {
                tsbBackward.Enabled = false;
                tsbForward.Enabled = false;
                tsbNext.Enabled = false;
                tsbPrevious.Enabled = false;
                txtPagenumber.Text = "";
            }
            else
            {
                tsbBackward.Enabled = Presenter.History.CanMoveBackward();
                tsbForward.Enabled = Presenter.History.CanMoveForward();
                tsbNext.Enabled = _pagemap.PageNumber < MaxPagenumber;
                tsbPrevious.Enabled = PageMap.PageNumber > 0;
                txtPagenumber.Text = Display.FormatNumber(_pagemap.PageNumber);
            }

            hexView.PageMap = _pagemap;
            pageMapHeader.ShowHeader(PageMap);
            pageMapFooter.ShowBytes(null);
            entryListView.ShowEntries(PageMap);
        }
    }
}
