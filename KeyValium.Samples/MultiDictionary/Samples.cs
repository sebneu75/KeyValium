﻿using KeyValium.Frontends.MultiDictionary;

namespace KeyValium.Samples.MultiDictionary
{
    public class Samples
    {
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

                    // foreach loops currently require an explicit transaction
                    dict.Do(() =>
                    {
                        // iterate over key value pairs
                        foreach (var item in dict) 
                        {
                            Console.WriteLine("{0}: {1}", item.Key, item.Value);
                        }

                        // iterate over keys
                        foreach (var key in dict.Keys)
                        {
                            Console.WriteLine("Key: {0}", key);
                        }

                        // iterate over values
                        foreach (var val in dict.Values)
                        {
                            Console.WriteLine("Value: {0}", val);
                        }
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
    }
}
