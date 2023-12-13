using KeyValium.Inspector.MVP.Events;
using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class FileMapPresenter : PresenterBaseGeneric<IFileMapView, InspectorContext, InspectorModel, FileMapPresenter>
    {
        public FileMapPresenter(IFileMapView view, EventContext context) : base(view, context)
        {
        }

        protected override void OnModelChanged(InspectorModel oldmodel)
        {
            View.ClearFileMap();
        }

        protected override void OnSubscribeModelEvents(InspectorModel newmodel)
        {
            newmodel.FileMapChanged += FileMapChanged;
            newmodel.SelectedMetaChanged += SelectedMetaChanged;
        }

        protected override void OnUnsubscribeModelEvents(InspectorModel oldmodel)
        {
            oldmodel.FileMapChanged -= FileMapChanged;
            oldmodel.SelectedMetaChanged -= SelectedMetaChanged;
        }

        private void SelectedMetaChanged(object sender, EventArgs e)
        {
            View.SetMetaInfo(Model.ActiveMeta);
        }

        private void FileMapChanged(object sender, EventArgs e)
        {
            View.SetFileMap(Model?.Map);
            View.SetMetaInfo(Model.ActiveMeta);            
        }

        internal void ShowPage(KvPagenumber pageno)
        {
            Context.RaiseEvent(this, new ShowPageRequestedEvent(pageno), true);
        }
    }
}
