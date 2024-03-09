using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends
{
    internal class TransactionManager
    {
        internal TransactionManager(Database db, bool prevsnapshot)
        {
            _db = db;
            _prevsnapshot = prevsnapshot;
        }

        private readonly bool _prevsnapshot;

        private readonly object _lock = new object();

        private Transaction _tx;

        private readonly Database _db;

        private int _refcount;

        /// <summary>
        /// Returns the current transaction. Throws if no current tansaction exists.
        /// </summary>
        internal Transaction Tx
        {
            get
            {
                Perf.CallCount();

                KvDebug.Assert(Monitor.IsEntered(_lock), "Lock not held!");

                if (_tx == null)
                {
                    throw new ArgumentNullException("Transaction", "There is no current transaction. The action must be done within a transaction.");
                }

                return _tx;
            }
        }

        /// <summary>
        /// Does an action within a transaction. Calls can be nested.
        /// If no transaction exists one is started. 
        /// If the call created a transaction it is rolled back if action throws an exception. Otherwise it is committed.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="appendmode">Enables append mode if true. This can be used when inserting multiple keys in order to save disk space.</param>
        internal void Do(Action action, bool appendmode = false)
        {
            Perf.CallCount();

            lock (_lock)
            {
                Ensure(appendmode);

                try
                {
                    action.Invoke();
                    Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        Rollback();
                    }
                    catch (Exception ex2)
                    {
                        throw new AggregateException(ex, ex2);
                    }

                    throw;
                }
            }
        }

        private void Ensure(bool appendmode)
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(_lock), "Lock not held!");

            if (_tx == null)
            {
                _tx = _prevsnapshot ? _db.BeginPreviousSnapshotReadTransaction() : _db.BeginWriteTransaction();
                _tx.AppendMode = appendmode;
                _refcount = 1;
            }
            else
            {
                _refcount++;
            }
        }

        private void Commit()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(_lock), "Lock not held!");

            if (--_refcount == 0)
            {
                _tx.Commit();
                _tx.Dispose();
                _tx = null;
            }
        }

        internal void Rollback()
        {
            Perf.CallCount();

            KvDebug.Assert(Monitor.IsEntered(_lock), "Lock not held!");

            if (--_refcount == 0)
            {
                _tx.Rollback();
                _tx.Dispose();
                _tx = null;
            }
        }
    }
}
