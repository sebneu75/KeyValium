using KeyValium.Options;
using System.ComponentModel;

namespace KeyValium.Inspector
{
    public class DatabaseProperties
    {
        internal DatabaseProperties()
        {
            MetaInfos = new List<MetaInfo>();
        }

        [Category("File")]
        [Description("The filename.")]        
        public string Filename
        {
            get;
            internal set;
        }

        [Category("File")]
        [Description("The file size in bytes.")]
        public long FileSize
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The maximum key size in bytes.")]

        public ushort MaxKeySize
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The maximum key and value sizes in bytes that can be stored inline without making use of overflow pages.")]
        public ushort MaxKeyAndValueSize
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The page size in bytes.")]
        public uint PageSize
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The database flags.")]
        public ushort Flags
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The number of pages in the database.")]
        public long PageCount
        {
            get
            {
                return FileSize / PageSize;
            }
        }

        [Category("Database")]
        [Description("The internal type code of the database.")]
        public uint InternalTypecode
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The user type code of the database.")]
        public uint UserTypecode
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The file format version.")]
        public ushort Version
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The page number of the first meta page.")]
        public KvPagenumber FirstMetaPage
        {
            get;
            internal set;
        }

        [Category("Database")]
        [Description("The page number of the first data page.")]
        public KvPagenumber FirstDataPage
        {
            get
            {
                return FirstMetaPage + MetaPages;
            }
        }

        [Category("Database")]
        [Description("The number of meta pages the database uses.")]
        public ushort MetaPages
        {
            get;
            internal set;
        }

        [Browsable(false)]
        public ushort MinKeysPerIndexPage
        {
            get;
            internal set;
        }

        [Browsable(false)]
        public ushort MinKeysPerLeafPage
        {
            get;
            internal set;
        }

        [Browsable(false)]  
        public bool SwapEndianess
        {
            get;
            internal set;
        }

        [Browsable(false)]
        public List<MetaInfo> MetaInfos
        {
            get;
            private set;
        }

    }
}
