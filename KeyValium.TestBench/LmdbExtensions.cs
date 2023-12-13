using LightningDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    public static class LmdbExtensions
    {
        public static (LightningTransaction, LightningDatabase) BeginReadTransaction(this LightningEnvironment env)
        {
            var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

            return (tx, db);
        }

        public static (LightningTransaction, LightningDatabase) BeginWriteTransaction(this LightningEnvironment env)
        {
            var tx = env.BeginTransaction();
            var db = tx.OpenDatabase("custom", new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

            return (tx, db);
        }
    }
}
