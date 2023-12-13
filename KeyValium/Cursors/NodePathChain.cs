using KeyValium.Collections;

namespace KeyValium.Cursors
{
    /// <summary>
    /// A class to manage tracked cursors among nested transactions
    /// </summary>
    internal sealed class NodePathChain
    {
        internal NodePathChain(Cursor cursor, Transaction tx)
        {
            Perf.CallCount();

            Cursor = cursor;
            Items = new KvArray<NodePathChainItem>();
            Items.Append(new NodePathChainItem(tx));

            UpdateTxAndPath();
        }

        internal KvArray<NodePathChainItem> Items;

        internal readonly Cursor Cursor;

        private void UpdateTxAndPath()
        {
            Perf.CallCount();

            if (Items.HasCurrent)
            {
                ref var item = ref Items.CurrentItem;
                Cursor.CurrentPath = item.Path;
                Cursor.CurrentTransaction = item.Transaction;
            }
            else
            {
                Cursor.CurrentPath = null;
                Cursor.CurrentTransaction = null;
            }
        }

        internal void AppendCopy(Transaction tx)
        {
            Perf.CallCount();

            Items.Append(Items.CurrentItem.Copy(tx));

            UpdateTxAndPath();
        }

        internal void CommitToParent()
        {
            Perf.CallCount();

            if (!Items.HasPrevItem)
            {
                // move to parent transaction
                ref var current = ref Items.CurrentItem;
                
                current.Transaction = current.Transaction.Parent;
            }
            else
            {
                ref var prev = ref Items.PrevItem;
                ref var current=ref Items.CurrentItem;

                // invalidate previous because of reference counting
                prev.Path.Invalidate();
                prev.Path = current.Path;
                
                // set current path to null to avoid disposing with next call
                current.Path = null;

                Items.Remove();
            }

            UpdateTxAndPath();
        }

        internal void RollbackToParent()
        {
            Perf.CallCount();

            // invalidate this.Path because of reference counting
            Items.CurrentItem.Path.Invalidate();

            Items.Remove();

            UpdateTxAndPath();
        }
    }
}
