using KeyValium.Collections;
using KeyValium.Cursors;
using static System.Formats.Asn1.AsnWriter;

namespace KeyValium
{
    /// <summary>
    /// Represents a "pointer" to a subtree.
    /// </summary>
    public sealed class TreeRef : IDisposable
    {
        private static ulong OidCounter = 0;

        /// <summary>
        /// Invalid TreeRef
        /// </summary>
        public static readonly TreeRef Invalid = new TreeRef();

        /// <summary>
        /// Constructor for invalid TreeRef
        /// </summary>
        private TreeRef()
        {
            Scope = TrackingScope.TransactionChain;
            Database = null;
            Oid = 0;
            Cursor = null;
            _isdisposed = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tx">the transaction.</param>
        /// <param name="scope">the tracking scope</param>
        internal TreeRef(Transaction tx, TrackingScope scope = TrackingScope.TransactionChain)
        {
            Perf.CallCount();

            Scope = scope;
            Database = tx.Database;

            Oid = Interlocked.Increment(ref OidCounter);

            Cursor = tx.GetCursor(null, (InternalTrackingScope)Scope, false);

            Database.Tracker.Add(this);
        }

        internal readonly ulong Oid;

        /// <summary>
        /// The tracking scope of the TreeRef
        /// </summary>
        public readonly TrackingScope Scope;

        internal readonly Database Database;

        internal byte[][] Keys;

        internal Cursor Cursor;

        /// <summary>
        /// Returns the state of the TreeRef.
        /// </summary>
        public TreeRefState State
        {
            get
            {
                Perf.CallCount();

                if (_isdisposed)
                {
                    return TreeRefState.Disposed;
                }

                if (Cursor == null)
                {
                    return TreeRefState.Suspended;
                }

                if (Cursor.CurrentPath == null || !Cursor.CurrentPath.IsValid)
                {
                    return TreeRefState.Inactive;
                }

                return TreeRefState.Active;
            }
        }

        internal void CopyKeys(ReadOnlyMemory<byte>[] keys)
        {
            if (Scope == TrackingScope.Database)
            {
                Keys = new byte[keys.Length][];

                for (int i = 0; i < keys.Length; i++)
                {
                    Keys[i] = keys[i].ToArray();
                }
            }
        }

        internal void Validate(Transaction tx)
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating KeyRef.");

            switch (State)
            {
                case TreeRefState.Active:
                    Cursor.Validate();

                    if (tx.Oid != Cursor.CurrentTransaction.Oid)
                    {
                        throw new KeyValiumException(ErrorCodes.InvalidCursor, "TreeRef does not belong to Transaction");
                    }

                    break;

                case TreeRefState.Inactive:
                    throw new KeyValiumException(ErrorCodes.InvalidCursor, "TreeRef is inactive.");

                case TreeRefState.Suspended:
                    throw new KeyValiumException(ErrorCodes.InvalidCursor, "TreeRef is suspended.");

                case TreeRefState.Disposed:
                    throw new ObjectDisposedException("TreeRef is already disposed.");
            }
        }

        internal KvPagenumber PageNumber
        {
            get
            {
                Perf.CallCount();

                return Cursor.GetCurrentSubTree().Value;
            }
            set
            {
                Perf.CallCount();

                //KvDebug.Assert(_istouched, "KeyRef is untouched!");

                Cursor.SetCurrentSubTree(value);
            }
        }

        internal ulong TotalCount
        {
            get
            {
                Perf.CallCount();

                return Cursor.GetCurrentTotalCount();
            }
            set
            {
                Perf.CallCount();

                //KvDebug.Assert(_istouched, "KeyRef is untouched!");

                Cursor.SetCurrentTotalCount(value);
            }
        }

        internal ulong LocalCount
        {
            get
            {
                Perf.CallCount();

                return Cursor.GetCurrentLocalCount();
            }
            set
            {
                Perf.CallCount();

                //KvDebug.Assert(_istouched, "KeyRef is untouched!");

                Cursor.SetCurrentLocalCount(value);
            }
        }

        internal void RestoreCursor(Transaction tx)
        {
            var keys = Keys.Select(x => new ReadOnlyMemory<byte>(x)).ToArray();

            try
            {
                using (var treeref = tx.GetTreeRef(Scope, keys))
                {
                    // HACK
                    Cursor = treeref.Cursor;
                    treeref.Cursor = null;
                }
            }
            catch (Exception ex)
            {
                Cursor = null;

                //throw;
            }
        }

        internal void AddNodes(NodePath nodes)
        {
            Perf.CallCount();

            //Validate();
            for (int i = nodes.First; i <= nodes.Last; i++)
            {
                ref var node = ref nodes.GetNode(i);
                Cursor.CurrentPath.Append(node.Page, node.KeyIndex);
            }

            //_istouched = false;
        }

        //internal void AddNodes(KeyPath2 nodes)
        //{
        //    //Validate();

        //    for (var index = nodes.First; index <= nodes.Last; index++)
        //    {
        //        Cursor.CurrentKeyPath.Append(nodes.Items[index]);
        //    }

        //    _istouched = false;
        //}

        //internal void AddNodes(KvList2<KeyPointer2> list)
        //{
        //    //Validate();

        //    for (var iter = list.GetIterator(); iter.MoveNext();)
        //    {
        //        ref var node = ref iter.CurrentItem;
        //        Cursor.CurrentKeyPath.Append(new KeyPointer2(node.Page, node.KeyIndex));
        //    }

        //    _istouched = false;
        //}

        //private bool _istouched;

        internal void Touch()
        {
            Perf.CallCount();

            // TODO test
            //if (!_istouched)
            //{
            Cursor.Touch(false);
            //_istouched = true;
            //}
        }

        public override string ToString()
        {
            return string.Format("T-Oid={0}, C-Oid={1}", Oid, Cursor?.Oid);
        }

        #region IDisposable implementation

        private bool _isdisposed;

        private void Dispose(bool disposing)
        {
            Perf.CallCount();

            if (!_isdisposed)
            {
                if (disposing)
                {
                    Database.Tracker.Remove(this);

                    Cursor?.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                _isdisposed = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~KeyRef()
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
