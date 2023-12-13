using KeyValium.Memory;
using System.Buffers.Binary;
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

            _password = password;
            _keyfile = keyfile;

            // Init
            {
                var pw = GetPassword();
                var salt = GetSalt();

                _bytegen = new Rfc2898DeriveBytes(pw, salt, 16384, HashAlgorithmName.SHA256);

                _key = _bytegen.GetBytes(32);
                var iv = _bytegen.GetBytes(16);

                _crypt = Aes.Create();
                _crypt.BlockSize = 128;
                _crypt.KeySize = 256;
                _crypt.Padding = PaddingMode.None;
                _crypt.Key = _key;
                _crypt.IV = iv;
            }
        }

        #region Encryption

        private readonly Aes _crypt;

        private readonly Rfc2898DeriveBytes _bytegen;

        private readonly string _password;

        private readonly string _keyfile;

        private readonly byte[] _key;

        private readonly uint PageSize;

        private readonly byte[] _cipherbuffer;

        /// <summary>
        /// the password is converted to a byte array.
        /// if password is null, the empty string is used.
        /// </summary>
        /// <returns></returns>
        private byte[] GetPassword()
        {
            Perf.CallCount();

            return Encoding.UTF8.GetBytes(_password ?? "");
        }

        /// <summary>
        /// The salt is derived from the Keyfile. 
        /// The salt must have a size of least 8 bytes. If the keyfile is smaller than that the remaining bytes are set to zero.
        /// A maximum of 1 MB is used from the keyfile.
        /// If _keyfile is null. A zeroed array of 8 bytes length is returned.
        /// </summary>
        /// <returns></returns>
        private byte[] GetSalt()
        {
            Perf.CallCount();

            if (_keyfile == null)
            {
                // initialized with zeros
                return new byte[8];
            }

            using (var reader = new FileStream(_keyfile, FileMode.Open, FileAccess.Read))
            {
                var saltlen = Math.Min(Math.Max(reader.Length, 8), 1024 * 1024);

                var bytes = new byte[saltlen];
                reader.Read(bytes, 0, (int)Math.Min(reader.Length, saltlen));

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

        private byte[] GetIV(KvPagenumber pageno)
        {
            Perf.CallCount();

            KvDebug.Assert(_key.Length == 32, "KeyLength missmatch!");
            KvDebug.Assert(sizeof(KvPagenumber) == 8, "Size mismatch!");

            var bytes = new byte[_key.Length + 16];

            BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(0, 8), pageno);
            Buffer.BlockCopy(_key, 0, bytes, 8, _key.Length);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(_key.Length + 8, 8), pageno);

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
