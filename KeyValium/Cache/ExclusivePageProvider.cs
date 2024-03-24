using KeyValium.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    /// <summary>
    /// Provides pages in exclusive mode
    /// </summary>
    internal sealed class ExclusivePageProvider : PageProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The Database</param>
        public ExclusivePageProvider(Database db) : base(db)
        {
            Perf.CallCount();
        }

        /// <summary>
        /// Gets a page from the Cache
        /// </summary>
        /// <param name="pagenumber">the pagenumber of the requested page</param>
        /// <param name="meta">Meta information (Not used in exclusive mode.)</param>
        /// <param name="usewritecache">true if the writecache is to be used. (Not used in exclusive mode.)</param>
        /// <returns>The requested page or null if the cache dors not contain the page.</returns>
        internal AnyPage GetPage(KvPagenumber pagenumber, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            // Page is null on the Sentinel value, so no need to check the out parameter "isvalid"
            ref var pageref = ref Cache.GetPage(pagenumber, out _);
            
            return pageref.Page;
        }

        /// <summary>
        /// Inserts or updates a page.
        /// </summary>
        /// <param name="page">The page to be inserted or updated</param>
        /// <param name="meta">Meta information (Not used in exclusive mode.)</param>
        /// <param name="usewritecache">true if the writecache is to be used. (Not used in exclusive mode.)</param>
        internal void UpsertPage(AnyPage page, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            var pageref = new PageRef(page.PageNumber, page, 0);

            Cache.UpsertPage(ref pageref);
        }

        /// <summary>
        /// Reads a page from the cache or if not found from disk.
        /// </summary>
        /// <param name="tx">The transaction</param>
        /// <param name="pagenumber">The page number.</param>
        /// <param name="createheader">true if header and content structures should be created on the page.</param>
        /// <param name="spilled">true if the page is a spilled page</param>
        /// <returns>the requested page with incremented refcount</returns>
        override protected AnyPage ReadPageInternal(Transaction tx, KvPagenumber pagenumber, bool createheader, bool spilled = false)
        {
            Perf.CallCount();

            KvDebug.Assert(pagenumber >= Limits.FirstMetaPage, "Pagenumber out of bounds.");

            var cachedpage = GetPage(pagenumber, tx?.Meta, spilled);
            if (cachedpage != null)
            {
                //KvDebug.Assert(cachedpage.State == PageStates.Clean, "Unclean page read from cache!");

                return cachedpage.AddRef();
            }

            var page = Allocator.GetPage(pagenumber, false, null, 0);
            ReadLocked(page, createheader);

            UpsertPage(page, tx?.Meta, spilled);

            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Limits.FirstMetaPage && page.PageNumber <= Limits.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Limits.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            return page;
        }

        /// <summary>
        /// writes a page to disk and upserts it in the cache.
        /// </summary>
        /// <param name="tx">The transaction</param>
        /// <param name="page">The page</param>
        override protected void WritePageInternal(Transaction tx, AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page.PageNumber >= Limits.FirstMetaPage, "Pagenumber out of bounds.");
            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Limits.FirstMetaPage && page.PageNumber <= Limits.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Limits.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            //KvDebug.Assert(page.State == PageStates.Dirty, "Only dirty pages can be written to disk!");

            WriteLocked(page);

            //page.State = spilled ? PageStates.Spilled : PageStates.Clean;

            UpsertPage(page, tx.Meta, true);
        }

        protected override void RemoveCachedPagesInternal(Transaction tx, PageRange range)
        {
            Perf.CallCount();

            for (var pageno = range.First; pageno <= range.Last; pageno++)
            {
                Cache.RemovePage(pageno);
            }
        }

        internal override CacheStats GetCacheStats()
        {
            Perf.CallCount();

            return Cache.GetStats();
        }

        internal override void ClearCacheStats()
        {
            Perf.CallCount();

            Cache.ClearStats();;
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!base.disposedValue)
            {
                if (disposing)
                {
                    Cache?.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
