using KeyValium.Cursors;
using KeyValium.Frontends.Serializers;
using KeyValium.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Frontends
{
    public class KvMultiDictionary : IDisposable
    {
        private KvMultiDictionary(string filename, DatabaseOptions options)
        {
            Perf.CallCount();

            options.InternalTypeCode = InternalTypes.MultiDictionary;
            _db = Database.Open(filename, options);

            _serializer = new KvJsonSerializer();
        }

        private readonly Database _db;

        internal readonly KvJsonSerializer _serializer;

        public static KvMultiDictionary Open(string filename)
        {
            Perf.CallCount();

            return new KvMultiDictionary(filename, new DatabaseOptions());
        }

        public static KvMultiDictionary Open(string filename, DatabaseOptions options)
        {
            Perf.CallCount();

            return new KvMultiDictionary(filename, options);
        }

        private Transaction _autotx;
        private Transaction _manualtx;

        internal object _txlock = new object();

        internal Transaction CurrentTx
        {
            get
            {
                Perf.CallCount();

                lock (_txlock)
                {
                    if (_manualtx != null)
                    {
                        return _manualtx;
                    }

                    if (_autotx == null)
                    {
                        _autotx = _db.BeginWriteTransaction();
                    }

                    return _autotx;
                }
            }
        }

        public void BeginTransaction()
        {
            Perf.CallCount();

            lock (_txlock)
            {
                if (_manualtx == null)
                {
                    _manualtx = _db.BeginWriteTransaction();
                }
            }
        }

        internal void CommitAuto()
        {
            Perf.CallCount();

            lock (_txlock)
            {
                _autotx?.Commit();
                _autotx?.Dispose();
                _autotx = null;
            }
        }

        public void RollbackAuto()
        {
            Perf.CallCount();

            lock (_txlock)
            {
                _autotx?.Rollback();
                _autotx?.Dispose();
                _autotx = null;
            }
        }

        public void CommitTransaction()
        {
            Perf.CallCount();

            lock (_txlock)
            {
                _manualtx?.Commit();
                _manualtx?.Dispose();
                _manualtx = null;
            }
        }

        public void RollbackTransaction()
        {
            Perf.CallCount();

            lock (_txlock)
            {
                _manualtx?.Rollback();
                _manualtx?.Dispose();
                _manualtx = null;
            }
        }

        private KvDictionaryInfo GetDictionaryInfo(string name)
        {
            Perf.CallCount();

            var namebytes = _serializer.Serialize(name);

            // check if dictionary exists
            using (var tx = _db.BeginReadTransaction())
            {
                // validate existing dictionary
                var val = tx.Get(null, namebytes);
                if (val.IsValid)
                {
                    var di = _serializer.Deserialize<KvDictionaryInfo>(val.ValueSpan);
                    di.Name = name;
                    return di;
                }
            }

            return null;
        }

        public IList<KvDictionaryInfo> GetDictionaries()
        {
            Perf.CallCount();

            var list = new List<KvDictionaryInfo>();

            using (var tx = _db.BeginReadTransaction())
            {
                using (var iter = tx.GetIterator(null, true))
                {
                    while (iter.MoveNext())
                    {                        
                        var val = iter.Current.Value;
                        if (val.IsValid)
                        {
                            var key = iter.Current.Value.Key;
                            var name = _serializer.Deserialize<string>(key);
                            var di = _serializer.Deserialize<KvDictionaryInfo>(val.ValueSpan);
                            di.Name = name;
                            list.Add(di);
                        }
                    }
                }
            }

            return list;
        }

        public KvDictionary<TKey, TValue> EnsureDictionary<TKey, TValue>(string name, bool zipvalues)
        {
            Perf.CallCount();

            var dict = GetDictionaryInfo(name);

            if (dict == null)
            {
                dict = new KvDictionaryInfo()
                {
                    Name = name,
                    KeyType = typeof(TKey).FullName,
                    ValueType = typeof(TValue).FullName,
                    ZipValues = zipvalues
                };

                var key = _serializer.Serialize(name);

                // create dictionary
                using (var tx = _db.BeginWriteTransaction())
                {
                    tx.EnsureTreeRef(TrackingScope.TransactionChain, key);
                    tx.Upsert(null, key, _serializer.Serialize(dict));

                    tx.Commit();
                }
            }
            else
            {
                // TODO validate types
            }

            return new KvDictionary<TKey, TValue>(this, name, dict);
        }

        public bool DeleteDictionary(string name)
        {
            Perf.CallCount();

            var key = _serializer.Serialize(name);

            using (var tx = _db.BeginWriteTransaction())
            {
                var keyref = tx.GetTreeRef(TrackingScope.TransactionChain, key);
                if (keyref != null)
                {
                    tx.DeleteTree(keyref);
                    keyref.Dispose();
                }

                tx.Delete(null, key);

                tx.Commit();
            }

            return false;
        }

        //public bool DeleteDictionary(string name)
        //{

        //}

        #region IValidatable

        public void Validate()
        {
            Perf.CallCount();

            if (_isdisposed)
            {
                throw new ObjectDisposedException("MultiDictionary is already disposed.");
            }

            _db.Validate();
        }

        #endregion

        #region IDisposable

        private bool _isdisposed;

        protected virtual void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _isdisposed = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~MultiDictionary()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
