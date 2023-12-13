using KeyValium.Collections;
using KeyValium.Inspector;
using KeyValium.TypeDefs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KeyValium.Cache
{
    internal unsafe class ExclusiveMMPageprovider : PageProvider
    {
        public ExclusiveMMPageprovider(Database db) : base(db)
        {
            File = MemoryMappedFile.CreateFromFile(DbFile, null, 256 * 1024 * 1024,
                                                   MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
            View = File.CreateViewAccessor();

            View.SafeMemoryMappedViewHandle.AcquirePointer(ref Pointer);

            Pointer += View.PointerOffset;
        }

        private readonly byte* Pointer;

        private readonly MemoryMappedFile File;

        private readonly MemoryMappedViewAccessor View;

        //private Stopwatch sw = new Stopwatch();
        //private long total;
        //private long count;
        //private long total2;
        //private long count2;

        override protected AnyPage ReadPageInternal(Transaction tx, KvPagenumber pagenumber, bool createheader, bool spilled = false)
        {
            KvDebug.Assert(pagenumber >= Database.Options.FirstMetaPage, "Pagenumber out of bounds.");

            var bh = Allocator.Allocate();

            var offset = (long)pagenumber * PageSize;

            //sw.Restart();
            Buffer.MemoryCopy(Pointer + offset, bh.Pointer, bh.Size, PageSize);
            //sw.Stop();
            //total += sw.ElapsedTicks;
            //count++;

            var plain = Encryptor.Decrypt(pagenumber, bh);

            var page = new AnyPage(pagenumber, PageSize, createheader, null, plain, 0);
            Validator.ValidatePage(page, pagenumber);

            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Database.Options.FirstMetaPage && page.PageNumber <= Database.Options.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Database.Options.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            return page;
        }

        override protected void WritePageInternal(Transaction tx, AnyPage page)
        {
            KvDebug.Assert(page.PageNumber >= Database.Options.FirstMetaPage, "Pagenumber out of bounds.");
            KvDebug.Assert(page.PageType == PageTypes.Meta && page.PageNumber >= Database.Options.FirstMetaPage && page.PageNumber <= Database.Options.MetaPages ||
                         page.PageType != PageTypes.Meta && page.PageNumber >= Database.Options.MinDataPageNumber,
                         "Pagetype and Pagenumber mismatch!");

            Validator.ValidatePage(page, page.PageNumber);

            //KvDebug.Assert(page.State == PageStates.Dirty, "Only dirty pages can be written to disk!");
            KvDebug.Assert(page.PageSize == PageSize, "Pagesize mismatch!");

            var offset = (long)page.PageNumber * PageSize;

            var cipher = Encryptor.Encrypt(page.PageNumber, page.Handle);

            Buffer.MemoryCopy(cipher.Pointer, Pointer + offset, cipher.Size, PageSize);

            //View.WriteArray((long)page.PageNumber.Value * PageSize, bytes, 0, bytes.Length);

            //page.State = spilled ? PageStates.Spilled : PageStates.Clean;
        }

        internal override void Flush()
        {
            View.Flush();

            // TODO check if necessary
            //base.Flush();
        }

        internal override void SetFilesize(long length)
        {
            // not supported with memory mapped files
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            //var ns = ((double)total / (double)count) / Stopwatch.Frequency * 1000000000;
            //var ns2 = ((double)total2 / (double)count2) / Stopwatch.Frequency * 1000000000;

            //Console.WriteLine("{0:#.00}ns", ns);
            //Console.WriteLine("{0:#.00}ns", ns2);

            if (!base.disposedValue)
            {
                if (disposing)
                {
                    View.SafeMemoryMappedViewHandle.ReleasePointer();
                    View.Dispose();
                    File.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
