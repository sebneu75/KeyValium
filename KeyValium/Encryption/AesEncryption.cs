using KeyValium.Memory;
using System.Buffers.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace KeyValium.Encryption
{
    internal class AesEncryption : IEncryption
    {
        public AesEncryption(uint pagesize, string password, string keyfile)
        {
            Perf.CallCount();

            PageSize = pagesize;
            _cipherbuffer = new byte[PageSize];

            if (password == null && keyfile == null)
            {
                throw new ArgumentNullException("Password and/or Keyfile must be set!");
            }

            //
            // Initialization
            //
            var pw = Encoding.UTF8.GetBytes(password ?? "");
            var salt = GetSalt(keyfile);

            _bytegen = new Rfc2898DeriveBytes(pw, salt, 16384, HashAlgorithmName.SHA256);

            // clear password
            Array.Clear(pw, 0, pw.Length);

            // clear salt
            Array.Clear(salt, 0, salt.Length);

            _crypt = Aes.Create();
            _crypt.BlockSize = 128;
            _crypt.KeySize = 256;
            _crypt.Padding = PaddingMode.None;
            _crypt.Key = _bytegen.GetBytes(32);
            _crypt.IV = _bytegen.GetBytes(16);
        }

        #region Encryption

        private readonly Rfc2898DeriveBytes _bytegen;

        private readonly Aes _crypt;

        private readonly byte[] _cipherbuffer;

        private readonly uint PageSize;        

        /// <summary>
        /// The salt is derived from the Keyfile. 
        /// The salt must have a size of least 8 bytes. If the keyfile is smaller than that the remaining bytes are set to zero.
        /// A maximum of 1 MB is used from the keyfile.
        /// If _keyfile is null. A zeroed array of 8 bytes length is returned.
        /// </summary>
        /// <returns></returns>
        private byte[] GetSalt(string keyfile)
        {
            Perf.CallCount();

            if (keyfile == null)
            {
                // initialized with zeros
                return new byte[8];
            }

            using (var reader = new FileStream(keyfile, FileMode.Open, FileAccess.Read))
            {
                var saltlen = Math.Min(Math.Max(reader.Length, 8), 1024 * 1024);

                var bytes = new byte[saltlen];
                reader.Read(bytes, 0, (int)Math.Min(reader.Length, saltlen));

                // clear buffers
                reader.Flush();

                return bytes;
            }
        }

        public Span<byte> Encrypt(AnyPage page)
        {
            Perf.CallCount();

            var iv = GetIV(page.PageNumber);

            _crypt.EncryptCbc(page.Bytes.Span, iv, _cipherbuffer, PaddingMode.None);

            return _cipherbuffer;
        }

        public unsafe void Decrypt(AnyPage page)
        {
            Perf.CallCount();

            fixed (byte* ptr = _cipherbuffer)
            {
                MemUtils.MemoryCopy(ptr, page.Pointer, (int)page.PageSize);
            }

            var iv = GetIV(page.PageNumber);

            _crypt.DecryptCbc(_cipherbuffer, iv, page.Bytes.Span, PaddingMode.None);
        }

        private readonly MD5 _md5 = MD5.Create();

        /// <summary>
        /// generates an IV from the page number and the key
        /// </summary>
        /// <param name="pageno">the page number</param>
        /// <returns>IV</returns>
        private byte[] GetIV(KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(_crypt.Key.Length == 32, "KeyLength missmatch!");
            KvDebug.Assert(sizeof(KvPagenumber) == 8, "Size mismatch!");

            var bytes = new byte[_crypt.Key.Length + sizeof(KvPagenumber) + sizeof(KvPagenumber)];

            // write page number big endian
            BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(0, sizeof(KvPagenumber)), pageno);

            // copy key
            Buffer.BlockCopy(_crypt.Key, 0, bytes, sizeof(KvPagenumber), _crypt.Key.Length);

            // write page number little endian
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(_crypt.Key.Length + sizeof(KvPagenumber), sizeof(KvPagenumber)), pageno);

            // take MD5 checksum
            var hash = _md5.ComputeHash(bytes);

            Array.Clear(bytes, 0, bytes.Length);

            return hash;
        }

        #endregion

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!disposedValue)
            {
                if (disposing)
                {
                    Array.Clear(_cipherbuffer);

                    _crypt.Clear();
                    _crypt.Dispose();                    
                    _bytegen.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
