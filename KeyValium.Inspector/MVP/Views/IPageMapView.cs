using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;

namespace KeyValium.Inspector.MVP.Views
{
    public interface IPageMapView : IViewGeneric<PageMapPresenter>
    {
        KvPagenumber? MaxPagenumber 
        { 
            get;
            set; 
        }

        PageMap PageMap
        {
            get;
            set;
        }
    }
}
