using System.Text.Json.Serialization;

namespace KeyValium.Frontends.MultiDictionary
{
    /// <summary>
    /// Represents the metadata for a KvDictionary.
    /// </summary>
    public class KvDictionaryInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        internal KvDictionaryInfo()
        {
            Perf.CallCount();
        }

        /// <summary>
        /// Name of the dictionary.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Name of the key type.
        /// </summary>
        [JsonInclude]
        public string KeyTypeName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Assembly name of the key type.
        /// </summary>
        [JsonInclude]
        public string KeyTypeAssemblyName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Name of the value type.
        /// </summary>
        [JsonInclude]
        public string ValueTypeName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Assembly name of the value type.
        /// </summary>
        [JsonInclude]
        public string ValueTypeAssemblyName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Name of the serializer type.
        /// </summary>
        [JsonInclude]
        public string SerializerTypeName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Assembly name of the serializer type.
        /// </summary>
        [JsonInclude]
        public string SerializerTypeAssemblyName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Name of the serializer options type if any.
        /// </summary>
        [JsonInclude]
        public string SerializerOptionsTypeName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Assembly name of the serializer type.
        /// </summary>
        [JsonInclude]
        public string SerializerOptionsTypeAssemblyName
        {
            get;
            internal set;
        }

        /// <summary>
        /// The serializer options.
        /// </summary>
        [JsonInclude]
        public object SerializerOptions
        {
            get;
            internal set;
        }
    }
}
