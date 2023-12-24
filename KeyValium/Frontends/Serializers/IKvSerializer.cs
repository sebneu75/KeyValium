using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyValium.Frontends.Serializers
{
    public interface IKvSerializer
    {
        object Options
        {
            get;
        }

        void SetOptions(JsonElement options);

        byte[] Serialize<T>(T data, bool isvalue);

        T Deserialize<T>(ReadOnlySpan<byte> data, bool isvalue);
    }
}
