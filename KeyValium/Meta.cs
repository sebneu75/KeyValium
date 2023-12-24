using KeyValium.Inspector;
using System.Data;

namespace KeyValium
{
    internal sealed class Meta
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="newtid"></param>
        /// <param name="mintid"></param>
        /// <param name="minlastpage"></param>
        /// <param name="maxlastpage"></param>
        internal Meta(ref MetaEntry meta, KvTid newtid, KvTid mintid, KvPagenumber minlastpage, KvPagenumber maxlastpage)
        {
            Perf.CallCount();

            Tid = newtid;
            MinTid = mintid;

            MinLastPage = minlastpage;
            MaxLastPage = maxlastpage;
            DataRootPage = meta.DataRootPage;
            FsRootPage = meta.FsRootPage;
            PageNumber = meta.PageNumber;
            LastPage = meta.LastPage;
            SourceLastPage = meta.LastPage;
            SourceTid = meta.Tid;
            DataTotalCount = meta.DataTotalCount;
            DataLocalCount = meta.DataLocalCount;
            FsTotalCount = meta.FsTotalCount;
            FsLocalCount = meta.FsLocalCount;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="source"></param>
        internal Meta(Meta source)
        {
            Perf.CallCount();

            Tid = source.Tid;
            MinTid = source.MinTid;
            MinLastPage = source.MinLastPage;
            MaxLastPage = source.MaxLastPage;

            DataRootPage = source.DataRootPage;
            FsRootPage = source.FsRootPage;
            PageNumber = source.PageNumber;
            LastPage = source.LastPage;
            SourceLastPage = source.SourceLastPage;
            SourceTid = source.SourceTid;
            DataTotalCount = source.DataTotalCount;
            DataLocalCount = source.DataLocalCount;
            FsTotalCount = source.FsTotalCount;
            FsLocalCount = source.FsLocalCount;
        }

        /// <summary>
        /// the original Tid as read from the source meta page
        /// </summary>
        internal readonly KvTid SourceTid;

        internal readonly KvTid Tid;

        internal readonly KvTid MinTid;

        internal KvPagenumber DataRootPage;

        internal KvPagenumber FsRootPage;

        internal ulong DataTotalCount;
        internal ulong DataLocalCount;
        internal ulong FsTotalCount;
        internal ulong FsLocalCount;

        internal readonly KvPagenumber PageNumber;

        // TODO check usage of LastPage vs MaxLastPage

        internal readonly KvPagenumber MinLastPage;

        internal readonly KvPagenumber MaxLastPage;

        /// <summary>
        /// the original last page as read from the source meta page
        /// </summary>
        internal readonly KvPagenumber SourceLastPage;

        /// <summary>
        /// the last page written to
        /// formerly initialized with the maximum last page of all metas
        /// </summary>        
        internal KvPagenumber LastPage;
    }
}