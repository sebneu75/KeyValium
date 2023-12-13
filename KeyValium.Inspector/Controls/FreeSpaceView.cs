using KeyValium.Inspector;
using KeyValium.Inspector.Controls;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class FreeSpaceView : UserControl, IFreeSpaceView
    {
        public FreeSpaceView()
        {
            InitializeComponent();
        }


        #region IFreeSpaceView

        public FreeSpacePresenter Presenter
        {
            get;
            set;
        }

        public void Clear()
        {
            grid.BeginUpdate();
            grid.Rows.Clear();
            grid.EndUpdate();
        }

        public void SetFreeSpaceList(IReadOnlyList<FsEntryWrapper> list)
        {
            tslEntryCount.Text = list == null ? "0" : list.Count.ToString();
            tslPageCount.Text = list == null ? "0" : list.Sum(x => (decimal)x.PageCount).ToString();

            grid.BeginUpdate();
            grid.Rows.Clear();

            if (list != null)
            {
                foreach (var item in list.OrderBy(x => x.FirstPage))
                {
                    var index = grid.Rows.Add();
                    grid.Rows[index].Cells[colFirstPage.Index].Value = item.FirstPage;
                    grid.Rows[index].Cells[colLastPage.Index].Value = item.LastPage;
                    grid.Rows[index].Cells[colLocation.Index].Value = item.Location;
                    grid.Rows[index].Cells[colPageCount.Index].Value = item.PageCount;
                    grid.Rows[index].Cells[colTid.Index].Value = item.Tid;
                }
            }

            grid.EndUpdate();
        }

        #endregion

        private void grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (grid.SelectedCells.Count > 0)
            {
                var cell = grid.SelectedCells[0];
                if (cell is DataGridViewLinkCell && cell.Value != null)
                {
                    var dl = cell.Value as DataLocation;
                    if (dl != null)
                    {
                        Presenter.ShowPage(dl.Pagenumber);
                    }
                    else
                    {
                        Presenter.ShowPage((KvPagenumber)cell.Value);
                    }
                }
            }
        }
    }
}
