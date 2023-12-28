using KeyValium.Frontends;

namespace KeyValium.Samples.MultiDictionary
{
    public class Samples
    {
        public void Create()
        {
            using (var md = KvMultiDictionary.Open("MyDictionary.kvlm"))
            {
                // do stuff here
            }
        }

        public void Sample1()
        {
            // open or create the dictionary
            using (var md = KvMultiDictionary.Open("MyDictionaries.kvlm"))
            {
                // making sure the dictionary exists
                using (var dict = md.EnsureDictionary<string, string>("StringDictionary"))
                {
                    // adding some values
                    dict.Add("key1", "value1");
                    dict.Add("key2", "value2");
                    dict.Add("key3", "value3");

                    // reading
                    var val1 = dict["key1"];
                    var val2 = dict["key2"];
                    var val3 = dict["key3"];

                    // in a transaction
                    dict.DoInTransaction(() =>
                    {
                        dict["key1"] = "value100";
                        dict["key2"] = "value200";
                        dict["key3"] = "value300";
                    });

                    // deleting
                    if (dict.ContainsKey("key1"))
                    {
                        dict.Remove("key1");
                    }

                    if (dict.TryGetValue("key2", out val2))
                    {
                        dict.Remove("key2");
                    }

                    dict.Remove("key3");
                }
            }
        }
    }
}
