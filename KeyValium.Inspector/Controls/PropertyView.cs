using KeyValium.Inspector.MVP.Events;
using KeyValium.Inspector.MVP.Presenters;
using KeyValium.Inspector.MVP.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class PropertyView : UserControl, IPropertyView
    {
        public PropertyView()
        {
            InitializeComponent();
        }

        #region IPropertyView

        public PropertyPresenter Presenter
        {
            get;
            set;
        }

        public void ShowDatabaseProperties(DatabaseProperties props)
        {
            dgViewMeta.Rows.Clear();

            pgProps.SelectedObject = props;

            if (props != null)
            {
                var maxtid = props.MetaInfos.Max(x => x.Tid);

                var rownames = new List<string>()
                {
                    "Page Number", "Transaction ID",
                    "Data Root Page", "Free Space Root Page", "Last Page Written",
                    "Data Total Count", "Data Local Count", "Free Space Total Count", "Free Space Local Count"
                };

                var rowvalues0 = new List<object>()
                {
                    props.MetaInfos[0].PageNumber,
                    props.MetaInfos[0].Tid,
                    props.MetaInfos[0].DataRootPage,
                    props.MetaInfos[0].FsRootPage,
                    props.MetaInfos[0].LastPage,
                    props.MetaInfos[0].DataTotalCount,
                    props.MetaInfos[0].DataLocalCount,
                    props.MetaInfos[0].FsTotalCount,
                    props.MetaInfos[0].FsLocalCount,
                };

                var rowvalues1 = new List<object>()
                {
                    props.MetaInfos[1].PageNumber,
                    props.MetaInfos[1].Tid,
                    props.MetaInfos[1].DataRootPage,
                    props.MetaInfos[1].FsRootPage,
                    props.MetaInfos[1].LastPage,
                    props.MetaInfos[1].DataTotalCount,
                    props.MetaInfos[1].DataLocalCount,
                    props.MetaInfos[1].FsTotalCount,
                    props.MetaInfos[1].FsLocalCount,
                };

                var rowtext = new List<bool>()
                {
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    true,
                    true,
                    true,
                };

                for (int i = 0; i < rownames.Count; i++)
                {
                    var row = dgViewMeta.Rows[dgViewMeta.Rows.Add()];

                    if (rowtext[i])
                    {
                        row.Cells[0] = new DataGridViewTextBoxCell();
                        row.Cells[1] = new DataGridViewTextBoxCell();
                    }

                    row.HeaderCell.Value = rownames[i];
                    row.Cells[colMeta0.Name].Value = rowvalues0[i];
                    row.Cells[colMeta1.Name].Value = rowvalues1[i];

                    if (rownames[i] == "Transaction ID")
                    {
                        if (props.MetaInfos[0].Tid == maxtid)
                        {
                            row.Cells[0].Style.BackColor = Color.LightGreen;
                        }

                        else if (props.MetaInfos[1].Tid == maxtid)
                        {
                            row.Cells[1].Style.BackColor = Color.LightGreen;
                        }
                    }
                }
            }
        }

        public void ShowPageCounts(DatabaseProperties props, FileMap map)
        {
            dgViewPageCounts.Rows.Clear();

            if (props != null && map != null)
            {
                var rownames = new List<string>()
                {
                    "Total", "Data Index", "Data Leaf", "Data Overflow", "Freespace Index", "Freespace Leaf", "Freespace", "Unreferenced"
                };

                var rowvalues0 = new List<ulong>()
                {
                    props.MetaInfos[0].LastPage - props.FirstDataPage + 1,
                    map.GetPageCount(0, PageTypesI.DataIndex),
                    map.GetPageCount(0, PageTypesI.DataLeaf),
                    map.GetPageCount(0, PageTypesI.DataOverflow) + map.GetPageCount(0, PageTypesI.DataOverflowCont),
                    map.GetPageCount(0, PageTypesI.FsIndex),
                    map.GetPageCount(0, PageTypesI.FsLeaf),
                    map.GetPageCount(0, PageTypesI.FreeSpace) + map.GetPageCount(0, PageTypesI.FreeSpaceInUse),
                    map.GetUnreferencedCount(0, props),
                };

                var rowvalues1 = new List<ulong>()
                {
                    props.MetaInfos[1].LastPage - props.FirstDataPage + 1,
                    map.GetPageCount(1, PageTypesI.DataIndex),
                    map.GetPageCount(1, PageTypesI.DataLeaf),
                    map.GetPageCount(1, PageTypesI.DataOverflow) + map.GetPageCount(1, PageTypesI.DataOverflowCont),
                    map.GetPageCount(1, PageTypesI.FsIndex),
                    map.GetPageCount(1, PageTypesI.FsLeaf),
                    map.GetPageCount(1, PageTypesI.FreeSpace) + map.GetPageCount(1, PageTypesI.FreeSpaceInUse),
                    map.GetUnreferencedCount(1, props),
                };

                for (int i = 0; i < rownames.Count; i++)
                {
                    var row = dgViewPageCounts.Rows[dgViewPageCounts.Rows.Add()];

                    row.HeaderCell.Value = rownames[i];
                    row.Cells[colPageCountsMeta0.Name].Value = rowvalues0[i];
                    row.Cells[colSizeMeta0.Name].Value = GetMegaBytes(rowvalues0[i], props.PageSize);
                    row.Cells[colPageCountsMeta0P.Name].Value = GetPercentage(rowvalues0[i], rowvalues0[0]);
                    row.Cells[colPageCountsMeta1.Name].Value = rowvalues1[i];
                    row.Cells[colSizeMeta1.Name].Value = GetMegaBytes(rowvalues1[i], props.PageSize);
                    row.Cells[colPageCountsMeta1P.Name].Value = GetPercentage(rowvalues1[i], rowvalues1[0]); ;
                }
            }
        }

        public void ShowUnusedSpace(DatabaseProperties props, FileMap map)
        {
            dgViewUnusedPages.Rows.Clear();

            if (props != null && map != null)
            {
                var rownames = new List<string>()
                {
                    "Total", "Data Index", "Data Leaf", "Freespace Index", "Freespace Leaf"
                };

                var rowvalues0 = new List<ulong>()
                {
                    map.GetUnusedSpace(0),
                    map.GetUnusedSpace(0, PageTypesI.DataIndex),
                    map.GetUnusedSpace(0, PageTypesI.DataLeaf),
                    map.GetUnusedSpace(0, PageTypesI.FsIndex),
                    map.GetUnusedSpace(0, PageTypesI.FsLeaf),
                };

                var rowvalues1 = new List<ulong>()
                {
                    map.GetUnusedSpace(1),
                    map.GetUnusedSpace(1, PageTypesI.DataIndex),
                    map.GetUnusedSpace(1, PageTypesI.DataLeaf),
                    map.GetUnusedSpace(1, PageTypesI.FsIndex),
                    map.GetUnusedSpace(1, PageTypesI.FsLeaf),
                };


                for (int i = 0; i < rownames.Count; i++)
                {
                    var row = dgViewUnusedPages.Rows[dgViewUnusedPages.Rows.Add()];

                    row.HeaderCell.Value = rownames[i];
                    row.Cells[colUnusedSpaceMeta0.Name].Value = GetMegaBytes(rowvalues0[i], 1);
                    row.Cells[colUnusedSpaceMeta0P.Name].Value = GetPercentage(rowvalues0[i], rowvalues0[0]);
                    row.Cells[colUnusedSpaceMeta1.Name].Value = GetMegaBytes(rowvalues1[i], 1);
                    row.Cells[colUnusedSpaceMeta1P.Name].Value = GetPercentage(rowvalues1[i], rowvalues1[0]); ;
                }
            }
        }

        private double GetMegaBytes(ulong count, uint pagesize)
        {
            return count * pagesize / 1024.0 / 1024.0;
        }

        private object GetPercentage(ulong val, ulong total)
        {
            if (total == 0) return 0;

            return (double)val / (double)total * 100.0;
        }

        #endregion

        private void dgViewMeta_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgViewMeta.SelectedCells.Count > 0)
            {
                var cell = dgViewMeta.SelectedCells[0];
                if (cell is DataGridViewLinkCell && cell.Value != null)
                {
                    var pageno = (KvPagenumber)cell.Value;

                    // TODO set SelectedMetaIndex
                    Presenter.Model.ActiveMetaIndex = cell.ColumnIndex;

                    Presenter.Context.RaiseEvent(Presenter, new ShowPageRequestedEvent(pageno), true);
                }
            }
        }
    }
}
