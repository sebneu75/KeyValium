using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;
using System.Collections.Generic;

namespace KeyValium.Inspector.MVP.Views
{
    public interface IMainView : IViewGeneric<MainPresenter>
    {
        #region child views

        IPropertyView PropertyView
        {
            get;
        }

        IFileMapView FileMapView
        {
            get;
        }

        IPageMapView PageMapView
        {
            get;
        }

        IBenchmarkView BenchmarkView
        {
            get;
        }

        IFreeSpaceView FreeSpaceView
        {
            get;
        }

        #endregion

        void SetTitle(string title);

        void ActivatePageMap();

        void UpdateProgress(ulong current, ulong total);

        void ShowLoadingPanel();

        void HideLoadingPanel();

        void SetMetas(IReadOnlyList<MetaInfo> metaInfos, int? meta);

        void SelectMeta(MetaInfo activeMeta);
    }
}
