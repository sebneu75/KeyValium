using KeyValium.Pages.Headers;
using System.Runtime.InteropServices;

namespace KeyValium.Pages
{
    [StructLayout(LayoutKind.Auto)]
    internal unsafe struct OverflowPage
    {
        #region Constructor

        internal OverflowPage(AnyPage page)
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

        internal readonly UniversalHeader Header;

        internal readonly ByteSpan Content;

        #endregion
    }
}
