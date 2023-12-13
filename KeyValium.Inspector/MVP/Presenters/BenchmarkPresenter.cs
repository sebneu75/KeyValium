using KeyValium.Inspector.MVP.Models;
using KeyValium.Inspector.MVP.Views;
using Mad.MVP;
using System.Collections.Generic;

namespace KeyValium.Inspector.MVP.Presenters
{
    public class BenchmarkPresenter : PresenterBaseGeneric<IBenchmarkView, InspectorContext, InspectorModel, BenchmarkPresenter>
    {
        public BenchmarkPresenter(IBenchmarkView view, EventContext context) : base(view, context)
        {
        }

        protected override void OnModelChanged(InspectorModel oldmodel)
        {
        }

        //internal List<TestDescription> LoadBenchmarks()
        //{
        //    return Model.LoadBenchmarks();
        //}
    }
}
