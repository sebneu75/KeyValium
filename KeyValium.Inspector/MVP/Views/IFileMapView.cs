using KeyValium.Inspector;
using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;
using System.Collections.Generic;

namespace KeyValium.Inspector.MVP.Views
{
    public interface IFileMapView : IViewGeneric<FileMapPresenter>
    {
        void SetFileMap(FileMap map);

        void ClearFileMap();

        void SetMetaInfo(MetaInfo mi);
    }
}
