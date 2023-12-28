using System;
using System.Buffers.Binary;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyValium.Frontends.Serializers
{
    internal class KvSerializer
    {
        public KvSerializer()
        {
            Perf.CallCount();

            _jsonoptions = new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = false,
            };

            _encoding = new UTF8Encoding(false, true);
        }

        private readonly Encoding _encoding;

        private readonly JsonSerializerOptions _jsonoptions;

        #region Serialization

        public byte[] Serialize<T>(T data, bool zip = false)
        {
            Perf.CallCount();

            byte[] ret;

            switch (data) // Since C# 7.0, any type is supported here
            {
                case byte[] val:
                    ret = val;
                    break;

                // integral types
                case sbyte val:
                    ret = GetBytes(val);
                    break;
                case byte val:
                    ret = GetBytes(val);
                    break;
                case short val:
                    ret = GetBytes(val);
                    break;
                case ushort val:
                    ret = GetBytes(val);
                    break;
                case int val:
                    ret = GetBytes(val);
                    break;
                case uint val:
                    ret = GetBytes(val);
                    break;
                case long val:
                    ret = GetBytes(val);
                    break;
                case ulong val:
                    ret = GetBytes(val);
                    break;

                // floating point types
                case Half val:
                    ret = GetBytes(val);
                    break;
                case float val:
                    ret = GetBytes(val);
                    break;
                case double val:
                    ret = GetBytes(val);
                    break;
                //case decimal val:
                //    ret = GetBytes(val);
                //    break;

                // DateTime and Timespan
                case DateTime val:
                    ret = GetBytes(val);
                    break;
                case TimeSpan val:
                    ret = GetBytes(val);
                    break;

                // string and char
                case char val:
                    ret = GetBytes(val);
                    break;
                case string val:
                    ret = GetBytes(val);
                    break;

                // null
                case null:
                    ret = null;
                    break;

                // default
                default:
                    ret = JsonSerializer.SerializeToUtf8Bytes(data, _jsonoptions);
                    break;
            }


            //var type = typeof(T);

            //if (type == typeof(byte[]))
            //{
            //    ret = data as byte[];
            //}
            //else if (type == typeof(DateTime))
            //{
            //    ret = GetBytes(data as DateTime?);
            //}
            //else if (type == typeof(long))
            //{
            //    ret = GetBytes(data as long?);
            //}
            //else if (type == typeof(string))
            //{
            //    ret = GetBytes(data as string);
            //}
            //else if (type == typeof(ulong))
            //{
            //    ret = GetBytes(data as ulong?);
            //}
            //else
            //{
            //    //var ms = new MemoryStream();
            //    //JsonSerializer.Serialize<T>(ms, data, _jsonoptions);
            //    return JsonSerializer.SerializeToUtf8Bytes<T>(data, _jsonoptions);
            //    //ret = ms.ToArray();
            //}

            if (zip)
            {
                ret = ZipBytes(ret, CompressionLevel.Fastest);
            }

            return ret;
        }

        private byte[] GetBytes(sbyte val)
        {
            Perf.CallCount();

            return new byte[1] { (byte)val };
        }

        private byte[] GetBytes(byte val)
        {
            Perf.CallCount();

            return new byte[1] { val };
        }

        private byte[] GetBytes(short val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(ushort val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(int val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(uint val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(long val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(ulong val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(Half val)
        {
            Perf.CallCount();

            var ret = new byte[2];
            BinaryPrimitives.WriteHalfBigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(float val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(double val)
        {
            Perf.CallCount();

            var ret = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(ret, val);
            return ret;
        }

        private byte[] GetBytes(DateTime val)
        {
            Perf.CallCount();

            return GetBytes(val.ToUniversalTime().Ticks);
        }

        private byte[] GetBytes(TimeSpan val)
        {
            Perf.CallCount();

            return GetBytes(val.Ticks);
        }

        private byte[] GetBytes(char val)
        {
            Perf.CallCount();

            return GetBytes((ushort)val);
        }

        private byte[] GetBytes(string val)
        {
            Perf.CallCount();

            // special case for empty string needed for differentiation between null and ""
            // 0x80 is an invalid UTF8 code
            if (val == "")
            {
                return new byte[1] { 0x80 };
            }

            return _encoding.GetBytes(val);
        }

        #endregion

        #region Deserialization

        public T Deserialize<T>(ReadOnlySpan<byte> data, bool unzip = false)
        {
            Perf.CallCount();

            if (data.Length == 0)
            {
                return default;
            }

            if (unzip)
            {
                data = UnzipBytes(data.ToArray());
            }

            var type = typeof(T);

            if (type == typeof(byte[]))
            {
                return (T)(object)data.ToArray();
            }
            else if (type == typeof(sbyte))
            {
                return (T)(object)GetSByte(data);
            }
            else if (type == typeof(byte))
            {
                return (T)(object)GetByte(data);
            }
            else if (type == typeof(short))
            {
                return (T)(object)GetShort(data);
            }
            else if (type == typeof(ushort))
            {
                return (T)(object)GetUShort(data);
            }
            else if (type == typeof(int))
            {
                return (T)(object)GetInt(data);
            }
            else if (type == typeof(uint))
            {
                return (T)(object)GetUInt(data);
            }
            else if (type == typeof(long))
            {
                return (T)(object)GetLong(data);
            }
            else if (type == typeof(ulong))
            {
                return (T)(object)GetULong(data);
            }
            else if (type == typeof(Half))
            {
                return (T)(object)GetHalf(data);
            }
            else if (type == typeof(float))
            {
                return (T)(object)GetFloat(data);
            }
            else if (type == typeof(double))
            {
                return (T)(object)GetDouble(data);
            }
            //else if (type == typeof(decimal))
            //{
            //    return (T)(object)GetDecimal(data);
            //}
            else if (type == typeof(DateTime))
            {
                return (T)(object)GetDateTime(data);
            }
            else if (type == typeof(TimeSpan))
            {
                return (T)(object)GetTimeSpan(data);
            }
            else if (type == typeof(char))
            {
                return (T)(object)GetChar(data);
            }
            else if (type == typeof(string))
            {
                return (T)(object)GetString(data);
            }
            else
            {
                return JsonSerializer.Deserialize<T>(data, _jsonoptions);
            }

            //if (type == typeof(byte[]))
            //{
            //    return (T)((object)data.ToArray());
            //}
            //else if (type == typeof(DateTime))
            //{
            //    return (T)GetDateTime(data);
            //}
            //else if (type == typeof(long))
            //{
            //    return (T)GetLong(data);
            //}
            //else if (type == typeof(string))
            //{
            //    return (T)GetString(data);
            //}
            //else if (type == typeof(ulong))
            //{
            //    return (T)GetULong(data);
            //}
            //else
            //{
            //    var ms = new MemoryStream(data);
            //    return JsonSerializer.Deserialize<T>(ms, _jsonoptions);
            //}
        }

        private sbyte GetSByte(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return (sbyte)data[0];
        }

        private byte GetByte(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return data[0];
        }

        private short GetShort(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadInt16BigEndian(data);
        }

        private ushort GetUShort(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadUInt16BigEndian(data);
        }

        private int GetInt(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadInt32BigEndian(data);
        }

        private uint GetUInt(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadUInt32BigEndian(data);
        }

        private long GetLong(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadInt64BigEndian(data);
        }

        private ulong GetULong(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadUInt64BigEndian(data);
        }

        private Half GetHalf(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadHalfBigEndian(data);
        }

        private float GetFloat(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadSingleBigEndian(data);
        }

        private double GetDouble(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            return BinaryPrimitives.ReadDoubleBigEndian(data);
        }

        private DateTime GetDateTime(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            var val = GetLong(data);

            return new DateTime(val, DateTimeKind.Utc);
        }

        private TimeSpan GetTimeSpan(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            var val = GetLong(data);

            return new TimeSpan(val);
        }

        private char GetChar(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            var val = GetUShort(data);

            return (char)val;
        }

        private string GetString(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            // special case for empty string
            // 0x80 is an invalid UTF8 code
            if (data.Length == 1 && data[0] == 0x80)
            {
                return "";
            }

            return _encoding.GetString(data);
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

