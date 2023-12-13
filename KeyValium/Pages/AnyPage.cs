using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System.Buffers;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;

namespace KeyValium.Pages
{
    internal sealed unsafe class AnyPage : IDisposable
    {
        private static ulong OidCounter = 0;

        internal AnyPage(PageAllocator allocator, uint pagesize)
        {
            Perf.CallCount();

            Oid = Interlocked.Increment(ref OidCounter);

            Allocator = allocator;
            PageSize = pagesize;
            _bytes = new byte[pagesize];
            Handle = new Memory<byte>(_bytes).Pin();
            Pointer = (byte*)Handle.Pointer;

            Bytes = default;
        }

        internal void Initialize(KvPagenumber pageno, bool createheader, ushort? initpagetype, KvTid tid)
        {
            Perf.CallCount();

            PageNumber = pageno;
            PageType = PageTypes.Raw;

            Bytes = new ByteSpan(Pointer, (int)PageSize);

            if (createheader)
            {
                CreateHeaderAndContent(initpagetype, tid);

                KvDebug.Assert(PageType == Header.PageType &&
                              (
                                  (AsContentPage.Content.Length == 0) ||
                                  (PageType == AsContentPage.PageType && PageType == AsContentPage.Header.PageType)
                              ), "FAIL");
            }
        }

        internal void CreateHeaderAndContent(ushort? initpagetype, KvTid tid)
        {
            Perf.CallCount();

            //Header = new UniversalHeader(this);
            Header = new UniversalHeader(Bytes.Slice(0, UniversalHeader.HeaderSize), PageSize);

            if (initpagetype.HasValue)
            {
                Header.InitCommonHeader(initpagetype.Value, PageNumber);
            }

            PageType = Header.PageType;

            if (tid != 0)
            {
                Header.Tid = tid;
            }

            // Create Pagehandlers
            if (PageTypes.IsContentPageType(PageType))
            {
                // TODO get rid of new
                AsContentPage = new ContentPage(this);
            }
            else if (PageType == PageTypes.DataOverflow)
            {
                // TODO get rid of new
                AsOverflowPage = new OverflowPage(this);
            }
        }

        internal readonly ulong Oid;

        internal readonly PageAllocator Allocator;

        internal readonly byte[] _bytes;
        internal readonly byte* Pointer;
        internal ByteSpan Bytes;
        internal readonly MemoryHandle Handle;

        internal UniversalHeader Header;
        internal ContentPage AsContentPage;
        internal OverflowPage AsOverflowPage;

        internal readonly uint PageSize;

        internal KvPagenumber PageNumber;
        internal ushort PageType;

        internal void InitContentHeader(KvTid tid)
        {
            Perf.CallCount();

            switch (PageType)
            {
                case PageTypes.DataIndex:
                case PageTypes.DataLeaf:
                case PageTypes.FsIndex:
                case PageTypes.FsLeaf:
                    AsContentPage.Header.InitContentHeader(tid);
                    break;

                case PageTypes.DataOverflow:
                    AsOverflowPage.Header.InitOverflowHeader(tid);
                    break;

                case PageTypes.Raw:
                    // do nothing
                    break;

                default:
                    throw new KeyValiumException(ErrorCodes.InvalidPageType, "PageType cannot be initialized");
            }
        }

        #region Reference Counting

        /// <summary>
        /// managed by PageAllocator
        /// </summary>
        internal bool IsInUse = false;

        public override int GetHashCode()
        {
            Perf.CallCount();

            // TODO where is this needed?
            return (int)Oid ^ (int)(Oid >> 32);
        }

        public override bool Equals(object obj)
        {
            Perf.CallCount();

            return object.ReferenceEquals(this, obj);
        }

        internal int RefCount;

        internal AnyPage AddRef()
        {
            Perf.CallCount();

            if (!IsInUse)
            {
                throw new NotSupportedException("Page is not in use!");
            }

            RefCount++;

            return this;
        }

        public void Dispose()
        {
            Perf.CallCount();

            if (!IsInUse)
            {
                throw new NotSupportedException("Page is not in use!");
            }

            if (--RefCount == 0)
            {
                // clear ByteSpans
                Bytes = default;
                Header = default;
                AsContentPage = default;
                AsOverflowPage = default;

                Allocator.Recycle(this);
            }
        }

        internal void Destroy()
        {
            Perf.CallCount();

            if (IsInUse)
            {
                throw new NotSupportedException("Cannot destroy page in use!");
            }

            Handle.Dispose();
        }

        #endregion

        #region Debugging

        private void PrintStackTrace()
        {
            var sb = new StringBuilder();
            var st = new StackTrace(1);

            var first = st.GetFrame(0);
            sb.AppendFormat("******* {0} *******\n", first.GetMethod().Name);
            sb.AppendFormat("RefCount Before = {0}\n", RefCount);

            foreach (var frame in st.GetFrames())
            {
                sb.AppendFormat("{0}:{1} - {2}\n", frame.GetFileName(), frame.GetFileLineNumber, frame.GetMethod().Name);
            }

            sb.AppendFormat("*******************\n");

            Console.WriteLine(sb.ToString());
        }

        #endregion
    }
}
