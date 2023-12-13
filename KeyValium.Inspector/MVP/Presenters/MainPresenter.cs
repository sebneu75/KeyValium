using KeyValium.Inspector.MVP.Events;
using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class MainPresenter : PresenterBaseGeneric<IMainView, InspectorContext, InspectorModel, MainPresenter>
    {
        public MainPresenter(IMainView view, EventContext context) : base(view, context)
        {
        }

        protected override void OnInitializeChildPresenters()
        {
            PropertyP = new PropertyPresenter(View.PropertyView, Context);
            FileMapP = new FileMapPresenter(View.FileMapView, Context);
            PageMapP = new PageMapPresenter(View.PageMapView, Context);
            BenchmarkP = new BenchmarkPresenter(View.BenchmarkView, Context);
            FreespaceP = new FreeSpacePresenter(View.FreeSpaceView, Context);

            //ResourceP = new ResourcePresenter(View.ResourceView, this.Context);
            //BrowserP = new BrowserPresenter(View.BrowserView, this.Context);
            //DownloadP = new DownloadPresenter(View.DownloadView, this.Context);
        }

        protected override void OnAppContextChanged(InspectorContext oldappcontext)
        {
            PropertyP.AppContext = AppContext;
            FileMapP.AppContext = AppContext;
            PageMapP.AppContext = AppContext;
            BenchmarkP.AppContext = AppContext;
            FreespaceP.AppContext = AppContext;
        }

        internal void ShowProjects()
        {
            //ProjectP.View.AddProjects(Model.Projects);
        }

        protected async override void OnModelChanged(InspectorModel oldmodel)
        {
            PropertyP.Model = Model;
            FileMapP.Model = Model;
            PageMapP.Model = Model;
            BenchmarkP.Model = Model;
            FreespaceP.Model = Model;

            View.SetTitle(Model.Inspector?.Properties?.Filename);

            var metas = Model?.Metas;
            if (metas != null)
            {
                View.SetMetas(metas, metas.OrderBy(x => x.Tid).Last().Index);
            }
            else
            {
                View.SetMetas(null, null);
            }

            await LoadFileMap();
        }

        protected override void OnSubscribeModelEvents(InspectorModel newmodel)
        {
            newmodel.FileMapChanged += FileMapChanged;
            newmodel.SelectedMetaChanged += SelectedMetaChanged;
        }

        private void SelectedMetaChanged(object sender, EventArgs e)
        {
            View.SelectMeta(Model.ActiveMeta);
        }

        protected override void OnUnsubscribeModelEvents(InspectorModel oldmodel)
        {
            oldmodel.FileMapChanged -= FileMapChanged;
            oldmodel.SelectedMetaChanged -= SelectedMetaChanged;
        }

        private void FileMapChanged(object sender, EventArgs e)
        {
            PropertyP.View.ShowPageCounts(Model?.Inspector?.Properties, Model?.Map);
        }

        internal void InspectDatabase(string filename, string password, string keyfile)
        {
            var temp = new InspectorModel();
            temp.InspectDatabase(filename, password, keyfile);

            Model = temp;
        }

        public PropertyPresenter PropertyP
        {
            get;
            private set;
        }

        public FileMapPresenter FileMapP
        {
            get;
            private set;
        }

        public PageMapPresenter PageMapP
        {
            get;
            private set;
        }

        public BenchmarkPresenter BenchmarkP
        {
            get;
            private set;
        }

        public FreeSpacePresenter FreespaceP
        {
            get;
            private set;
        }

        public override void OnEventRaised(PresenterBase sender, ContextEvent e)
        {
            var spe = e as ShowPageRequestedEvent;
            if (spe != null)
            {
                View.ActivatePageMap();
                PageMapP.ShowPage(spe.PageNumber);
            }
        }

        internal void CloseDatabase()
        {
            Model?.CloseDatabase();
        }

        CancellationTokenSource _canceltokensource;

        internal async Task<FileMap> LoadFileMap()
        {
            if (Model == null || Model.Inspector == null || Model.Inspector.Properties == null)
            {
                return null;
            }

            View.ShowLoadingPanel();

            var progress = new FileMapProgress((ulong)Model.Inspector.Properties.PageCount);
            progress.ProgressChanged += Progress_ProgressChanged;

            _canceltokensource = new CancellationTokenSource();

            try
            {
                Model.Map = await Task.Run(() => Model.Inspector.GetFileMap(progress, _canceltokensource.Token), _canceltokensource.Token);
                return Model.Map;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _canceltokensource = null;
                progress.ProgressChanged -= Progress_ProgressChanged;
                View.HideLoadingPanel();
            }

            return null;
        }

        private void Progress_ProgressChanged(object sender, EventArgs e)
        {
            var progress = sender as FileMapProgress;
            if (progress != null)
            {
                View.UpdateProgress(progress.Current, progress.Total);
            }
        }

        internal void CancelLoadFileMap()
        {
            _canceltokensource?.Cancel();
        }
    }
}
