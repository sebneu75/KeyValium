using KeyValium.Cursors;
using System.Collections;

namespace KeyValium.Iterators
{
    public struct KeyIterator : IEnumerator<KVItem>, IEnumerable<KVItem>
    {
        internal KeyIterator(Transaction tx, TreeRef keyref, bool forward)
        {
            Perf.CallCount();

            _forward = forward;

            Version = tx.GetVersion();

            _cursor = tx.GetCursor(keyref, InternalTrackingScope.TransactionChain);
            _cursor.DeleteHandling = DeleteHandling.Invalidate;

            _item = new KVItem(_cursor);

            Reset();
        }

        internal readonly TxVersion Version;

        private readonly KVItem _item;

        private readonly bool _forward;

        private Cursor _cursor;

        public void Reset()
        {
            Perf.CallCount();

            Validate();

            _cursor.SetPosition(_forward ? CursorPositions.BeforeFirst : CursorPositions.BehindLast);
        }

        public bool MoveNext()
        {
            Perf.CallCount();

            Validate();

            lock (Version.Tx.TxLock)
            {
                if (_forward)
                {
                    return _cursor.MoveToNextKey();
                }
                else
                {
                    return _cursor.MoveToPrevKey();
                }
            }
        }

        public KVItem Current
        {
            get
            {
                Perf.CallCount();

                Validate();

                if (_cursor.IsBOF || _cursor.IsEOF)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Cursor is in invalid position");
                }

                return _item;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                Perf.CallCount();

                return Current;
            }
        }

        private void Validate()
        {
            Perf.CallCount();

            if (_isdisposed)
            {
                throw new ObjectDisposedException("Iterator is already disposed.", (Exception)null);
            }

            Version.Validate();
        }

        public IEnumerator<KVItem> GetEnumerator()
        {
            Perf.CallCount();

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Perf.CallCount();

            return this;
        }

        #region IDisposable

        private bool _isdisposed;

        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    //_cursor?.TreeRef?.Dispose();
                    _cursor?.Dispose();
                    _cursor = null;
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _isdisposed = true;
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
            Perf.CallCount();

            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
