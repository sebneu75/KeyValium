using KeyValium.Frontends;

namespace KeyValium.Options
{
    /// <summary>
    /// Options for creating and opening a database.
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Constructor
        /// 
        /// Sets the default values
        /// </summary>
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
            SharingMode = SharingModes.Exclusive;
            LockTimeout = 60000;
            LockInterval = 40;
            LockIntervalVariance = 20;

            InternalTypeCode = 0;
            UserTypeCode = 0;

            // Validation
            ValidationMode = PageValidationMode.Default;

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
            CacheSizeMB = 16;
            SpillSizeMB = 16;
            ValueSpillSizeMB = 4;

            SetLogDefaults();
        }

        #region Options for Debugging

        internal LogLevel LogLevel 
        { 
            get; 
            set; 
        }

        internal LogTopics LogTopics
        {
            get;
            set;
        }

        [Conditional("DEBUG")]
        internal void SetLogDefaults()
        {
            LogLevel = LogLevel.All;

            //LogTopics.Lock | LogTopics.Transaction | LogTopics.Meta);
            //LogTopics.All);
            //LogTopics.Freespace | LogTopics.Allocation | LogTopics.Transaction);
            //LogTopics.Transaction | LogTopics.Validation | LogTopics.Insert);
            LogTopics = LogTopics.All & ~(LogTopics.Tracking | LogTopics.Allocation | LogTopics.Validation | LogTopics.Cursor);
        }

        #endregion

        #region Options for Creating a Database

        /// <summary>
        /// Enables or disables indexed access.
        /// </summary>
        internal bool EnableIndexedAccess
        {
            get;
            set;
        }

        /// <summary>
        ///  The page size in bytes. It must be a power of two in the range from 256 to 65536. Valid values are 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536.
        ///  The maximum key size depends on the page size. Values smaller than 4096 are not recommended for general use.
        ///  Default page size is 4096.
        ///  Has no effect on existing databases.
        /// </summary>
        public uint PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Version (only 1 is allowed)
        ///  Has no effect on existing databases.
        /// </summary>
        internal ushort Version
        {
            get;
            set;
        }

        /// <summary>
        /// True if the database should be created if it does not exist.
        /// Default is true.
        /// </summary>
        public bool CreateIfNotExists
        {
            get;
            set;
        }

        /// <summary>
        /// internal type code
        /// Has no effect on existing databases.
        /// </summary>
        internal uint InternalTypeCode
        {
            get;
            set;
        }

        /// <summary>
        /// A user defined type code.
        /// Default is 0.
        /// Has no effect on existing databases.
        /// </summary>
        public uint UserTypeCode
        {
            get;
            set;
        }

        #endregion

        #region Options for Opening a Database

        /// <summary>
        /// The sharing mode for the database.
        /// Default is SharingModes.None.
        /// </summary>
        public SharingModes SharingMode
        {
            get
            {
                return (SharingModes)InternalSharingMode;
            }
            set
            {
                InternalSharingMode = (InternalSharingModes)value;
            }
        }

        /// <summary>
        /// The sharing mode for the database.
        /// Default is SharingModes.None.
        /// </summary>
        internal InternalSharingModes InternalSharingMode
        {
            get;
            set;
        }

        /// <summary>
        /// The timeout in milliseconds for acquiring a lock.
        /// Only used when SharingMode is not None.
        /// Default is 60000.
        /// </summary>
        public int LockTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// The interval in milliseconds between attempts to acquire the lock.
        /// Only used when SharingMode is Shared.
        /// Default is 40.
        /// </summary>
        public int LockInterval
        {
            get;
            set;
        }

        /// <summary>
        /// The variance in milliseconds of the LockInterval. A random value in the range from 0 to LockIntervalVariance will be added to LockInterval.
        /// Only used when SharingMode is Shared.
        /// Default is 20.
        /// </summary>
        public int LockIntervalVariance
        {
            get;
            set;
        }

        /// <summary>
        /// If true the database will be opened in readonly mode.
        /// Default is false.
        /// </summary>
        public bool ReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// If true the cache will be filled after opening the database by walking the tree breadth first.
        /// </summary>
        public bool FillCache
        {
            get;
            set;
        }

        #endregion

        #region Validation

        /// <summary>
        /// The validation mode for pages.
        /// Pages can be validated when they are read and/or written from/to disk.
        /// Default is ValidationMode.Default.
        /// </summary>
        public PageValidationMode ValidationMode
        {
            get;
            set;
        }

        #endregion

        #region Security

        /// <summary>
        /// The password used for encryption of the database. Can be combined with a key file.
        /// Encryption is enabled when a password or a keyfile has been set.
        /// Default is null.
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// The key file used for encryption of the database. Can be combined with a password.
        /// If the keyfile is smaller than 8 bytes it will be padded with zeroes up to a length of 8 bytes.
        /// Up to 1 MB of the key file will be used to derive the encryption key.
        /// Encryption is enabled when a password or a keyfile has been set.
        /// Default is null.
        /// </summary>
        public string KeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// The algorithm used for encryption. Currently only one (EncryptionAlgorithms.AesMd5) is available.
        /// The actual key is derived via Rfc2898DeriveBytes from the password and the key file (salt) using 16384 
        /// iterations with SHA256 as the hash algorithm.
        /// To derive the initialization vector for a given page the actual key is combined with the page number 
        /// via MD5 hash algorithm.
        /// </summary>
        public EncryptionAlgorithms Algorithm
        {
            get;
            set;
        }

        /// <summary>
        /// returns true if a password or a keyfile has been set.
        /// </summary>
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

        /// <summary>
        /// If true data will be flushed to disk on write. This is used as parameter to FileStream.Flush(Boolean).
        /// </summary>
        public bool FlushToDisk
        {
            get;
            set;
        }

        /// <summary>
        /// if true the memory of the page will be zeroed out before it is evicted from the cache
        /// </summary>
        internal bool ZeroPagesOnEvict
        {
            get;
            set;
        }

        /// <summary>
        /// if true the freespace within a page will be zeroed out before writing to disk
        /// </summary>
        internal bool ZeroFreespace
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
        internal bool ZeroFreePages
        {
            get;
            set;
        }

        #endregion

        #region Memory

        /// <summary>
        /// The cache size in megabytes. If set to 0 or less than zero caching is disabled.
        /// </summary>
        public int CacheSizeMB
        {
            get;
            set;
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

                var size = (long)CacheSizeMB * 1024 * 1024;
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

        /// <summary>
        /// The spill size in megabytes. If the in memory size of the transaction greater then dirty pages will be written to disk.
        /// </summary>
        public int SpillSizeMB
        {
            get;
            set;
        }

        internal long SpillSize
        {
            get
            {
                Perf.CallCount();

                return SpillSizeMB * 1024 * 1024;
            }
        }

        /// <summary>
        /// The value spill size in megabytes. Values greater than this will be written directly to disk.
        /// </summary>
        public int ValueSpillSizeMB
        {
            get;
            set;
        }

        internal long ValueSpillSize
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
            ret.CacheSizeMB = CacheSizeMB;
            ret.CreateIfNotExists = CreateIfNotExists;
            ret.EnableIndexedAccess = EnableIndexedAccess;
            ret.FlushToDisk = FlushToDisk;
            ret.InternalTypeCode = InternalTypeCode;
            ret.KeyFile = KeyFile;
            ret.LockInterval = LockInterval;
            ret.LockIntervalVariance = LockIntervalVariance;
            ret.LockTimeout = LockTimeout;
            ret.PageSize = PageSize;
            ret.Password = Password;
            ret.ReadOnly = ReadOnly;
            ret.InternalSharingMode = InternalSharingMode;
            ret.SpillSizeMB = SpillSizeMB;
            ret.UserTypeCode = UserTypeCode;
            ret.ValidationMode = ValidationMode;
            ret.ValueSpillSizeMB = ValueSpillSizeMB;
            ret.Version = Version;
            ret.ZeroFreePages = ZeroFreePages;
            ret.ZeroFreespace = ZeroFreespace;
            ret.ZeroPagesOnEvict = ZeroPagesOnEvict;

            return ret;
        }

        internal void Validate()
        {
            if (InternalSharingMode == InternalSharingModes.SharedNetwork)
            {
                // SharedNetwork is only available on Windows
                // SharedNetwork requires a page size of at least 4096

                var msgs = new List<string>();

                if (PageSize < 4096)
                {
                    msgs.Add("Page size must be at least 4096 for sharing mode SharedNetwork.");
                }

                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    msgs.Add("Sharing mode SharedNetwork is only available on Windows.");
                }

                if (msgs.Count > 0)
                {
                    throw new NotSupportedException(string.Join("\n", msgs.ToArray()));
                }
            }
        }

        #endregion
    }
}
