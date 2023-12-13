using KeyValium.Cursors;
using KeyValium.Frontends.Serializers;
using KeyValium.Logging;
using KeyValium.Pages.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends
{
    public class KvDictionary<TKey, TValue>
    {
        internal KvDictionary(KvMultiDictionary md, string name, KvDictionaryInfo info)
        {
            Perf.CallCount();

            Parent = md;
            Name = name;
            Info = info;
            Serializer = md._serializer;

            SerializedName = Serializer.Serialize(name);
        }

        private readonly KvJsonSerializer Serializer;

        private readonly KvMultiDictionary Parent;

        public readonly string Name;

        private readonly byte[] SerializedName;

        public readonly KvDictionaryInfo Info;

        private TreeRef _dictref;

        private TreeRef GetDictionaryRef(Transaction tx)
        {
            Perf.CallCount();

            if (_dictref == null)
            {
                _dictref = tx.GetTreeRef(TrackingScope.Database, SerializedName);
                if (_dictref == null)
                {
                    throw new ArgumentException("Dictionary KeyRef not found!");
                }
            }

            return _dictref;
        }

        public TValue Get(TKey key)
        {
            Perf.CallCount();

            try
            {
                TValue ret = default;
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                var val = tx.Get(keyref, Serializer.Serialize(key));
                if (val.IsValid)
                {
                    ret = Serializer.Deserialize<TValue>(val.ValueSpan, Info.ZipValues);
                }

                Parent.CommitAuto();
                return ret;
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }

        /// <summary>
        /// Updates the key. Will fail if key does not exist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        internal void Update(TKey key, TValue val)
        {
            Perf.CallCount();

            try
            {
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                tx.Update(keyref, Serializer.Serialize(key), Serializer.Serialize(val, Info.ZipValues));

                Parent.CommitAuto();
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }

        public bool Upsert(TKey key, TValue val)
        {
            Perf.CallCount();

            try
            {
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                var ret = tx.Upsert(keyref, Serializer.Serialize(key), Serializer.Serialize(val, Info.ZipValues));

                Parent.CommitAuto();

                return ret;
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }

        internal void Insert(TKey key, TValue val)
        {
            Perf.CallCount();

            try
            {
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                tx.Insert(keyref, Serializer.Serialize(key), Serializer.Serialize(val, Info.ZipValues));

                Parent.CommitAuto();
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }

        internal bool Delete(TKey key)
        {
            Perf.CallCount();

            try
            {
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                var ret = tx.Delete(keyref, Serializer.Serialize(key));

                Parent.CommitAuto();

                return ret;
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }

        internal bool Exists(TKey key)
        {
            Perf.CallCount();

            try
            {
                var tx = Parent.CurrentTx;

                var keyref = GetDictionaryRef(tx);
                var ret = tx.Exists(keyref, Serializer.Serialize(key));

                Parent.CommitAuto();

                return ret;
            }
            catch (Exception)
            {
                Parent.RollbackAuto();
                throw;
            }
        }
    }
}
