
using KeyValium.Encryption;

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

        public bool EnableIndexedAccess
        {
            get
            {
                return _options.EnableIndexedAccess;
            }
        }

        /// <summary>
        ///  Pagesize, must be a power of two
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
        /// Version (only 1 is allowed)
        /// </summary>
        public ushort Version
        {
            get
            {
                Perf.CallCount();

                return _options.Version;
            }
        }

        /// <summary>
        /// true if the database should be created if itr does not exist
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
        public uint InternalTypeCode
        {
            get
            {
                Perf.CallCount();

                return _options.InternalTypeCode;
            }
        }

        /// <summary>
        /// user type code
        /// </summary>
        public uint UserTypeCode
        {
            get
            {
                Perf.CallCount();

                return _options.UserTypeCode;
            }
        }

        public ByteOrder ByteOrder
        {
            get
            {
                Perf.CallCount();

                return _options.ByteOrder;
            }
        }

        public bool Shared
        {
            get
            {
                Perf.CallCount();

                return _options.Shared;
            }
        }

        public bool ReadOnly
        {
            get
            {
                Perf.CallCount();

                return _options.ReadOnly;
            }
        }

        public bool PreviousSnapshot
        {
            get
            {
                Perf.CallCount();

                return _options.PreviousSnapshot;
            }
        }

        #region Security

        public string Password
        {
            get
            {
                Perf.CallCount();

                return _options.Password;
            }
        }

        public string KeyFile
        {
            get
            {
                Perf.CallCount();

                return _options.KeyFile;
            }
        }

        public EncryptionAlgorithms Algorithm
        {
            get
            {
                Perf.CallCount();

                return _options.Algorithm;
            }
        }

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
        public bool ZeroPagesOnEvict
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
        public bool ZeroFreespace
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
        public bool ZeroFreePages
        {
            get
            {
                Perf.CallCount();

                return _options.ZeroFreePages;
            }
        }

        #endregion

        #region Memory

        public int CacheSizeDatabaseMB
        {
            get
            {
                Perf.CallCount();

                return _options.CacheSizeDatabaseMB;
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

        public int SpillSizeMB
        {
            get
            {
                Perf.CallCount();

                return _options.SpillSizeMB;
            }
        }

        public long SpillSize
        {
            get
            {
                Perf.CallCount();

                return _options.SpillSize;
            }
        }

        public int ValueSpillSizeMB
        {
            get
            {
                Perf.CallCount();

                return _options.ValueSpillSizeMB;
            }
        }

        public long ValueSpillSize
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
