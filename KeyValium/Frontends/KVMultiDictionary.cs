using KeyValium.Cursors;
using KeyValium.Frontends.Serializers;
using KeyValium.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeyValium.Frontends
{
    public class KvMultiDictionary : IDisposable
    {
        #region Constructor

        private KvMultiDictionary(string filename, DatabaseOptions options)
        {
            Perf.CallCount();

            options.InternalTypeCode = InternalTypes.MultiDictionary;
            _db = Database.Open(filename, options);

            DefaultSerializer = new KvJsonSerializer(new KvJsonSerializerOptions());
        }

        #endregion

        #region Variables

        private readonly Database _db;

        internal readonly IKvSerializer DefaultSerializer;

        internal object MdLock = new object();

        private Transaction _tx;

        #endregion

        #region Public API

        /// <summary>
        /// Opens a MultiDictionary with default options. If the file does not exist it will be created.
        /// </summary>
        /// <param name="filename">The database filename.</param>
        /// <returns>An instance of KvMultiDictionary.</returns>
        public static KvMultiDictionary Open(string filename)
        {
            Perf.CallCount();

            return new KvMultiDictionary(filename, new DatabaseOptions());
        }

        /// <summary>
        /// Opens a MultiDictionary with user defined options. If the file does not exist it will be created 
        /// if options.CreateIfNotExists is true.
        /// </summary>
        /// <param name="filename">The database filename.</param>
        /// <returns>An instance of KvMultiDictionary.</returns>
        public static KvMultiDictionary Open(string filename, DatabaseOptions options)
        {
            Perf.CallCount();

            return new KvMultiDictionary(filename, options);
        }
               
        /// <summary>
        /// Does an action within a transaction.
        /// Calls can be nested.
        /// If action throws an exception the transaction is rolled back. Otherwise the transaction is commited.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="appendmode"></param>
        public void DoInTransaction(Action action, bool appendmode = false)
        {
            lock (MdLock)
            {
                var created = EnsureTransaction();

                if (created) 
                {
                    Tx.AppendMode = appendmode;
                }
                
                try
                {
                    action.Invoke();

                    if (created)
                    {
                        //UpdateCounts();
                        CommitTransaction();
                    }
                }
                catch (Exception ex)
                {
                    if (created)
                    {
                        RollbackTransaction();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Returns information about the requested dictionary. If the dictionary does not exists null is returned.
        /// </summary>
        /// <param name="name">Name of the dictionary.</param>
        /// <returns></returns>
        public KvDictionaryInfo GetDictionaryInfo(string name)
        {
            Perf.CallCount();

            ValidateName(name);

            var namebytes = DefaultSerializer.Serialize(name, false);

            KvDictionaryInfo ret = null;

            DoInTransaction(() =>
            {
                var val = _tx.Get(null, namebytes);
                if (val.IsValid)
                {
                    ret = DefaultSerializer.Deserialize<KvDictionaryInfo>(val.ValueSpan, true);
                    ret.Name = name;
                }
            });

            return ret;
        }

        /// <summary>
        /// Returns information about all dictionaries in this instance of KvMultiDictionary.
        /// </summary>
        /// <returns>A list of dictionary information.</returns>
        public IList<KvDictionaryInfo> GetDictionaryInfos()
        {
            Perf.CallCount();

            var list = new List<KvDictionaryInfo>();

            DoInTransaction(() =>
            {
                using (var iter = _tx.GetIterator(null, true))
                {
                    while (iter.MoveNext())
                    {
                        var val = iter.Current.Value;
                        if (val.IsValid)
                        {
                            var key = iter.Current.Value.Key;
                            var name = DefaultSerializer.Deserialize<string>(key, false);
                            var di = DefaultSerializer.Deserialize<KvDictionaryInfo>(val.ValueSpan, true);
                            di.Name = name;
                            list.Add(di);
                        }
                    }
                }
            });

            return list;
        }

        /// <summary>
        /// Makes sure that the KvDictionary exists.
        /// If the dictionary exists it will be opened. The types of keys, values and serializer will be validated.
        /// If the dictionary does not exist it will be created with a default serializer of type KvJsonSerializer and default serializer options.
        /// </summary>
        /// <typeparam name="TKey">type of Key</typeparam>
        /// <typeparam name="TValue">type of Value</typeparam>
        /// <param name="name">the name of the dictionary</param>
        /// <returns>the requested KvDictionary</returns>
        public KvDictionary<TKey, TValue> EnsureDictionary<TKey, TValue>(string name)
        {
            ValidateName(name);

            return EnsureDictionary<TKey, TValue>(name, new KvJsonSerializer());
        }

        /// <summary>
        /// Makes sure that the KvDictionary exists.
        /// If the dictionary exists it will be opened. The types of keys, values and serializer will be validated.
        /// If the dictionary does not exist it will be created with a default serializer of type KvJsonSerializer and default serializer options.
        /// </summary>
        /// <typeparam name="TKey">type of Key</typeparam>
        /// <typeparam name="TValue">type of Value</typeparam>
        /// <param name="name">the name of the dictionary</param>
        /// <param name="serializer">the serializer to use.</param>
        /// <param name="force">if true the types of keys, values and serializer are not checked. Use only in case of emergency.</param>
        /// <returns>the requested KvDictionary</returns>
        public KvDictionary<TKey, TValue> EnsureDictionary<TKey, TValue>(string name, IKvSerializer serializer, bool force = false)
        {
            Perf.CallCount();

            ValidateName(name);

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer), "Serializer is null.");
            }

            var dict = GetDictionaryInfo(name);

            if (dict == null)
            {
                dict = new KvDictionaryInfo()
                {
                    Name = name,
                    KeyTypeName = typeof(TKey).FullName,
                    KeyTypeAssemblyName = typeof(TKey).Assembly.FullName,
                    ValueTypeName = typeof(TValue).FullName,
                    ValueTypeAssemblyName = typeof(TValue).Assembly.FullName,
                    SerializerTypeName = serializer.GetType().FullName,
                    SerializerTypeAssemblyName = serializer.GetType().Assembly.FullName,
                    SerializerOptionsTypeName = serializer.Options?.GetType().FullName,
                    SerializerOptionsTypeAssemblyName = serializer.Options?.GetType().Assembly.FullName,
                    SerializerOptions = serializer.Options
                };

                UpdateDictionaryInfo(name, dict, true);
            }
            else
            {
                if (!force)
                {
                    var kt = typeof(TKey).FullName;
                    if (dict.KeyTypeName != kt)
                    {
                        var msg = string.Format("Key type mismatch. Actual: '{0}' Expected: '{1}'", kt, dict.KeyTypeName);
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, msg);
                    }

                    var vt = typeof(TValue).FullName;
                    if (dict.ValueTypeName != vt)
                    {
                        var msg = string.Format("Value type mismatch. Actual: '{0}' Expected: '{1}'", vt, dict.ValueTypeName);
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, msg);
                    }

                    var st = serializer.GetType().FullName;
                    if (dict.SerializerTypeName != st)
                    {
                        var msg = string.Format("Serializer type mismatch. Actual: '{0}' Expected: '{1}'", st, dict.SerializerTypeName);
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, msg);
                    }

                    var sot = serializer.Options?.GetType().FullName;
                    if (dict.SerializerOptionsTypeName != sot)
                    {
                        var msg = string.Format("SerializerOptions type mismatch. Actual: '{0}' Expected: '{1}'", sot, dict.SerializerOptionsTypeName);
                        throw new KeyValiumException(ErrorCodes.InvalidParameter, msg);
                    }
                }

                // TODO set options on serializer
                //SerializerOptions = serializer.SerializerOptions

                var opttype = Type.GetType(dict.SerializerOptionsTypeName, ResolveAssembly, null);

                //JsonSerializer.Deserialize()

                Assembly ResolveAssembly(AssemblyName name)
                {
                    if (force)
                    {
                        name.Version = null;
                    }

                    return Assembly.Load(name);
                }

                serializer.SetOptions((JsonElement)dict.SerializerOptions);
            }

            return new KvDictionary<TKey, TValue>(this, name, dict, serializer);
        }

        /// <summary>
        /// Updates the dictionaries type information.
        /// </summary>
        /// <param name="name">Required. The name of the dictionary</param>
        /// <param name="keytype">Required. The key type. If null no changes are made.</param>
        /// <param name="valuetype">Required. The value type. If null no changes are made.</param>
        /// <exception cref="KeyValiumException"></exception>
        public void UpdateKeyValueTypes(string name, Type keytype, Type valuetype)
        {
            ValidateName(name);

            if (keytype == null)
            {
                throw new ArgumentNullException(nameof(keytype), "Key type is null.");
            }

            if (valuetype == null)
            {
                throw new ArgumentNullException(nameof(valuetype), "Value type is null.");
            }

            var dict = GetDictionaryInfo(name);
            if (dict == null)
            {
                throw new KeyValiumException(ErrorCodes.InvalidParameter, "Dictionary not found.");
            }

            dict.KeyTypeName = keytype.FullName;
            dict.KeyTypeAssemblyName = keytype.Assembly.FullName;

            dict.ValueTypeName = valuetype.FullName;
            dict.ValueTypeAssemblyName = valuetype.Assembly.FullName;

            UpdateDictionaryInfo(name, dict, false);
        }

        /// <summary>
        /// Updates the dictionaries serializer information.
        /// </summary>
        /// <param name="name">Required. The name of the dictionary</param>
        /// <param name="serializertype">Required. The serializer type.</param>
        /// <param name="optionstype">The serializer options type.</param>
        /// <param name="serializeroptions">The serializer options.</param>
        /// <exception cref="KeyValiumException"></exception>
        public void UpdateSerializer(string name, Type serializertype, Type optionstype, object serializeroptions)
        {
            ValidateName(name);

            if (serializertype == null)
            {
                throw new ArgumentNullException(nameof(serializertype), "Serializer type is null.");
            }

            var dict = GetDictionaryInfo(name);
            if (dict == null)
            {
                throw new KeyValiumException(ErrorCodes.InvalidParameter, "Dictionary not found.");
            }

            dict.SerializerTypeName = serializertype.FullName;
            dict.SerializerTypeAssemblyName = serializertype.Assembly.FullName;

            dict.SerializerOptionsTypeName = optionstype?.FullName;
            dict.SerializerOptionsTypeAssemblyName = optionstype?.Assembly.FullName;

            dict.SerializerOptions = serializeroptions;

            UpdateDictionaryInfo(name, dict, false);
        }

        /// <summary>
        /// Checks if the MultiDictionary is valid. Throws an exception if not valid
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
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

        private void UpdateCounts()
        {
            throw new NotImplementedException();
        }


        internal Transaction Tx
        {
            get
            {
                return _tx;
            }
        }

        internal bool EnsureTransaction()
        {
            if (_tx == null)
            {
                _tx = _db.BeginWriteTransaction();
                return true;
            }

            return false;
        }

        internal void CommitTransaction()
        {
            Perf.CallCount();

            _tx.Commit();
            _tx.Dispose();
            _tx = null;
        }

        internal void RollbackTransaction()
        {
            Perf.CallCount();

            _tx.Rollback();
            _tx.Dispose();
            _tx = null;
        }

        private void UpdateDictionaryInfo(string name, KvDictionaryInfo dict, bool create)
        {
            var key = DefaultSerializer.Serialize(name, false);

            DoInTransaction(() =>
            {
                if (create)
                {
                    using (var treeref = _tx.EnsureTreeRef(TrackingScope.TransactionChain, key))
                    {
                        _tx.Update(null, key, DefaultSerializer.Serialize(dict, true));
                    }
                }
                else
                {
                    _tx.Update(null, key, DefaultSerializer.Serialize(dict, true));
                }
            });
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "The dictionary name is invalid.");
            }
        }

        public bool DeleteDictionary(string name)
        {
            Perf.CallCount();

            ValidateName(name);

            var key = DefaultSerializer.Serialize(name, false);

            DoInTransaction(() =>
            {
                using (var treeref = _tx.GetTreeRef(TrackingScope.TransactionChain, key))
                {
                    if (treeref != null)
                    {
                        _tx.DeleteTree(treeref);
                    }
                }

                _tx.Delete(null, key);
            });

            return false;
        }


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
