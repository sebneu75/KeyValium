using KeyValium.Memory;

namespace KeyValium.Encryption
{
    /// <summary>
    /// an ímplementation of IEncryption that does nothing
    /// </summary>
    internal class NoEncryption : IEncryption
    {
        public NoEncryption()
        {
            Perf.CallCount();
        }

        #region Encryption

        public Span<byte> Encrypt(AnyPage page)
        {
            Perf.CallCount();

            return page.Bytes.Span;
        }

        public void Decrypt(AnyPage page)
        {
            Perf.CallCount();
        }

        /// <summary>
        /// For testing purposes only
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pagenumber"></param>
        public void Decrypt(Span<byte> buffer, ulong pagenumber)
        {
            Perf.CallCount();
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
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~Encryptor()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

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
