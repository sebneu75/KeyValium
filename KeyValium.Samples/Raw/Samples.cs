using KeyValium.Options;
using System.Text;

namespace KeyValium.Samples.Raw
{
    public class Samples
    {
        /// <summary>
        /// Creating a database with default options.
        /// </summary>
        public void CreateDatabase()
        {
            // the database will be created if it does not exist
            using (var db = Database.Open("sample0.kvlm"))
            {
                // do some stuff
            }
        }

        /// <summary>
        /// Creating a database with options.
        /// </summary>
        public void CreateDatabase2()
        {
            var options = new DatabaseOptions();

            options.CacheSizeMB = 256;
            options.PageSize = 16384;
            options.UserTypeCode = 0xc0de;

            // the database will be created if it does not exist
            using (var db = Database.Open("sample1.kvlm", options))
            {
                // do some stuff
            }
        }

        static Encoding encoding = Encoding.UTF8;

        public void Sample1()
        {
            // open or create a database with default options
            using (var db = Database.Open("sample1.kvlm"))
            {
                // insert some data
                using (var tx = db.BeginWriteTransaction())
                {
                    tx.Insert(null, encoding.GetBytes("Key1"), encoding.GetBytes("Value1"));
                    tx.Insert(null, encoding.GetBytes("Key2"), encoding.GetBytes("Value2"));
                    tx.Insert(null, encoding.GetBytes("Key3"), encoding.GetBytes("Value3"));

                    tx.Commit();
                }

                // read data
                using (var tx = db.BeginReadTransaction())
                {
                    Display(tx.Get(null, encoding.GetBytes("Key1")));
                    Display(tx.Get(null, encoding.GetBytes("Key2")));
                    Display(tx.Get(null, encoding.GetBytes("Key3")));
                }

                // update data
                using (var tx = db.BeginWriteTransaction())
                {
                    tx.Update(null, encoding.GetBytes("Key1"), encoding.GetBytes("Value100"));
                    tx.Update(null, encoding.GetBytes("Key2"), encoding.GetBytes("Value200"));
                    tx.Update(null, encoding.GetBytes("Key3"), encoding.GetBytes("Value300"));

                    tx.Commit();
                }

                // read data
                using (var tx = db.BeginReadTransaction())
                {
                    Display(tx.Get(null, encoding.GetBytes("Key1")));
                    Display(tx.Get(null, encoding.GetBytes("Key2")));
                    Display(tx.Get(null, encoding.GetBytes("Key3")));
                }

                // iterate over data
                using (var tx = db.BeginReadTransaction())
                {
                    // iterate using lambda
                    tx.ForEach(null, (item) => 
                    {
                        Display(item.Value);
                        return true; 
                    });

                    // iterate using delegate and inner function
                    tx.ForEach(null, Iterator);

                    bool Iterator(ref ValueRef val)
                    {
                        Display(val);
                        return true;
                    }

                    // iterate using foreach
                    foreach (var item in tx.GetIterator(null, true))
                    {
                        Display(item.Value);
                    }
                }

                // delete data
                using (var tx = db.BeginWriteTransaction())
                {
                    tx.Delete(null, encoding.GetBytes("Key1"));
                    tx.Delete(null, encoding.GetBytes("Key2"));
                    tx.Delete(null, encoding.GetBytes("Key3"));

                    tx.Commit();
                }

                // read data
                using (var tx = db.BeginReadTransaction())
                {
                    Display(tx.Get(null, encoding.GetBytes("Key1")));
                    Display(tx.Get(null, encoding.GetBytes("Key2")));
                    Display(tx.Get(null, encoding.GetBytes("Key3")));
                }
            }
        }

        static void Display(ValueRef valref)
        {
            if (valref.IsValid)
            {
                Console.WriteLine("Key='{0}', Value ='{1}'", encoding.GetString(valref.Key), encoding.GetString(valref.ValueSpan));
            }
            else
            {
                Console.WriteLine("Invalid ValueRef.");
            }
        }

