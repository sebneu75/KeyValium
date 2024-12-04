namespace KeyValium.Frontends.TreeArray
{
    public class KvArrayKey : IEquatable<KvArrayKey>
    {
        #region Constructors

        /// <summary>
        /// Creates an KvArrayKey of kind long.
        /// </summary>
        /// <param name="val">The long value. Must be zero or positive.</param>
        /// <exception cref="ArgumentException"></exception>
        public KvArrayKey(long val)
        {
            if (val < 0)
            {
                throw new ArgumentException("Value cannot be negative.");
            }

            _longvalue = val;
            Type = KvArrayTypes.Long;
        }

        /// <summary>
        /// Creates an KvArrayKey of kind string.
        /// </summary>
        /// <param name="val">The string value. Must be non empty.</param>
        /// <exception cref="ArgumentException"></exception>
        public KvArrayKey(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                throw new ArgumentException("Value cannot be null or empty.");
            }

            _stringvalue = val;
            Type = KvArrayTypes.String;
        }

        #endregion

        #region Variables

        public readonly KvArrayFlags Flags = KvArrayFlags.None; // Always None for now

        public readonly KvArrayTypes Type;

        private readonly long _longvalue;

        private readonly string _stringvalue;

        #endregion

        #region Operators

        /// <summary>
        /// implicit conversion from long to KvArrayKey
        /// </summary>
        /// <param name="val">the long value</param>
        public static implicit operator KvArrayKey(long val) => new KvArrayKey(val);

        /// <summary>
        /// implicit conversion from string to KvArrayKey
        /// </summary>
        /// <param name="val">the string value</param>
        public static implicit operator KvArrayKey(string val) => new KvArrayKey(val);

        #endregion

        #region Properties

        public long LongValue
        {
            get
            {
                if (Type != KvArrayTypes.Long)
                {
                    throw new InvalidOperationException("KvKey has no long value.");
                }

                return _longvalue;
            }
        }

        public string StringValue
        {
            get
            {
                if (Type != KvArrayTypes.String)
                {
                    throw new InvalidOperationException("KvKey has no string value.");
                }

                return _stringvalue;
            }
        }

        #endregion

        #region IEquatable

        public bool Equals(KvArrayKey other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            if (Type != other.Type)
            {
                return false;
            }

            if (Type == KvArrayTypes.String && this.StringValue != other.StringValue)
            {
                return false;
            }
            else if (Type == KvArrayTypes.Long && this.LongValue != other.LongValue)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
