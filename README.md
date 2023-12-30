# KeyValium - A key-value-store for DotNet

KeyValium is a very fast key-value store for DotNet (currently DotNet 7 and 8 is supported). All data is stored in a recursive B+-tree as byte arrays.
A frontend is included that implements the IDictionary interface and allows multiple dictionaries in a single database file.

There are no dependencies.

## Features

* **Recursive B+-Tree:** Data is stored in a single file. Every key can be the root of another B+-tree.
* **Supported Pagesizes:** The following pagesizes are supported: 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536. 
Maximum key size depends on the page size (usually 1/4 of it).
* **Transactions:** One writer, multiple readers, nested transactions.
* **Stream Support:** Values greater then 2 GB are supported via stream interface.
* **Multiple sharing modes:** 
    + **Exclusive**: Only one instance can be opened.
    + **SharedLocal**: Multiple instances can be opened on the same machine.
    + **SharedNetwork**: Multiple instances can be opened on different machines.
* **Count support:** Every tree keeps a local and a total count of keys.
* **Frontends:** A MultiDictionary which manages multiple persistent dictionaries in one database file. More frontends are possible.
* **Encryption:** The database can be encrypted with AES via password and/or a keyfile.


# Documentation

See the Wiki for documentation.

# Usage

## Using KeyValium directly

### Data storage and retrieval

```C#
using KeyValium;
using KeyValium.Options;

...

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

```

### Working with sub trees (TreeRefs)

```C#
using KeyValium;
using KeyValium.Options;

...

static Encoding encoding = Encoding.UTF8;

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
```

## Using the MultiDictionary Frontend

```C#
using KeyValium.Frontends;

...

public void Sample1()
{
    // open or create the multidictionary with default settings
    using (var md = KvMultiDictionary.Open("MyDictionaries.kvlm"))
    {
        // making sure the dictionary exists
        using (var dict = md.EnsureDictionary<string, string>("StringDictionary"))
        {
            // adding some values (uses one transaction per call)
            dict.Add("key1", "value1");
            dict.Add("key2", "value2");
            dict.Add("key3", "value3");

            // reading values (uses one transaction per call)
            var val1 = dict["key1"];
            var val2 = dict["key2"];
            var val3 = dict["key3"];

            // doing multiple actions in one transaction
            dict.Do(() =>
            {
                dict["key1"] = "value100";
                dict["key2"] = "value200";
                dict["key3"] = "value300";

                var val1 = dict["key1"];
                var val2 = dict["key2"];
                var val3 = dict["key3"];
            });

            // check if key exists
            if (dict.ContainsKey("key1"))
            {
                // delete key and value
                dict.Remove("key1");
            }

            // trying to get a value
            if (dict.TryGetValue("key2", out val2))
            {
                dict.Remove("key2");
            }

            // delete key and value
            var isdeleted = dict.Remove("key3");
        }
    }
}

```

# Changelog

See the release notes.

# License

[Apache 2.0](https://opensource.org/license/apache-2-0)






