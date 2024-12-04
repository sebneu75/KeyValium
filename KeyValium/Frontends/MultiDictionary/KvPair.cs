using System.Runtime.InteropServices;

namespace KeyValium.Frontends.MultiDictionary
{
    [StructLayout(LayoutKind.Auto)]
    internal struct KvPair<TKey, TValue>
    {
        internal KvPair(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }

        private TKey _key;

        private TValue _value;

        public TKey Key
        {
            get
            {
                return _key;
            }
        }

        public TValue Value
        {

            get
            {
                return _value;
            }
        }
    }
}
