using KeyValium.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeyValium.Inspector.MVP.Models
{
    public class InspectorModel
    {
        public InspectorModel()
        {
        }

        private MetaInfo _meta;

        public int? ActiveMetaIndex
        {
            get
            {
                return _meta?.Index;
            }
            set
            {
                if (Metas != null)
                {
                    ActiveMeta = Metas.FirstOrDefault(x => x.Index == value);
                }                
            }
        }

        public MetaInfo ActiveMeta
        {
            get
            {
                return _meta;
            }
            internal set
            {
                if (_meta != value)
                {
                    _meta = value;
                    RaiseSelectedMetaChanged();
                }
            }
        }

        public IReadOnlyList<MetaInfo> Metas
        {
            get
            {
                return Inspector?.Properties?.MetaInfos;
            }
        }

        public event EventHandler<EventArgs> SelectedMetaChanged;

        private void RaiseSelectedMetaChanged()
        {
            SelectedMetaChanged?.Invoke(this, EventArgs.Empty);
        }

        public void InspectDatabase(string filename, string password, string keyfile)
        {
            if (Inspector != null)
            {
                Inspector.Dispose();
                Inspector = null;
            }

            if (!string.IsNullOrWhiteSpace(filename))
            {
                Inspector = new DbInspector(filename, password, keyfile);
            }
        }

        public DbInspector Inspector
        {
            get;
            private set;
        }

        public event EventHandler<EventArgs> FileMapChanged;

        private void RaiseFileMapChanged()
        {
            FileMapChanged?.Invoke(this, EventArgs.Empty);
        }

        private FileMap _map;
        public FileMap Map
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
                RaiseFileMapChanged();
            }
        }

        //internal List<TestDescription> LoadBenchmarks()
        //{
        //    TestDescription.WorkingPath = @"d:\Work\Temp";

        //    var benchmarks = TestDescription.LoadAll();
        //    benchmarks = benchmarks.Where(x => x.ParameterName != null).ToList();

        //    return benchmarks;
        //}

        internal void CloseDatabase()
        {
            Inspector?.Dispose();
            Inspector = null;
        }
    }
}
