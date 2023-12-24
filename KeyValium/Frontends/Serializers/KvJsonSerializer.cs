using System;
using System.Buffers.Binary;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyValium.Frontends.Serializers
{
    public class KvJsonSerializer : IKvSerializer
    {
        public KvJsonSerializer() : this(new KvJsonSerializerOptions())
        {
        }

        public KvJsonSerializer(KvJsonSerializerOptions options)
        {
            Perf.CallCount();

            _options = options;
        }

        private KvJsonSerializerOptions _options;

        public object Options
        {
            get
            {
                return _options;
            }
        }

        public void SetOptions(JsonElement options)
        {
            var newoptions = JsonSerializer.Deserialize<KvJsonSerializerOptions>(options);

            if (newoptions.ZipValues != _options.ZipValues)
            {
                var msg = string.Format("Changing ZipValues is not supported. (Current value is '{0}')", _options.ZipValues);
                throw new KeyValiumException(ErrorCodes.InvalidParameter, msg);
            }

            _options = newoptions;
        }

        #region Serialization

        public byte[] Serialize<T>(T data, bool isvalue)
        {
            Perf.CallCount();

            var ret = JsonSerializer.SerializeToUtf8Bytes(data, _options.JsonOptions);

            if (isvalue && _options.ZipValues)
            {
                ret = ZipBytes(ret, CompressionLevel.Fastest);
            }

            return ret;
        }

        #endregion

        #region Deserialization

        public T Deserialize<T>(ReadOnlySpan<byte> data, bool isvalue)
        {
            Perf.CallCount();

            if (isvalue && _options.ZipValues)
            {
                data = UnzipBytes(data.ToArray());
            }

            return JsonSerializer.Deserialize<T>(data, _options.JsonOptions);
        }

        #endregion

        #region Compression

        private static byte[] ZipBytes(byte[] data, CompressionLevel level)
        {
            Perf.CallCount();

            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, level, true))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }

        private static byte[] UnzipBytes(byte[] data)
        {
            Perf.CallCount();

            using (var ms = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress, true))
                {
                    using (var reader = new MemoryStream())
                    {
                        gzip.CopyTo(reader);
                        return reader.ToArray();
                    }
                }
            }
        }

        #endregion
    }
}

