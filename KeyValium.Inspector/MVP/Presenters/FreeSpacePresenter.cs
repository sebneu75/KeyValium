using KeyValium.Inspector;
using KeyValium.Inspector.Controls;
using KeyValium.Inspector.MVP.Events;
using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class FreeSpacePresenter : PresenterBaseGeneric<IFreeSpaceView, InspectorContext, InspectorModel, FreeSpacePresenter>
    {
        public FreeSpacePresenter(IFreeSpaceView view, EventContext context) : base(view, context)
        {
        }

        protected override void OnModelChanged(InspectorModel oldmodel)
        {
            View.Clear();
        }

        protected override void OnSubscribeModelEvents(InspectorModel newmodel)
        {
            newmodel.FileMapChanged += SelectedMetaChanged;
            newmodel.SelectedMetaChanged += SelectedMetaChanged;
        }

        protected override void OnUnsubscribeModelEvents(InspectorModel oldmodel)
        {
            oldmodel.FileMapChanged -= SelectedMetaChanged;
            oldmodel.SelectedMetaChanged -= SelectedMetaChanged;
        }

        private void SelectedMetaChanged(object sender, EventArgs e)
        {
            var meta = Model.ActiveMeta;
            var metaindex = meta == null ? 0 : meta.Index;

            var list = Model.Map?.GetFreespaceList((short)metaindex).Select(x => new FsEntryWrapper(x)).ToList();

            View.SetFreeSpaceList(list);
        }

        internal void ShowPage(KvPagenumber pageno)
        {
            Context.RaiseEvent(this, new ShowPageRequestedEvent(pageno), true);
        }
    }
}
