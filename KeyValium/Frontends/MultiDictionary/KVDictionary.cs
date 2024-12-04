using KeyValium.Frontends.Serializers;
using KeyValium.Iterators;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace KeyValium.Frontends.MultiDictionary
{
    /// <summary>
    /// A persistent dictionary of TKey and TValue.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
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

        private TreeRef _dictref;

        /// <summary>
        /// Returns the name of the KvDictionary
        /// </summary>
        public readonly string Name;

        private readonly byte[] SerializedName;

        /// <summary>
        /// Returns the meta data of this dictionary.
        /// </summary>
        public readonly KvDictionaryInfo Info;

        private readonly IKvSerializer Serializer;

        private readonly KvMultiDictionary Parent;

        internal readonly bool _isreadonly;

        #endregion

        /// <summary>
        /// Does an action within a transaction. Calls can be nested. No nested transactions are used.
        /// If no transaction exists one is started. 
        /// If the call created a transaction it is rolled back if action throws an exception. Otherwise it is committed.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="appendmode">Enables append mode if true. This can be used when inserting multiple keys in order to save disk space.</param>
        public void Do(Action action, bool appendmode = false)
        {
            Validate();

            Parent.Do(action, appendmode);
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

        /// <summary>
        /// Returns or sets the value associated with a key.
        /// If the getter is called with a key that does not exist in the database an exception is thrown.
        /// If the setter is called with a nonexisting key it is inserted into the database.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
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

                Do(() =>
                {
                    Parent.Tx.Upsert(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false), Serializer.Serialize(value, true));

                });
            }
        }

        /// <summary>
        /// Returns the collection of keys.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                Perf.CallCount();

                Validate();

                return new KvKeyCollection(this);
            }
        }

        /// <summary>
        /// Returns the collection of values.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                Perf.CallCount();

                Validate();

                return new KvValueCollection(this);
            }
        }

        /// <summary>
        /// Returns the collection of keys.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        /// <summary>
        /// Returns the collection of values.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        /// <summary>
        /// Returns the number of key value pairs in this dictionary.
        /// If this dictionary contains more than Array.MaxLength pairs an exception is thrown.
        /// LINQ methods (for example mydict.Keys.ToList()) will no longer work if this is the case.
        /// Use <see cref="LongCount"/> in this case.
        /// </summary>
        public int Count
        {
            get
            {
                var ret = LongCount;
                if (ret > (ulong)Array.MaxLength)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Key count is greater than Array.MaxLength.");
                }

                return (int)ret;
            }
        }

        /// <summary>
        /// Returns the number of key value pairs in this dictionary. Use this if your dictionary contains
        /// more than Array.MaxLength entries.
        /// </summary>
        public ulong LongCount
        {
            get
            {
                Perf.CallCount();
                Validate();

                ulong ret = 0;

                Do(() =>
                {
                    ret = Parent.Tx.GetLocalCount(GetDictionaryRef(Parent.Tx));
                });

                return ret;
            }
        }

        /// <summary>
        /// Returns true if the dictionary is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return _isreadonly;
            }
        }

        /// <summary>
        /// Inserts a key value pair into the dictionary. If the key already exists an exception is thrown.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The associated value.</param>
        public void Add(TKey key, TValue value)
        {
            Perf.CallCount();

            Validate();

            Do(() =>
            {
                Parent.Tx.Insert(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false), Serializer.Serialize(value, true));
            });
        }

        /// <summary>
        /// Inserts a key value pair into the dictionary. If the key already exists an exception is thrown.
        /// </summary>
        /// <param name="item">The key value pair.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears the dictionary.
        /// </summary>
        public void Clear()
        {
            Perf.CallCount();

            Validate();

            Do(() =>
            {
                Parent.Tx.DeleteTree(GetDictionaryRef(Parent.Tx));
            });
        }

        /// <summary>
        /// Checks if the dictionary contains the key value pair.
        /// </summary>
        /// <param name="item">The key value pair.</param>
        /// <returns>True if this dictionary contains item. Otherwise false.</returns>
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

        /// <summary>
        /// Checks if the dictionary contains the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if this dictionary contains the key. Otherwise false.</returns>
        public bool ContainsKey(TKey key)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            Do(() =>
            {
                ret = Parent.Tx.Exists(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false));
            });

            return ret;
        }

        /// <summary>
        /// Checks if the dictionary contains the value. 
        /// This is an expensive operation. Use only in cases of emergency.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if this dictionary contains the value. Otherwise false.</returns>
        public bool ContainsValue(TValue value)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            Do(() =>
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

        /// <summary>
        /// Copies the key value pairs to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The offset into the array.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// Returns an enumerator for this dictionary.
        /// </summary>
        /// <returns>the enumerator</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Perf.CallCount();

            Validate();

            return new DictionaryEnumerator(this);
        }

        /// <summary>
        /// Returns an enumerator for this dictionary.
        /// </summary>
        /// <returns>the enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Deletes a key and an associated value from the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the key has been removed. Otherwise false.</returns>
        public bool Remove(TKey key)
        {
            Perf.CallCount();

            Validate();

            var ret = false;

            Do(() =>
            {
                ret = Parent.Tx.Delete(GetDictionaryRef(Parent.Tx), Serializer.Serialize(key, false));
            });

            return ret;
        }

        /// <summary>
        /// Deletes a key value pair from the dictionary.
        /// </summary>
        /// <param name="item">The key value pair.</param>
        /// <returns>True if the item has been removed. Otherwise false.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Tries to get the value associated with key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The associated value. Only valid if the method returned true.</param>
        /// <returns>True if the value has been found. Otherwise false.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            Perf.CallCount();

            Validate();

            TValue retval = default;
            bool isvalid = false;

            Do(() =>
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

        #endregion

        private void Validate()
        {
            if (_isdisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private bool _isdisposed;

        /// <summary>
        /// Disposes this dictionary.
        /// </summary>
        public void Dispose()
        {
            //Parent.RemoveDictionary(this);
            _dictref?.Dispose();
            _isdisposed = true;
        }

        #region IEnumerable implementation

        #endregion

        #region Enumerator support

        /// <summary>
        /// A collection of keys.
        /// </summary>
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

        /// <summary>
        /// A collection of values.
        /// </summary>
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

        /// <summary>
        /// An enumerator for a KvDictionary. Must be called within an transaction.
        /// </summary>
        public class DictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            internal DictionaryEnumerator(KvDictionary<TKey, TValue> dict)
            {
                _dict = dict;

                _iterator = _dict.Parent.Tx.GetIterator(dict.GetDictionaryRef(_dict.Parent.Tx), true);
            }

            private KvDictionary<TKey, TValue> _dict;

            private KeyIterator _iterator;

            internal TKey GetCurrentKey()
            {
                var valref = _iterator.Current.Value;

                return _dict.Serializer.Deserialize<TKey>(valref.Key, false);
            }

            internal TValue GetCurrentValue()
            {
                var valref = _iterator.Current.Value;

                return _dict.Serializer.Deserialize<TValue>(valref.ValueSpan, true);
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
                    return Current;
                }
            }

            public bool MoveNext()
            {
                return _iterator.MoveNext();
            }

            public void Reset()
            {
                _iterator.Reset();
            }

            public void Dispose()
            {
                _iterator.Dispose();
            }
        }

        #endregion
    }
}
