using KeyValium.Pages.Headers;


namespace KeyValium.Pages
{
    public unsafe sealed class FileHeaderPage
    {
        #region Constructor

        internal FileHeaderPage(AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page != null, "page cannot be null.");

            Page = page;

            Header = Page.Header;
            Content = Page.Bytes.Slice(UniversalHeader.HeaderSize, Header.ContentSize);
        }

        #endregion

        #region Properties

        internal readonly AnyPage Page;

        internal readonly UniversalHeader Header;

        internal readonly ByteSpan Content;

        #endregion

    }
}
