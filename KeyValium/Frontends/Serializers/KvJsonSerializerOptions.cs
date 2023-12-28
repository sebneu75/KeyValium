using System;
using System.Buffers.Binary;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyValium.Frontends.Serializers
{
    /// <summary>
    /// Options class for the KvJsonSerializer
    /// </summary>
    public class KvJsonSerializerOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public KvJsonSerializerOptions()
        {
            Perf.CallCount();

            ZipValues = false;

            JsonOptions = new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = false,
            };
        }

        internal readonly JsonSerializerOptions JsonOptions;

        /// <summary>
        /// Should fields be included. The default is false.
        /// </summary>
        public bool IncludeFields
        {
            get
            {
                return JsonOptions.IncludeFields;
            }
            set
            {
                JsonOptions.IncludeFields = value;
            }
        }

        /// <summary>
        /// Should references be preserved. The default is true.
        /// </summary>
        public bool PreserveReferences
        {
            get
            {
                return JsonOptions.ReferenceHandler != null;
            }
            set
            {
                JsonOptions.ReferenceHandler = value ? ReferenceHandler.Preserve : null;
            }
        }

        /// <summary>
        /// Should the Json be indented. The default is false.
        /// </summary>
        public bool WriteIndented
        {
            get
            {
                return JsonOptions.WriteIndented;
            }
            set
            {
                JsonOptions.WriteIndented = value;
            }
        }

        /// <summary>
        /// Should the values be zipped. The default is false.
        /// </summary>
        public bool ZipValues
        {
            get;
            set;
        }
    }
}

