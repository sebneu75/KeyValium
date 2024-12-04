using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Cursors
{
    [StructLayout(LayoutKind.Auto)]
    internal struct NodePathChainItem : IDisposable
    {
        internal NodePathChainItem(Transaction transaction)
        {
            Transaction = transaction;

            Path = new NodePath();
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="path"></param>
        internal NodePathChainItem(Transaction transaction, NodePath path)
        {
            Transaction = transaction;

            Path = path;
        }

        internal Transaction Transaction;

        internal NodePath Path;

        internal NodePathChainItem Copy(Transaction tx)
        {
            return new NodePathChainItem(tx, Path.Copy());  
        }

        public void Dispose()
        {
            Path?.DisposeItems();
            Path= null;
        }
    }
}
