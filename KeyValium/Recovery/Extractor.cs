using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Encryption;
using KeyValium.Inspector;
using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KeyValium.Recovery
{
    public class Extractor
    {

        #region Constructor

        private Extractor(string dbfile, string targetpath, uint pagesize = 0, string password = null, string keyfile = null)
        {
            DbFilename = dbfile;
            TargetPath = targetpath;
            PageSize = pagesize;
            Password = password;
            Keyfile = keyfile;

            ValidateFile(DbFilename);
            ValidateTargetPath(TargetPath);

            if (PageSize != 0)
            {
                Limits.ValidatePageSize(pagesize);
            }

            DbFile = new FileStream(dbfile, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);

            if (PageSize == 0)
            {
                PageSize = GetPageSize();
            }
        }

        #endregion

        #region Properties

        internal readonly string DbFilename;

        internal readonly string TargetPath;

        internal readonly string Password;

        internal readonly string Keyfile;

        internal readonly uint PageSize;

        internal readonly FileStream DbFile;

        #endregion

        public static void Dump(string dbfile, string targetpath, uint pagesize = 0, string password = null, string keyfile = null)
        {
            var ext = new Extractor(dbfile, targetpath);
            ext.ExtractData();
        }

        private void ExtractData()
        {
            var size = DbFile.Length;
            var numpages = (ulong)size / PageSize;

            if ((ulong)size % PageSize != 0)
            {
                Console.WriteLine("Warning: Filesize is not evenly divisable by PageSize.");
            }

            for (KvPagenumber pageno = 0; pageno < numpages; pageno++)
            {
                var page = ReadPage(pageno);
                if (page.Header.Magic == Limits.Magic)
                {
                    switch (page.PageType)
                    {
                        case PageTypes.DataLeaf:
                            ExtractDataFromPage(page);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ExtractDataFromPage(AnyPage page)
        {
            try
            {
                ref var cp = ref page.AsContentPage;

                for (int i = 0; i < cp.Header.KeyCount; i++)
                {
                    var entry = cp.GetEntryAt(i);

                    // save key
                    var key = entry.KeyBytes;
                    SaveKey(key, cp.Header.Tid, page.PageNumber, i);

                    if ((entry.Flags & EntryFlags.IsOverflow) != 0)
                    {
                        SaveValue(entry.OverflowPageNumber, entry.OverflowLength, cp.Header.Tid, page.PageNumber, i);
                    }
                    else
                    {
                        SaveValue(entry.InlineValueBytes, cp.Header.Tid, page.PageNumber, i);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Page {0}: {1}", page.PageNumber, ex.Message);
            }
        }

        private void SaveValue(ByteSpan value, ulong tid, ulong pageno, int index)
        {
            var path = EnsurePath(tid, pageno, index, "val");
            using (var writer = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                writer.Write(value.ReadOnlySpan);
            }
        }

        private void SaveValue(ulong ovpageno, ulong ovlength, ulong tid, ulong pageno, int index)
        {
            var buffer = new byte[65536];

            var path = EnsurePath(tid, pageno, index, "val");
            using (var writer = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                var toread = (long)ovlength;

                DbFile.Seek((long)ovpageno * (long)PageSize + UniversalHeader.HeaderSize, SeekOrigin.Begin);

                while (toread > 0)
                {
                    var bytesread = DbFile.Read(buffer, 0, (int)Math.Min(buffer.Length, toread));
                    writer.Write(buffer, 0, bytesread);

                    toread -= bytesread;
                }
            }
        }

        private void SaveKey(ByteSpan key, ulong tid, ulong pageno, int index)
        {
            var path = EnsurePath(tid, pageno, index, "key");
            using (var writer = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                writer.Write(key.ReadOnlySpan);
            }
        }

        private string EnsurePath(ulong tid, ulong pageno, int index, string ext)
        {
            var filename = string.Format("{0}.{1}.{2}", pageno, index, ext);
            var stid = string.Format("{0:000000000000}", tid);
            var path = Path.Combine(TargetPath, stid, filename);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }

        private AnyPage ReadPage(KvPagenumber pageno)
        {
            DbFile.Seek((long)pageno * (long)PageSize, SeekOrigin.Begin);

            var page = new AnyPage(null, PageSize);
            page.Initialize(pageno, false, null, 0);

            var read = DbFile.Read(page.Bytes.Span);

            // TODO add encryption
            // encheader.Decrypt(page);
            page.CreateHeaderAndContent(null, 0);

            return page;
        }

        #region Validation

        private static void ValidateTargetPath(string targetpath)
        {
            if (Directory.Exists(targetpath))
            {
                foreach (var item in Directory.EnumerateFileSystemEntries(targetpath))
                {
                    throw new NotSupportedException("Target path is not empty.");
                }
            }
        }

        private static void ValidateFile(string dbfile)
        {

        }

        #endregion

        #region Helpers

        private uint GetPageSize()
        {
            Perf.CallCount();

            DbFile.Seek(0, SeekOrigin.Begin);

            var encheader = GetEncryptor(Limits.MinPageSize);

            var page = new AnyPage(null, Limits.MinPageSize);
            page.Initialize(0, false, null, 0);
            var read = DbFile.Read(page.Bytes.Span);
            encheader.Decrypt(page);
            page.CreateHeaderAndContent(null, 0);

            try
            {
                PageValidator.ValidateFileHeader(page, 0, false);
                return 1u << page.Header.PageSizeExponent;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException("Page size could not be determined.");
            }
        }

        private IEncryption GetEncryptor(uint pagesize)
        {
            Perf.CallCount();

            if (Keyfile != null || Password != null)
            {
                return new AesEncryption(pagesize, Password, Keyfile);
            }

            return new NoEncryption();
        }

        #endregion
    }
}
