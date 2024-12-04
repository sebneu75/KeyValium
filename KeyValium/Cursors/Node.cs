using System.Runtime.InteropServices;

namespace KeyValium.Cursors
{
    [StructLayout(LayoutKind.Auto)]
    internal struct Node : IDisposable
    {
        public Node(AnyPage page, int keyindex)
        {
            Perf.CallCount();

            _page = null;
            KeyIndex = keyindex;

            Page = page;
        }

        private AnyPage _page;

        internal AnyPage Page
        {
            get
            {
                Perf.CallCount();

                return _page;
            }
            set
            {
                Perf.CallCount();

                if (value != _page)
                {
                    value?.AddRef();
                    _page?.Dispose();
                    _page = value;
                }
            }
        }

        internal int KeyIndex;

        public override string ToString()
        {
            Perf.CallCount();

            return String.Format("{0}.{1}", Page == null ? "<null>" : Page.PageNumber, KeyIndex);
        }

        public void Dispose()
        {
            Perf.CallCount();

            Page = null;
        }
    }
}
