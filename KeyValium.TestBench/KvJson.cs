using KeyValium.TestBench.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    public class KvJson
    {
        static JsonSerializerOptions _jsonoptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
             
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
        };

        public static void Save<T>(T item, string path)
        {
            var json = JsonSerializer.Serialize(item, _jsonoptions);

            using (var writer = new StreamWriter(path))
            {
                writer.Write(json);
            }
        }

        public static T Load<T>(string path)
        {
            var json = "";

            using (var reader = new StreamReader(path))
            {
                json = reader.ReadToEnd();
            }

            return JsonSerializer.Deserialize<T>(json, _jsonoptions);
        }
    }
}
