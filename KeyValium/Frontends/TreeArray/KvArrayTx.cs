using KeyValium.Frontends.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    public class KvArrayTx : IDisposable
    {
        internal const int MaxKeys = 16;

        public KvArrayTx(Transaction tx)
        {
            _tx = tx;

            Serializer = new KvArraySerializer(Limits.GetMaxKeyLength(tx.PageSize));
        }

        internal readonly KvArraySerializer Serializer;

        private readonly object _lock = new object();

        public KvArrayValue this[params KvArrayKey[] indices]
        {
            get
            {
                ValidateKeys(indices);

                lock (_lock)
                {
                    //if (_tx.TryGetTreeRef(TrackingScope.Database, out var treeref, Serializer.GetPath(ref indices)))
                    //{
                    //    using (treeref)
                    //    {
                    //        var val = _tx.Get(treeref, Serializer.GetKey(ref indices).Span);

                    //        return CreateValue(ref val);
                    //    }
                    //}

                    return null;
                }
            }
            set
            {
                ValidateKeys(indices);

                lock (_lock)
                {
                    //using (var treeref = _tx.EnsureTreeRef(TrackingScope.Database, Serializer.GetPath(ref indices)))
                    //{
                    //    var x = new ValInfo();
                    //    _tx.Upsert(treeref, Serializer.GetKey(ref indices).Span,  value);

                    //    return val.
                    //}
                }
            }
        }

        //internal KvArrayValue CreateValue(ref ValueRef val)
        //{
        //    if (!val.IsValid)
        //    {
        //        return null;
        //    }

        //    return Serializer.GetValue(ref val);

        //}

        private void ValidateKeys(KvArrayKey[] keys)
        {
            if (keys.Length < 1)
            {
                throw new ArgumentException("At least one argument must be given");
            }

            if (keys.Length > MaxKeys)
            {
                var msg = string.Format("At most {0} keys can be given", MaxKeys);
                throw new ArgumentException(msg);
            }
        }

        private Transaction _tx;

        public bool IsReadOnly
        {
            get
            {
                return _tx.IsReadOnly;
            }
        }

        public KvArrayTx BeginChildTransaction()
        {
            return new KvArrayTx(_tx.BeginChildTransaction());
        }

        public void Commit()
        {
            _tx.Commit();
        }

        public void Rollback()
        {
            _tx.Rollback();
        }

        public void Dispose()
        {
            _tx?.Dispose();
        }
    }
}
