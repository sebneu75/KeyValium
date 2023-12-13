using KeyValium.Inspector;
using KeyValium.Inspector.Controls;
using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;
using System.Collections.Generic;

namespace KeyValium.Inspector.MVP.Views
{
    public interface IFreeSpaceView : IViewGeneric<FreeSpacePresenter>
    {
        void Clear();

        void SetFreeSpaceList(IReadOnlyList<FsEntryWrapper> list);
    }
}
