﻿using KeyValium.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cache
{
    internal sealed class SharedPageProvider : PageProvider
    {
        public SharedPageProvider(Database db) : base(db)
        {
            Perf.CallCount();

            _writecache = new LruCache(Database.Options.CachedItems >> 2);

            MinPageNumber = Limits.MinDataPageNumber;
        }

        private readonly LruCache _writecache;

        /// <summary>
        /// pages with a number smaller than this will not be cached
        /// </summary>
        private readonly KvPagenumber MinPageNumber;

        public AnyPage GetPage(KvPagenumber pagenumber, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            if (pagenumber < MinPageNumber)
            {
                return null;
            }

            if (usewritecache)
            {
                // TODO remove from cache if Tid not equal (should not happen)
                ref var pageref = ref _writecache.GetPage(pagenumber, out var isvalid);

                if (isvalid && pageref.Tid == meta.Tid)
                {
                    return pageref.Page;
                }

                return null;
            }
            else
            {
                ref var pageref = ref Cache.GetPage(pagenumber, out var isvalid);
                if (isvalid)
                {
                    // check if page is still valid
                    if (pageref.Tid >= meta.MinTid && pageref.Tid <= meta.SourceTid)
                    {
                        pageref.Tid = meta.SourceTid;

                        // Update SourceTid
                        //var newpageref = pageref.Value;
                        //newpageref.Tid = meta.SourceTid;                            
                        //_cache.UpsertPage(newpageref);

                        return pageref.Page;
                    }
                    else
                    {
                        Cache.RemovePage(pagenumber);
                    }
                }

                return null;
            }
        }

        public void UpsertPage(AnyPage page, Meta meta, bool usewritecache)
        {
            Perf.CallCount();

            if (page.PageNumber < MinPageNumber)
            {
                return;
            }

            if (usewritecache)
            {
                var pageref = new PageRef(page.PageNumber, page, meta.Tid);
                _writecache.UpsertPage(ref pageref);
            }
            else
            {
                var pageref = new PageRef(page.PageNumber, page, meta.SourceTid);
                Cache.UpsertPage(ref pageref);
            }
        }

        override protected void ClearWriteCacheInternal()
        {
            Perf.CallCount();

            _writecache.Clear();
        }

        override protected void CommitWriteCacheInternal(KvTid sourcetid, KvTid tid)
        {
            Perf.CallCount();

            try
            {
                // copy writecache to cache
                _writecache.CopyTo(Cache);

                // Update Tid on cached pages
                // Don't do that because it will mess up the lifetime of freed pages.
                // If the freed page is reused by another machine it will still appear to be valid because
                // its Tid will fall in the range of active transactions

                // TODO check if it works to remove freed pages from the cache
                Cache.UpdateTids(sourcetid, tid);
            }
            catch (Exception)
            {
                try
                {
                    // rollback the changes in case of error
                    _writecache.RemoveFrom(Cache);
                }
                catch (Exception)
                {
                    // invalidate complete cache if rollback also fails
                    Cache.Clear();
                    throw;
                }
                finally
                {
                    _writecache.Clear();
                }

                throw;
            }
            finally
            {
                _writecache.Clear();
            }
        }

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

        internal override CacheStats GetCacheStats()
        {
            Perf.CallCount();

            return Cache.GetStats();
        }

        internal override void ClearCacheStats()
        {
            Perf.CallCount();

            Cache.ClearStats();
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!base.disposedValue)
            {
                if (disposing)
                {
                    Cache.Dispose();
                    _writecache.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
