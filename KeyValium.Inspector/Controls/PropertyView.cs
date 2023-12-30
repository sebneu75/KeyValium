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

            if (props != null)
            {
                //lblHostByteorder.Text = string.Format("{0}", props.HostByteOrder);
                lblFilename.Text = props.Filename;
                lblFilesize.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0} Bytes", props.FileSize);
                lblFirstDatapage.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.FirstDataPage);
                lblFirstMetapage.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.FirstMetaPage);
                lblMaxEntrysize.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0} Bytes", props.MaxKeyAndValueSize);
                lblMaxKeysize.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0} Bytes", props.MaxKeySize);
                lblMetapages.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.MetaPages);
                lblPagecount.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.PageCount);
                lblPagesize.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0} Bytes", props.PageSize);
                lblVersion.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.Version);
                lblInternalTypecode.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.InternalTypecode);
                lblUserTypecode.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", props.UserTypecode);

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
            else
            {
                lblFilename.Text = "";
                lblFilesize.Text = "";
                lblFirstDatapage.Text = "";
                lblFirstMetapage.Text = "";
                lblMaxEntrysize.Text = "";
                lblMaxKeysize.Text = "";
                lblMetapages.Text = "";
                lblPagecount.Text = "";
                lblPagesize.Text = "";
                lblVersion.Text = "";
                lblInternalTypecode.Text = "";
                lblUserTypecode.Text = "";
            }
        }

        public void ShowPageCounts(DatabaseProperties props, FileMap map)
        {
            dgViewPageCounts2.Rows.Clear();

            if (props != null && map != null)
            {
                var rownames = new List<string>()
                {
                    "Total", "DataIndex", "DataLeaf", "Overflow", "FsIndex", "FsLeaf", "FreeSpace", "Unreferenced"
                };

                var rowvalues0 = new List<ulong>()
                {
                    map.GetPageCount(0),
                    map.GetPageCount(0, PageTypesI.DataIndex),
                    map.GetPageCount(0, PageTypesI.DataLeaf),
                    map.GetPageCount(0, PageTypesI.DataOverflow) + map.GetPageCount(0, PageTypesI.DataOverflowCont),
                    map.GetPageCount(0, PageTypesI.FsIndex),
                    map.GetPageCount(0, PageTypesI.FsLeaf),
                    map.GetPageCount(0, PageTypesI.FreeSpace) + map.GetPageCount(0, PageTypesI.FreeSpaceInUse),
                    map.GetPageCount(0, PageTypesI.Unknown),
                };

                var rowvalues1 = new List<ulong>()
                {
                    map.GetPageCount(1),
                    map.GetPageCount(1, PageTypesI.DataIndex),
                    map.GetPageCount(1, PageTypesI.DataLeaf),
                    map.GetPageCount(1, PageTypesI.DataOverflow) + map.GetPageCount(0, PageTypesI.DataOverflowCont),
                    map.GetPageCount(1, PageTypesI.FsIndex),
                    map.GetPageCount(1, PageTypesI.FsLeaf),
                    map.GetPageCount(1, PageTypesI.FreeSpace) + map.GetPageCount(0, PageTypesI.FreeSpaceInUse),
                    map.GetPageCount(1, PageTypesI.Unknown),
                };

                for (int i = 0; i < rownames.Count; i++)
                {
                    var row = dgViewPageCounts2.Rows[dgViewPageCounts2.Rows.Add()];

                    row.HeaderCell.Value = rownames[i];
                    row.Cells[colPageCountsMeta0.Name].Value = rowvalues0[i];
                    row.Cells[colPageCountsMeta0P.Name].Value = GetPercentage(rowvalues0[i], rowvalues0[0]);
                    row.Cells[colPageCountsMeta1.Name].Value = rowvalues1[i];
                    row.Cells[colPageCountsMeta1P.Name].Value = GetPercentage(rowvalues1[i], rowvalues1[0]); ;
                }
            }
        }

        private object GetPercentage(ulong val, ulong total)
        {
            if (total == 0) return 0;

            return (double)val / (double)total;
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
