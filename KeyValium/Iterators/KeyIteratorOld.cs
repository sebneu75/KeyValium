using KeyValium.Cursors;

namespace KeyValium.Iterators
{
    public class KeyIterator : IDisposable
    {
        internal KeyIterator(Transaction tx, bool forward)
        {
            _tx = tx;

            _cursor = _tx.GetCursor(null, InternalTrackingScope.TransactionChain);
            _cursor.DeleteHandling = DeleteHandling.Invalidate;

            var keyspan=ReadOnlySpan<byte>.Empty;
            _cursor.SetPosition(forward ? CursorPositions.BeforeFirst : CursorPositions.BehindLast, ref keyspan);
        }

        private Transaction _tx;

        private Cursor _cursor;

        public bool MoveNext()
        {
            lock (_tx.TxLock)
            {
                return _cursor.MoveNext();
            }
        }

        public bool MovePrevious()
        {
            lock (_tx.TxLock)
            {
                return _cursor.MovePrevious();
            }
        }

        public byte[] CurrentKey()
        {
            lock (_tx.TxLock)
            {
                return _cursor.GetCurrentKey();
            }
        }

        public ValueRef CurrentValue()
        {
            lock (_tx.TxLock)
            {
                return _cursor.GetCurrentValue();
            }
        }

        #region IDisposable implementation

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                    _cursor.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~KeyIterator()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    #endregion

}
