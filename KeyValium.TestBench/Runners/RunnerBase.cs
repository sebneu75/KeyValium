using KeyValium;
using KeyValium.Cursors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyValium.TestBench.Runners
{
    abstract class RunnerBase
    {
        public RunnerBase()
        {
        }

        public readonly List<uint> PageSizes = new List<uint>() { /*256, 512,*/ 1024, 2048, 4096, 8192, 16384, 32768, 65536 };

        public abstract IEnumerable<TestDescription> GetItems();

        public virtual bool IsEndless
        {
            get
            {
                return false;
            }
        }

        public abstract string Name
        {
            get;
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0}", Name);
            }
        }

        protected void DoAction(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> templist, Action<Transaction, List<KeyValuePair<byte[], byte[]>>> action)
        {
            Transaction tx = null;

            try
            {
                tx = db.BeginWriteTransaction();

                var m1 = td.Measure.MeasureTime(action.Method.Name, 0, templist.Count, () => action.Invoke(tx, templist));

                tx.Commit();

                Console.WriteLine(m1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tx.Rollback();
                throw;
            }

        }

        public void Run(int count)
        {
            var exceptions = new List<Exception>();

            foreach (var td in GetItems())
            {
                try
                {
                    Console.WriteLine("Running Item '{0} - {1} - {2}'...", td.Name, td.ParameterName, td.ParameterValue);

                    for (int i = 0; i < count; i++)
                    {
                        RunItem(td);
                    }

                    td.Finish();
                    td.Save();

                    td.Measure.PrintLastResult();

                    //KeyValium.Performance.Counters.Print();
                    //KeyValium.Performance.Counters.Clear();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Multiple errors occured", exceptions);
            }
        }

        internal abstract void RunItem(TestDescription td);




        #region Functions

        protected void IterateItems(TestDescription td, Database db, bool forward, List<KeyValuePair<byte[], byte[]>> items)
        {
            var tx = BeginReadTransaction(td, db);
            var count = 0;

            var title = string.Format("Iterate{0}", forward ? "Forward" : "Backward");

            try
            {
                var m = td.Measure.MeasureTime(title, 0, td.KeyCount, () =>
                {
                    using (var iter = tx.GetIterator(null, forward))
                    {
                        //if (forward)
                        //{
                            while (iter.MoveNext())
                            {
                                var key = iter.Current.Value.Key;
                                var val = iter.Current.Value.Value;

                                var keylen = key == null ? 0 : key.Length;
                                var vallen = val.Length;

                                //Debug.Assert(keylen >= td.MinKeySize && keylen <= td.MaxKeySize, "Fail!");
                                //Debug.Assert(vallen >= td.MinValueSize && vallen <= td.MaxValueSize, "Fail!");

                                count++;
                            }
                        //}
                        //else
                        //{
                        //    while (iter.MovePrevious())
                        //    {
                        //        var key = iter.CurrentKey();
                        //        var val = iter.CurrentValue();

                        //        var keylen = key == null ? 0 : key.Length;
                        //        var vallen = val.Length;

                        //        //Debug.Assert(keylen >= td.MinKeySize && keylen <= td.MaxKeySize, "Fail!");
                        //        //Debug.Assert(vallen >= td.MinValueSize && vallen <= td.MaxValueSize, "Fail!");

                        //        count++;
                        //    }
                        //}
                    }
                });

                Console.WriteLine("{0} (Count: {1})", m, count);

                CommitReadTransaction(td, tx);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                RollbackReadTransaction(td, tx);
                throw;
            }
        }

        protected void ReadItems(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
            {
                var temp = items.Skip(i).Take(td.CommitSize).ToList();

                var tx = BeginReadTransaction(td, db);

                try
                {
                    var m = td.Measure.MeasureTime("Read", i / td.CommitSize, td.CommitSize, () =>
                    {
                        for (int k = 0; k < temp.Count; k++)
                        {
                            var val = tx.Get(null, temp[k].Key);
                            var len = (int)val.Length;
                            System.Diagnostics.Debug.Assert(val.IsValid && len == temp[k].Value.Length, "Fail!");
                        }
                    });

                    Console.WriteLine(m);

                    CommitReadTransaction(td, tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RollbackReadTransaction(td, tx);
                    throw;
                }
            }
        }

        protected void SeekItems(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
            {
                var temp = items.Skip(i).Take(td.CommitSize).ToList();

                var tx = BeginReadTransaction(td, db);
                var cursor = tx.GetDataCursor(null);

                try
                {
                    var m = td.Measure.MeasureTime("Seek", i / td.CommitSize, td.CommitSize, () =>
                    {
                        for (int k = 0; k < temp.Count; k++)
                        {
                            var keyspan = new ReadOnlySpan<byte>(temp[k].Key);
                            var result = cursor.SetPositionEx(CursorPositions.Key, ref keyspan);
                            System.Diagnostics.Debug.Assert(result, "Key not found");
                        }
                    });

                    Console.WriteLine(m);

                    CommitReadTransaction(td, tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RollbackReadTransaction(td, tx);
                    throw;
                }
            }
        }

        protected void InsertItems(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
            {
                var temp = items.Skip(i).Take(td.CommitSize).ToList();

                var tx = BeginWriteTransaction(td, db);
                tx.AppendMode = false;

                try
                {
                    var m = td.Measure.MeasureTime("Insert", i / td.CommitSize, td.CommitSize, () =>
                    {
                        for (int k = 0; k < temp.Count; k++)
                        {
                            //Console.WriteLine(Tools.GetHexString(temp[k].Key));
                            tx.Insert(null, temp[k].Key, temp[k].Value);
                        }
                    });

                    Console.WriteLine(m);

                    CommitWriteTransaction(td, tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RollbackWriteTransaction(td, tx);
                    throw;
                }
            }
        }

        protected void UpdateItems(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
            {
                var temp = items.Skip(i).Take(td.CommitSize).ToList();

                var tx = BeginWriteTransaction(td, db);

                try
                {
                    var m = td.Measure.MeasureTime("Update", i / td.CommitSize, td.CommitSize, () =>
                    {
                        for (int k = 0; k < temp.Count; k++)
                        {
                            tx.Update(null, temp[k].Key, temp[k].Value);
                        }
                    });

                    Console.WriteLine(m);

                    CommitWriteTransaction(td, tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RollbackWriteTransaction(td, tx);
                    throw;
                }
            }
        }

        protected void DeleteItems(TestDescription td, Database db, List<KeyValuePair<byte[], byte[]>> items)
        {
            for (int i = 0; i < td.KeyCount; i += td.CommitSize)
            {
                var temp = items.Skip(i).Take(td.CommitSize).ToList();

                var tx = BeginWriteTransaction(td, db);

                try
                {
                    var m = td.Measure.MeasureTime("Delete", i / td.CommitSize, td.CommitSize, () =>
                    {
                        for (int k = 0; k < temp.Count; k++)
                        {
                            tx.Delete(null, temp[k].Key);
                        }
                    });

                    Console.WriteLine(m);

                    CommitWriteTransaction(td, tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RollbackWriteTransaction(td, tx);
                    throw;
                }
            }
        }

        protected Transaction BeginWriteTransaction(TestDescription td, Database db)
        {
            Transaction ret = null;

            var m = td.Measure.MeasureTime("BeginWT", 0, 1, () => { ret = db.BeginWriteTransaction(); });
            //Console.WriteLine(m);

            return ret;
        }

        protected void CommitWriteTransaction(TestDescription td, Transaction tx)
        {
            var m = td.Measure.MeasureTime("CommitWT", 0, 1, () => { tx.Commit(); });
            //Console.WriteLine(m);
        }

        protected void RollbackWriteTransaction(TestDescription td, Transaction tx)
        {
            var m = td.Measure.MeasureTime("RollbackWT", 0, 1, () => { tx.Rollback(); });
            //Console.WriteLine(m);
        }

        protected Transaction BeginReadTransaction(TestDescription td, Database db)
        {
            Transaction ret = null;

            var m = td.Measure.MeasureTime("BeginRT", 0, 1, () => { ret = db.BeginReadTransaction(); });
            //Console.WriteLine(m);

            return ret;
        }

        protected void CommitReadTransaction(TestDescription td, Transaction tx)
        {
            var m = td.Measure.MeasureTime("CommitRT", 0, 1, () => { tx.Commit(); });
            //Console.WriteLine(m);
        }

        protected void RollbackReadTransaction(TestDescription td, Transaction tx)
        {
            var m = td.Measure.MeasureTime("RollbackRT", 0, 1, () => { tx.Rollback(); });
            //Console.WriteLine(m);
        }

        #endregion
    }
}
