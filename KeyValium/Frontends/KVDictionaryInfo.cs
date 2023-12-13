using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KeyValium.Frontends
{
    public class KvDictionaryInfo
    {
        public KvDictionaryInfo() 
        {
            Perf.CallCount();
        }

        [JsonIgnore]
        public string Name
        {
            get; 
            internal set;
        }

        [JsonInclude]
        public string KeyType
        {
            get;            
            internal set;
        }

        [JsonInclude]
        public string ValueType
        {
            get;
            internal set;
        }

        [JsonInclude]
        public bool ZipValues
        {
            get;
            internal set;
        }
    }
}
