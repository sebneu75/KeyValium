namespace KeyValium.Options
{
    public class ReadonlyDatabaseOptions
    {
        public ReadonlyDatabaseOptions(DatabaseOptions options)
        {
            Perf.CallCount();

            _options = options;
        }

        private readonly DatabaseOptions _options;

        /// <summary>
        /// Returns true if indexed access is enabled.
        /// </summary>
        public bool EnableIndexedAccess
        {
            get
            {
                return _options.EnableIndexedAccess;
            }
        }

        /// <summary>
        /// Returns the page size.
        /// </summary>
        public uint PageSize
        {
            get
            {
                Perf.CallCount();

                return _options.PageSize;
            }
        }

        /// <summary>
        /// Returns the version.
        /// </summary>
        internal ushort Version
        {
            get
            {
                Perf.CallCount();

                return _options.Version;
            }
        }

        /// <summary>
        /// True if the database should be created if it does not exist.
        /// </summary>
        public bool CreateIfNotExists
        {
            get
            {
                Perf.CallCount();

                return _options.CreateIfNotExists;
            }
        }

        /// <summary>
        /// internal type code
        /// </summary>
        internal uint InternalTypeCode
        {
            get
            {
                Perf.CallCount();

                return _options.InternalTypeCode;
            }
        }

        /// <summary>
        /// Returns the user defined type code.
        public uint UserTypeCode
        {
            get
            {
                Perf.CallCount();

                return _options.UserTypeCode;
            }
        }

        /// <summary>
        /// Returns the sharing mode for the database.
        /// </summary>
        public SharingModes SharingMode
        {
            get
            {
                Perf.CallCount();

                return _options.SharingMode;
            }
        }

        /// <summary>
        /// Returns the sharing mode for the database.
        /// </summary>
        internal InternalSharingModes InternalSharingMode
        {
            get
            {
                Perf.CallCount();

                return _options.InternalSharingMode;
            }
        }

        /// <summary>
        /// Returns the lock timeout in milliseconds.
        /// </summary>
        public int LockTimeout
        {
            get
            {
                Perf.CallCount();

                return _options.LockTimeout;
            }
        }

        /// <summary>
        /// Returns the lock interval in milliseconds.
        /// </summary>
        public int LockInterval
        {
            get
            {
                Perf.CallCount();

                return _options.LockInterval;
            }
        }

        /// <summary>
        /// Returns the lock interval variance in milliseconds.
        /// </summary>
        public int LockIntervalVariance
        {
            get
            {
                Perf.CallCount();

                return _options.LockIntervalVariance;
            }
        }

        /// <summary>
        /// Returns true if the database is readonly.
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                Perf.CallCount();

                return _options.ReadOnly;
            }
        }

        #region Validation

        /// <summary>
        /// Returns validation mode for pages.
        /// </summary>
        public PageValidationMode ValidationMode
        {
            get
            {
                return _options.ValidationMode;
            }
        }

        #endregion

        #region Security

        /// <summary>
        /// Returns the password.
        /// </summary>
        public string Password
        {
            get
            {
                Perf.CallCount();

                return _options.Password;
            }
        }

        /// <summary>
        /// Returns the key file.
        /// </summary>
        public string KeyFile
        {
            get
            {
                Perf.CallCount();

                return _options.KeyFile;
            }
        }

        /// <summary>
        /// Returns the encryption alogorithm.
        /// </summary>
        public EncryptionAlgorithms Algorithm
        {
            get
            {
                Perf.CallCount();

                return _options.Algorithm;
            }
        }

        /// <summary>
        /// Returns true if the database is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get
            {
                Perf.CallCount();

                return _options.IsEncrypted;
            }
        }

        #endregion

        #region Safety

        /// <summary>
        /// Returns true if data is flushed to disk after write.
        /// </summary>
        public bool FlushToDisk
        {
            get
            {
                Perf.CallCount();

                return _options.FlushToDisk;
            }

        }

        /// <summary>
        /// if true the memory of the page will be zeroed out before it is evicted from the cache
        /// </summary>
        internal bool ZeroPagesOnEvict
        {
            get
            {
                Perf.CallCount();

                return _options.ZeroPagesOnEvict;
            }
        }

        /// <summary>
        /// if true the freespace within a page will be zeroed out before writing to disk
        /// </summary>
        internal bool ZeroFreespace
        {
            get
            {
                Perf.CallCount();

                return _options.ZeroFreespace;
            }
        }

        /// <summary>
        /// if true the free pages that are no longer referenced will be zeroed out when
        /// a write transaction commits
        /// TODO mark freepages that are cleared (new flag)
        /// TODO do it in post commit phase
        /// </summary>
        internal bool ZeroFreePages
        {
            get
            {
                Perf.CallCount();

                return _options.ZeroFreePages;
            }
        }

        #endregion

        #region Memory

        /// <summary>
        /// Returns the cache size in megabytes.
        /// </summary>
        public int CacheSizeDatabaseMB
        {
            get
            {
                Perf.CallCount();

                return _options.CacheSizeMB;
            }
        }

        internal int CachedItems
        {
            get
            {
                Perf.CallCount();

                return _options.CachedItems;
            }
        }

        /// <summary>
        /// Returns the spill size in megabytes.
        /// </summary>
        public int SpillSizeMB
        {
            get
            {
                Perf.CallCount();

                return _options.SpillSizeMB;
            }
        }

        internal long SpillSize
        {
            get
            {
                Perf.CallCount();

                return _options.SpillSize;
            }
        }

        /// <summary>
        /// Returns the value spill size in megabytes.
        /// </summary>
        public int ValueSpillSizeMB
        {
            get
            {
                Perf.CallCount();

                return _options.ValueSpillSizeMB;
            }
        }

        internal long ValueSpillSize
        {
            get
            {
                Perf.CallCount();

                return _options.ValueSpillSize;
            }
        }

        internal DatabaseFlags Flags
        {
            get
            {
                return _options.Flags;
            }
        }

        #endregion
    }
}
