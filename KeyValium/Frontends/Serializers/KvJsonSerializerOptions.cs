using System;
using System.Buffers.Binary;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyValium.Frontends.Serializers
{
    public class KvJsonSerializerOptions
    {
        public KvJsonSerializerOptions()
        {
            Perf.CallCount();

            ZipValues = false;

            JsonOptions = new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
            };
        }

        internal readonly JsonSerializerOptions JsonOptions;

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

        public bool ZipValues
        {
            get;
            set;
        }
    }
}

