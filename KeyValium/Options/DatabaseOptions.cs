using KeyValium.Encryption;
using KeyValium.Frontends;

namespace KeyValium.Options
{
    public class DatabaseOptions
    {
        public DatabaseOptions()
        {
            Perf.CallCount();

            // Create
            PageSize = 4096;
            CreateIfNotExists = true;
            Version = 1;
            EnableIndexedAccess = false;

            // Open
            ReadOnly = false;
            Shared = false;
            PreviousSnapshot = false;
            InternalTypeCode = 0;
            UserTypeCode = 0;

            // Security
            Password = null;
            KeyFile = null;
            Algorithm = EncryptionAlgorithms.AesMd5;

            // Safety
            FlushToDisk = true;
            ZeroFreePages = false;
            ZeroFreespace = false;
            ZeroPagesOnEvict = false;

            // Memory
            CacheSizeDatabaseMB = 16;
            SpillSizeMB = 16;
            ValueSpillSizeMB = 4;
        }

        public bool EnableIndexedAccess
        {
            get;
            internal set;
        }

        #region Options for Creating a Database

        /// <summary>
        ///  Pagesize, must be a power of two
        /// </summary>
        public uint PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Version (only 1 is allowed)
        /// </summary>
        public ushort Version
        {
            get;
            set;
        }

        /// <summary>
        /// true if the database should be created if it does not exist
        /// </summary>
        public bool CreateIfNotExists
        {
            get;
            set;
        }

        /// <summary>
        /// internal type code
        /// </summary>
        public uint InternalTypeCode
        {
            get;
            internal set;
        }

        /// <summary>
        /// user type code
        /// </summary>
        public uint UserTypeCode
        {
            get;
            set;
        }

        public ByteOrder ByteOrder
        {
            get
            {
                return ByteOrder.LittleEndian;
            }
        }

        #endregion

        #region Options for Opening a Database

        public bool Shared
        {
            get;
            set;
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        public bool PreviousSnapshot
        {
            get;
            set;
        }

        #endregion

        #region Security

        public string Password
        {
            get;
            set;
        }

        public string KeyFile
        {
            get;
            set;
        }

        public EncryptionAlgorithms Algorithm
        {
            get;
            set;
        }

        public bool IsEncrypted
        {
            get
            {
                Perf.CallCount();

                return KeyFile != null || Password != null;
            }
        }

        #endregion

        #region Safety

        public bool FlushToDisk
        {
            get;
            set;
        }

        /// <summary>
        /// if true the memory of the page will be zeroed out before it is evicted from the cache
        /// </summary>
        public bool ZeroPagesOnEvict
        {
            get;
            set;
        }

        /// <summary>
        /// if true the freespace within a page will be zeroed out before writing to disk
        /// </summary>
        public bool ZeroFreespace
        {
            get;
            set;
        }

        /// <summary>
        /// if true the free pages that are no longer referenced will be zeroed out when
        /// a write transaction commits
        /// TODO mark freepages that are cleared (new flag)
        /// TODO do it in post commit phase
        /// </summary>
        public bool ZeroFreePages
        {
            get;
            set;
        }

        #endregion

        #region Memory

        private int _cachesize;

        public int CacheSizeDatabaseMB
        {
            get
            {
                Perf.CallCount();

                return _cachesize;
            }
            set
            {
                Perf.CallCount();

                _cachesize = value;
            }
        }

        const int MaxCachedItems = 1 << 30;

        /// <summary>
        /// returns the number of items in the cache
        /// depends on CacheSizeDatabaseMB and PageSize
        /// </summary>
        internal int CachedItems
        {
            get
            {
                Perf.CallCount();

                var size = (long)CacheSizeDatabaseMB * 1024 * 1024;
                if (size <= 0)
                {
                    return 0;
                }

                var items = size / PageSize;
                if (items > MaxCachedItems)
                {
                    items = MaxCachedItems;
                }
                else if (items < 0)
                {
                    items = 0;
                }

                return (int)items;
            }
        }

        public int SpillSizeMB
        {
            get;
            set;
        }

        public long SpillSize
        {
            get
            {
                Perf.CallCount();

                return SpillSizeMB * 1024 * 1024;
            }
        }

        public int ValueSpillSizeMB
        {
            get;
            set;
        }

        public long ValueSpillSize
        {
            get
            {
                Perf.CallCount();

                return ValueSpillSizeMB * 1024 * 1024;
            }
        }

        internal DatabaseFlags Flags
        {
            get
            {
                var ret = DatabaseFlags.None;

                if (EnableIndexedAccess)
                {
                    ret |= DatabaseFlags.IndexedAccess;
                }

                return ret;
            }
        }

        internal DatabaseOptions Copy()
        {
            Perf.CallCount();

            var ret = new DatabaseOptions();

            ret.Algorithm = Algorithm;
            ret.CacheSizeDatabaseMB = CacheSizeDatabaseMB;
            ret.CreateIfNotExists = CreateIfNotExists;
            ret.FlushToDisk = FlushToDisk;
            ret.InternalTypeCode = InternalTypeCode;
            ret.KeyFile = KeyFile;
            ret.PageSize = PageSize;
            ret.Password = Password;
            ret.PreviousSnapshot = PreviousSnapshot;
            ret.ReadOnly = ReadOnly;
            ret.Shared = Shared;
            ret.SpillSizeMB = SpillSizeMB;
            ret.UserTypeCode = UserTypeCode;
            ret.ValueSpillSizeMB = ValueSpillSizeMB;
            ret.Version = Version;
            ret.ZeroFreePages = ZeroFreePages;
            ret.ZeroFreespace = ZeroFreespace;
            ret.ZeroPagesOnEvict = ZeroPagesOnEvict;

            return ret;
        }

        #endregion
    }
}
