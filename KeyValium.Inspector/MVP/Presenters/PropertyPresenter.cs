using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class PropertyPresenter : PresenterBaseGeneric<IPropertyView, InspectorContext, InspectorModel, PropertyPresenter>
    {
        public PropertyPresenter(IPropertyView view, EventContext context) : base(view, context)
        {
        }

        protected async override void OnModelChanged(InspectorModel oldmodel)
        {
            View.ShowDatabaseProperties(Model?.Inspector?.Properties);
        }

        protected override void OnSubscribeModelEvents(InspectorModel newmodel)
        {
            newmodel.FileMapChanged += FileMapChanged;
        }

        protected override void OnUnsubscribeModelEvents(InspectorModel oldmodel)
        {
            oldmodel.FileMapChanged -= FileMapChanged;
        }

        private void FileMapChanged(object sender, EventArgs e)
        {
            View.ShowPageCounts(Model?.Inspector?.Properties, Model?.Map);
        }
    }
}
