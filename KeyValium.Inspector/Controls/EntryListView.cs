using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class EntryListView : UserControl
    {
        public EntryListView()
        {
            InitializeComponent();

            //colSubTree
        }

        internal void ShowEntries(PageMap map)
        {
            grid.BeginUpdate();

            grid.Rows.Clear();

            ConfigureColumns(map);

            if (map != null)
            {
                foreach (var item in map.Entries)
                {
                    var index = grid.Rows.Add();
                    grid.Rows[index].Cells[colFirstPage.Name].Value = item.FirstPage;
                    grid.Rows[index].Cells[colFlags.Name].Value = Display.FormatFlags(item.Flags);
                    grid.Rows[index].Cells[colIndex.Name].Value = item.Index;
                    grid.Rows[index].Cells[colValue.Name].Value = Display.FormatHex(item.InlineValue ?? item.OverflowValue, false, 256);
                    grid.Rows[index].Cells[colKey.Name].Value = Display.FormatHex(item.Key, true, 256);
                    grid.Rows[index].Cells[colKeyLength.Name].Value = item.KeyLength;
                    grid.Rows[index].Cells[colLastPage.Name].Value = item.LastPage;
                    grid.Rows[index].Cells[colLeftBranch.Name].Value = item.LeftBranch;
                    grid.Rows[index].Cells[colOffset.Name].Value = item.Offset;
                    grid.Rows[index].Cells[colOverflowPage.Name].Value = item.OverflowPage;
                    grid.Rows[index].Cells[colPageCount.Name].Value = item.PageCount;
                    grid.Rows[index].Cells[colRightBranch.Name].Value = item.RightBranch;
                    grid.Rows[index].Cells[colSubTree.Name].Value = item.SubTree;
                    grid.Rows[index].Cells[colTid.Name].Value = item.Tid;
                    grid.Rows[index].Cells[colValueLength.Name].Value = item.InlineValueLength ?? item.OverflowLength;
                    grid.Rows[index].Cells[colGlobalCount.Name].Value = item.GlobalCount;
                    grid.Rows[index].Cells[colLocalCount.Name].Value = item.LocalCount;

                    grid.Rows[index].Tag = item;
                }
            }

            UpdateSelection();

            grid.EndUpdate();
        }

        private void ConfigureColumns(PageMap map)
        {
            colIndex.Visible = true;
            colOffset.Visible = true;

            colFirstPage.Visible = false;
            colFlags.Visible = false;
            colValue.Visible = false;
            colKey.Visible = false;
            colKeyLength.Visible = false;
            colLastPage.Visible = false;
            colLeftBranch.Visible = false;

            colOverflowPage.Visible = false;
            colGlobalCount.Visible = false;
            colLocalCount.Visible = false;
            colPageCount.Visible = false;
            colRightBranch.Visible = false;
            colSubTree.Visible = false;
            colTid.Visible = false;
            colValueLength.Visible = false;

            if (map != null)
            {
                switch (map.PageType)
                {
                    case PageTypesI.Unknown:
                    case PageTypesI.FileHeader:
                    case PageTypesI.Meta:
                    case PageTypesI.FreeSpace:
                    case PageTypesI.FreeSpaceInUse:
                    case PageTypesI.DataOverflowCont:
                    case PageTypesI.DataOverflow:
                        break;
                    case PageTypesI.FsIndex:
                        colFlags.Visible = true;
                        colKeyLength.Visible = true;
                        colKey.Visible = true;
                        colFirstPage.Visible = true;
                        colLeftBranch.Visible = true;
                        colRightBranch.Visible = true;
                        break;
                    case PageTypesI.FsLeaf:
                        colFlags.Visible = true;
                        colKeyLength.Visible = true;
                        colKey.Visible = true;
                        colFirstPage.Visible = true;
                        colLastPage.Visible = true;
                        colPageCount.Visible = true;
                        colTid.Visible = true;
                        break;
                    case PageTypesI.DataIndex:
                        colFlags.Visible = true;
                        colKeyLength.Visible = true;
                        colKey.Visible = true;
                        colLeftBranch.Visible = true;
                        colRightBranch.Visible = true;
                        break;
                    case PageTypesI.DataLeaf:
                        colFlags.Visible = true;
                        colKeyLength.Visible = true;
                        colKey.Visible = true;
                        colValue.Visible = true;
                        colValueLength.Visible = true;
                        colSubTree.Visible = true;
                        colOverflowPage.Visible = true;
                        colGlobalCount.Visible = true;
                        colLocalCount.Visible = true;
                        break;
                }
            }
        }

        private void grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (grid.SelectedCells.Count > 0)
            {
                var cell = grid.SelectedCells[0];
                if (cell is DataGridViewLinkCell && cell.Value != null)
                {
                    var pageno = (KvPagenumber)cell.Value;
                    RaisePageNumberClicked(pageno);
                }
            }
        }

        internal event EventHandler<ShowPageEventArgs> PageNumberClicked;

        internal void RaisePageNumberClicked(KvPagenumber pageno)
        {
            PageNumberClicked?.Invoke(this, new ShowPageEventArgs(pageno));
        }

        internal event EventHandler<ShowBytesEventArgs> SelectedBytesChanged;

        internal void RaiseSelectedBytesChanged(byte[] bytes)
        {
            SelectedBytesChanged?.Invoke(this, new ShowBytesEventArgs(bytes));
        }

        private byte[] _selectedbytes;

        private byte[] SelectedBytes
        {
            get
            {
                return _selectedbytes;
            }
            set
            {
                if (_selectedbytes != value)
                {
                    _selectedbytes = value;
                    RaiseSelectedBytesChanged(_selectedbytes);
                }
            }
        }

        private byte[] GetValueBytes(DataGridViewCell cell)
        {
            var ei = cell.OwningRow?.Tag as EntryInfo;

            return ei?.InlineValue ?? ei?.OverflowValue;
        }

        private byte[] GetKeyBytes(DataGridViewCell cell)
        {
            var ei = cell.OwningRow?.Tag as EntryInfo;

            return ei?.Key;
        }

        private void grid_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            byte[] bytes = null;

            if (grid.SelectedCells.Count > 0)
            {
                var cell = grid.SelectedCells[0];
                if (cell.OwningColumn == colValue)
                {
                    bytes = GetValueBytes(cell);
                }
                else if (cell.OwningColumn == colKey)
                {
                    bytes = GetKeyBytes(cell);
                }
            }

            SelectedBytes = bytes;
        }
    }
}
