using KeyValium.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    internal sealed class ExclusivePageProvider : PageProvider
    {
        public ExclusivePageProvider(Database db) : base(db)
        {
            Perf.CallCount();

            _cache = new LruCache(Database.Options.CachedItems);
        }

        internal readonly LruCache _cache;

        public AnyPage GetPage(KvPagenumber pagenumber, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            // Page is null on the Sentinel value, so no need to check the out parameter "isvalid"
            ref var pageref = ref _cache.GetPage(pagenumber, out _);
            
            return pageref.Page;
        }

        public void UpsertPage(AnyPage page, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            var pageref = new PageRef(page.PageNumber, page, 0);

            _cache.UpsertPage(ref pageref);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="pagenumber"></param>
        /// <param name="createheader"></param>
        /// <param name="spilled"></param>
        /// <returns>page with incremented refcount</returns>
        override protected AnyPage ReadPageInternal(Transaction tx, KvPagenumber pagenumber, bool createheader, bool spilled = false)
        {
            Perf.CallCount();

            KvDebug.Assert(pagenumber >= Limits.FirstMetaPage, "Pagenumber out of bounds.");

            var cachedpage = GetPage(pagenumber, tx?.Meta, spilled);
            if (cachedpage != null)
            {
                Validator.ValidatePage(cachedpage, pagenumber);
                //KvDebug.Assert(cachedpage.State == PageStates.Clean, "Unclean page read from cache!");

                return cachedpage.AddRef();
            }

            var page = Allocator.GetPage(pagenumber, false, null, 0);
            ReadLocked(page, createheader);

            Validator.ValidatePage(page, pagenumber);

            UpsertPage(page, tx?.Meta, spilled);

            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Limits.FirstMetaPage && page.PageNumber <= Limits.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Limits.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            return page;
        }

        override protected void WritePageInternal(Transaction tx, AnyPage page)
        {
            Perf.CallCount();

            KvDebug.Assert(page.PageNumber >= Limits.FirstMetaPage, "Pagenumber out of bounds.");
            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Limits.FirstMetaPage && page.PageNumber <= Limits.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Limits.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            Validator.ValidatePage(page, page.PageNumber);

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
                _cache.RemovePage(pageno);
            }
        }

        internal override CacheStats GetCacheStats()
        {
            Perf.CallCount();

            return _cache.GetStats();
        }

        internal override void ClearCacheStats()
        {
            Perf.CallCount();

            _cache.ClearStats();;
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!base.disposedValue)
            {
                if (disposing)
                {
                    _cache?.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
