using KeyValium.Collections;
using KeyValium.Cursors;
using KeyValium.Frontends.Serializers;
using KeyValium.Iterators;
using KeyValium.Logging;
using KeyValium.Pages.Entries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends
{
    public class KvDictionary<TKey, TValue> :
        //IDictionary,
        IDictionary<TKey, TValue>,
        //ICollection,
        ICollection<KeyValuePair<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDisposable
    {
        internal KvDictionary(KvMultiDictionary md, string name, KvDictionaryInfo info, IKvSerializer serializer)
        {
            Perf.CallCount();

            Parent = md;
            Name = name;
            Info = info;

            _isreadonly = false;

            Serializer = serializer;

            SerializedName = Parent.DefaultSerializer.Serialize(name, false);
        }

        #region Variables

        public readonly bool _isreadonly;

        private readonly IKvSerializer Serializer;

        private readonly KvMultiDictionary Parent;

        public readonly string Name;

        private readonly byte[] SerializedName;

        public readonly KvDictionaryInfo Info;

        private TreeRef _dictref;

        #endregion

        public void DoInTransaction(Action action, bool appendmode = false)
        {
            Validate();

            Parent.DoInTransaction(action, appendmode);
        }

        internal TreeRef GetDictionaryRef(Transaction tx)
        {
            Perf.CallCount();

            Validate();

            if (_dictref == null)
            {
                _dictref = tx.GetTreeRef(TrackingScope.Database, SerializedName);
            }

            return _dictref;
        }

        #region IDictionary implementation

        public TValue this[TKey key]
        {
            get
            {
                Perf.CallCount();

                Validate();

                if (TryGetValue(key, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Perf.CallCount();

                Validate();

                DoInTransaction(() =>
                {
                    Parent.Tx.Upsert(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false), Serializer.Serialize(value, true));

                });
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                Perf.CallCount();

                Validate();

                return new KvKeyCollection(this);
            }
        }


        public ICollection<TValue> Values
        {
            get
            {
                Perf.CallCount();

                Validate();

                return new KvValueCollection(this);
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;


        public int Count
        {
            get
            {
                var ret = LongCount;
                if (ret > int.MaxValue)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Key count is greater than int.MaxValue.");
                }

                return (int)ret;
            }
        }

        public ulong LongCount
        {
            get
            {
                Perf.CallCount();
                Validate();

                ulong ret = 0;

                DoInTransaction(() =>
                {
                    ret = Parent.Tx.GetLocalCount(GetDictionaryRef(Parent.Tx));
                });

                return ret;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _isreadonly;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Perf.CallCount();

            Validate();

            DoInTransaction(() =>
            {
                Parent.Tx.Insert(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false), Serializer.Serialize(value, true));
            });
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Perf.CallCount();

            Validate();

            DoInTransaction(() =>
            {
                Parent.Tx.DeleteTree(GetDictionaryRef(Parent.Tx));
            });
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!ContainsKey(item.Key))
            {
                return false;
            }

            var comp = EqualityComparer<TValue>.Default;
            var val = this[item.Key];           

            return comp.Equals(val, item.Value);
        }

        public bool ContainsKey(TKey key)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            DoInTransaction(() =>
            {
                ret = Parent.Tx.Exists(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false));
            });

            return ret;
        }

        public bool ContainsValue(TValue value)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            DoInTransaction(() =>
            {
                var comp = EqualityComparer<TValue>.Default;

                foreach (var item in Parent.Tx.GetIterator(GetDictionaryRef(Parent.Tx), true))
                {
                    var val = Serializer.Deserialize<TValue>(item.Value.ValueSpan, true);
                    if (comp.Equals(val, value))
                    {
                        ret = true;
                        break;
                    }
                }
            });

            return ret;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if ((uint)index > (uint)array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            foreach (var item in this)
            {
                array[index++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Perf.CallCount();

            Validate();

            return new DictionaryEnumerator(this);
        }

        public bool Remove(TKey key)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            DoInTransaction(() =>
            {
                ret = Parent.Tx.Delete(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false));
            });

            return ret;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            Perf.CallCount();

            Validate();

            TValue retval = default;
            bool isvalid = false;

            DoInTransaction(() =>
            {
                var val = Parent.Tx.Get(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false));
                if (val.IsValid)
                {
                    retval = Serializer.Deserialize<TValue>(val.ValueSpan, true);
                    isvalid = true;
                }
            });

            value = retval;
            return isvalid;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void Validate()
        {
            if (_isdisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private bool _isdisposed;

        public void Dispose()
        {
            //Parent.RemoveDictionary(this);
            _dictref?.Dispose();
            _isdisposed = true;
        }

        #region IEnumerable implementation

        #endregion

        #region Enumerator support

        public sealed class KvKeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            public KvKeyCollection(KvDictionary<TKey, TValue> dict)
            {
                _dict = dict;
            }

            private readonly KvDictionary<TKey, TValue> _dict;

            public int Count => _dict.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => _dict;

            public bool Contains(TKey item)
            {
                return _dict.ContainsKey(item);
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return new KeyEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public sealed class KeyEnumerator : IEnumerator<TKey>, IEnumerator
            {
                internal KeyEnumerator(KvDictionary<TKey, TValue> dict)
                {
                    _enumerator = new DictionaryEnumerator(dict);
                }

                private DictionaryEnumerator _enumerator;

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public TKey Current
                {
                    get
                    {
                        return _enumerator.GetCurrentKey();
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }

                void IEnumerator.Reset()
                {
                    _enumerator.Reset();
                }

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }

        public sealed class KvValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            public KvValueCollection(KvDictionary<TKey, TValue> dict)
            {
                _dict = dict;
            }

            private readonly KvDictionary<TKey, TValue> _dict;

            public int Count => _dict.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => _dict;

            public bool Contains(TValue item)
            {
                return _dict.ContainsValue(item);
            }

            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new ValueEnumerator(_dict);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public sealed class ValueEnumerator : IEnumerator<TValue>, IEnumerator
            {
                internal ValueEnumerator(KvDictionary<TKey, TValue> dict)
                {
                    _enumerator = new DictionaryEnumerator(dict);
                }

                private DictionaryEnumerator _enumerator;

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public TValue Current
                {
                    get
                    {
                        return _enumerator.GetCurrentValue();
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }

                void IEnumerator.Reset()
                {
                    _enumerator.Reset();
                }

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }

        public class DictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            internal DictionaryEnumerator(KvDictionary<TKey, TValue> dict)
            {
                _dict = dict;

                Monitor.Enter(_dict.Parent._mdlock);
                _created = _dict.Parent.EnsureTransaction();
                _iterator = _dict.Parent.Tx.GetIterator(dict.GetDictionaryRef(_dict.Parent.Tx), true);
            }

            private KvDictionary<TKey, TValue> _dict;
            private bool _created;
            private KeyIterator _iterator;
            private bool _failed;

            internal TKey GetCurrentKey()
            {
                try
                {
                    var valref = _iterator.Current.Value;

                    return _dict.Serializer.Deserialize<TKey>(valref.Key, false);
                }
                catch (Exception)
                {
                    _failed = true;
                    throw;
                }
            }

            internal TValue GetCurrentValue()
            {
                try
                {
                    var valref = _iterator.Current.Value;

                    return _dict.Serializer.Deserialize<TValue>(valref.ValueSpan, true);
                }
                catch (Exception)
                {
                    _failed = true;
                    throw;
                }
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(GetCurrentKey(), GetCurrentValue());
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    try
                    {
                        return Current;
                    }
                    catch (Exception)
                    {
                        _failed = true;
                        throw;
                    }
                }
            }

            public bool MoveNext()
            {
                try
                {
                    return _iterator.MoveNext();
                }
                catch (Exception)
                {
                    _failed = true;
                    throw;
                }
            }

            public void Reset()
            {
                try
                {
                    _iterator.Reset();
                }
                catch (Exception)
                {
                    _failed = true;
                    throw;
                }
            }

            public void Dispose()
            {
                try
                {
                    _iterator.Dispose();
                    if (_created)
                    {
                        if (_failed)
                        {
                            _dict.Parent.Rollback();
                        }
                        else
                        {
                            _dict.Parent.Commit();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_dict.Parent._mdlock);
                }
            }
        }

        #endregion
    }
}
