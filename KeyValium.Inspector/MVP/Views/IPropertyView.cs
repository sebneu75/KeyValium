using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;

namespace KeyValium.Inspector.MVP.Views
{
    public interface IPropertyView : IViewGeneric<PropertyPresenter>
    {
        void ShowDatabaseProperties(DatabaseProperties props);

        void ShowPageCounts(DatabaseProperties props, FileMap map);

        void ShowUnusedSpace(DatabaseProperties props, FileMap map);
    }
}
