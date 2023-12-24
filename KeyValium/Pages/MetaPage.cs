using KeyValium.Pages.Headers;

namespace KeyValium.Pages
{
    internal unsafe sealed class MetaPage
    {
        #region Constructor

        internal MetaPage(AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page != null, "page cannot be null.");

            Page = page;
            Header = Page.Header;
            Content = Page.Bytes.Slice(UniversalHeader.HeaderSize);
        }

        #endregion

        #region Properties

        internal readonly AnyPage Page;

        internal UniversalHeader Header;

        internal readonly ByteSpan Content;

        #endregion

        internal void InitPage()
        {
            Perf.CallCount();

            Tid = 0;
            HeaderTid = 0;
            FooterTid = 0;            
            FsRootPage = 0;
            DataRootPage = 0;
            LastPage = 0;
            DataTotalCount = 0;
            DataLocalCount = 0;
            FsTotalCount = 0;
            FsLocalCount = 0;
        }

        #region Header

        /// <summary>
        /// Header Transaction Id
        /// </summary>
        internal KvTid HeaderTid
        {
            get
            {
                Perf.CallCount();

                return Header.Tid;
            }
            set
            {
                Perf.CallCount();

                Header.Tid = value;
            }
        }

        #endregion

        #region Footer

        /// <summary>
        /// Footer Transaction Id
        /// </summary>
        internal KvTid FooterTid
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(Content.Length - sizeof(KvTid));
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(Content.Length - sizeof(KvTid), value);
            }
        }

        #endregion

        #region Page Content

        /// <summary>
        /// 0x00 : 8 Bytes Transaction Id
        /// </summary>
        internal KvTid Tid
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x00);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x00, value);
            }
        }

        /// <summary>
        /// 0x08 : 8 Bytes Number of Freespace RootPage
        /// </summary>
        internal KvPagenumber FsRootPage
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x08);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x08, value);
            }
        }

        /// <summary>
        /// 0x10 : 8 Bytes Number of Data RootPage
        /// </summary>
        internal KvPagenumber DataRootPage
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x10);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x10, value);
            }
        }

        /// <summary>
        /// 0x18 : 8 Bytes Last Page Written
        /// </summary>
        internal KvPagenumber LastPage
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x18);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x18, value);
            }
        }

        /// <summary>
        /// 0x20 : 8 Bytes DataTotalCount
        /// </summary>
        internal ulong DataTotalCount
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x20);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x20, value);
            }
        }

        /// <summary>
        /// 0x28 : 8 Bytes DataLocalCount
        /// </summary>
        internal ulong DataLocalCount
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x28);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x28, value);
            }
        }

        /// <summary>
        /// 0x30 : 8 Bytes FsTotalCount
        /// </summary>
        internal ulong FsTotalCount
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x30);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x30, value);
            }
        }

        /// <summary>
        /// 0x38 : 8 Bytes FsLocalCount
        /// </summary>
        internal ulong FsLocalCount
        {
            get
            {
                Perf.CallCount();

                return Content.ReadULong(0x38);
            }
            set
            {
                Perf.CallCount();

                Content.WriteULong(0x38, value);
            }
        }

        #endregion
    }
}
