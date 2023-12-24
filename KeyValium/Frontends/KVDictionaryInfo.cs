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
        public string KeyTypeName
        {
            get;            
            internal set;
        }

        [JsonInclude]
        public string KeyTypeAssemblyName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string ValueTypeName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string ValueTypeAssemblyName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string SerializerTypeName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string SerializerTypeAssemblyName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string SerializerOptionsTypeName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public string SerializerOptionsTypeAssemblyName
        {
            get;
            internal set;
        }

        [JsonInclude]
        public object SerializerOptions
        {
            get;
            internal set;
        }
    }
}