        /// <summary>
        /// working with subtrees (TreeRefs)
        /// </summary>
        public void Sample2()
        {
            // open or create a database with default options
            using (var db = Database.Open("sample2.kvlm"))
            {
                TreeRef treeref1;
                TreeRef treeref2;
                TreeRef treeref3;

                // inserting some keys with subtree flag
                using (var tx = db.BeginWriteTransaction())
                {
                    // makes sure the key exists and has the subtree flag set
                    treeref1 = tx.EnsureTreeRef(TrackingScope.Database, encoding.GetBytes("Root1"));
                    treeref2 = tx.EnsureTreeRef(TrackingScope.Database, encoding.GetBytes("Root2"));
                    treeref3 = tx.EnsureTreeRef(TrackingScope.Database, encoding.GetBytes("Root3"));

                    tx.Commit();
                }

                // inserting data in subtrees
                using (var tx = db.BeginWriteTransaction())
                {
                    // TreeRefs are tracked beyond transaction boundaries. No need to create them again.

                    tx.Insert(treeref1, encoding.GetBytes("Root1-Key1"), encoding.GetBytes("Root1-Value1"));
                    tx.Insert(treeref1, encoding.GetBytes("Root1-Key2"), encoding.GetBytes("Root1-Value2"));
                    tx.Insert(treeref1, encoding.GetBytes("Root1-Key3"), encoding.GetBytes("Root1-Value3"));

                    tx.Insert(treeref2, encoding.GetBytes("Root2-Key1"), encoding.GetBytes("Root2-Value1"));
                    tx.Insert(treeref2, encoding.GetBytes("Root2-Key2"), encoding.GetBytes("Root2-Value2"));
                    tx.Insert(treeref2, encoding.GetBytes("Root2-Key3"), encoding.GetBytes("Root2-Value3"));

                    tx.Insert(treeref3, encoding.GetBytes("Root3-Key1"), encoding.GetBytes("Root3-Value1"));
                    tx.Insert(treeref3, encoding.GetBytes("Root3-Key2"), encoding.GetBytes("Root3-Value2"));
                    tx.Insert(treeref3, encoding.GetBytes("Root3-Key3"), encoding.GetBytes("Root3-Value3"));

                    tx.Commit();
                }

                // reading data from subtrees
                using (var tx = db.BeginReadTransaction())
                {
                    Display(tx.Get(treeref1, encoding.GetBytes("Root1-Key1")));
                    Display(tx.Get(treeref1, encoding.GetBytes("Root1-Key2")));
                    Display(tx.Get(treeref1, encoding.GetBytes("Root1-Key3")));

                    Display(tx.Get(treeref2, encoding.GetBytes("Root2-Key1")));
                    Display(tx.Get(treeref2, encoding.GetBytes("Root2-Key2")));
                    Display(tx.Get(treeref2, encoding.GetBytes("Root2-Key3")));

                    Display(tx.Get(treeref3, encoding.GetBytes("Root3-Key1")));
                    Display(tx.Get(treeref3, encoding.GetBytes("Root3-Key2")));
                    Display(tx.Get(treeref3, encoding.GetBytes("Root3-Key3")));

                    // getting count of keys in the root tree excluding its subtrees
                    var localcount = tx.GetLocalCount(null);    // 3 keys in  root tree

                    // getting total count of keys in the root tree and its subtrees
                    var globalcount = tx.GetTotalCount(null);   // 12 keys total. 3 in root tree and 3 in each subtree.

                    // getting count of keys in a specific subtree excluding its subtrees
                    var localcount1 = tx.GetLocalCount(treeref1);   // 3 keys in subtree.

                    // getting count of keys in a specific subtree including its subtrees
                    var globalcount1 = tx.GetTotalCount(treeref1);  // 3 keys total. There are no subtrees.
                }

                // deleting subtrees
                using (var tx = db.BeginWriteTransaction())
                {
                    tx.DeleteTree(treeref1);
                    tx.DeleteTree(treeref2);
                    tx.DeleteTree(treeref3);

                    tx.Commit();
                }
            }
        }
    }
}
