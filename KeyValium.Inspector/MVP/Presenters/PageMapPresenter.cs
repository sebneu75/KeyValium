using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class PageMapPresenter : PresenterBaseGeneric<IPageMapView, InspectorContext, InspectorModel, PageMapPresenter>
    {
        public PageMapPresenter(IPageMapView view, EventContext context) : base(view, context)
        {
            History = new PageNumberHistory();
        }

        protected override void OnModelChanged(InspectorModel oldmodel)
        {
            _pageno = null;
            History.Clear();
            View.PageMap = null;            
            View.MaxPagenumber = (KvPagenumber?)Model?.Inspector?.Properties?.PageCount -1;
        }

        protected override void OnSubscribeModelEvents(InspectorModel newmodel)
        {
            newmodel.SelectedMetaChanged += SelectedMetaChanged;
        }

        protected override void OnUnsubscribeModelEvents(InspectorModel oldmodel)
        {
            oldmodel.SelectedMetaChanged -= SelectedMetaChanged;
        }

        private void SelectedMetaChanged(object sender, System.EventArgs e)
        {
            ShowPage(_pageno);
        }

        private KvPagenumber? _pageno;

        internal void ShowPage(KvPagenumber? pageno)
        {
            _pageno = pageno;

            if (Model.Map != null && pageno.HasValue)
            {
                History.Add(pageno.Value);
                var pagetype = Model.Map.GetPageType((short)Model.ActiveMeta.Index, pageno.Value);
                View.PageMap = Model?.Inspector?.GetPageMap(pageno.Value, pagetype);
            }
        }

        #region History

        internal PageNumberHistory History
        {
            get;
            private set;
        }

        #endregion
    }
}
