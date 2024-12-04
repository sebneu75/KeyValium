using System;
using System.Buffers.Binary;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeyValium.Frontends.Serializers
{
    internal class KvSpanSerializer
    {
        public KvSpanSerializer()
        {
            Perf.CallCount();

            _encoding = new UTF8Encoding(false, true);

            _buffer = new byte[1024 * 1024];

            _current = 0;
        }

        private readonly Encoding _encoding;

        private readonly byte[] _buffer;
        private readonly int[] _bufferdec = new int[4];

        private int _current = 0;

        #region Serialization

        public void Reset()
        {
            _current = 0;
        }

        public ReadOnlySpan<byte> Return(int size)
        {
            var ret = _buffer.AsSpan().Slice(_current, size);
            _current += size;
            return ret;
        }

        /// <summary>
        /// Serializes data 
        /// All data is serialized to the same internal buffer. The returned span must be copied or used before the next call to Serialize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public ReadOnlySpan<byte> Serialize<T>(T data)
        {
            Perf.CallCount();

            var span = _buffer.AsSpan().Slice(_current);

            switch (data) // Since C# 7.0, any type is supported here
            {
                // byte arrays
                case byte[] val:
                    return val;

                // integral types
                case sbyte val:
                    span[0] = (byte)val;
                    return Return(sizeof(sbyte));

                case byte val:
                    span[0] = val;
                    return span.Slice(0, sizeof(byte));

                case short val:
                    BinaryPrimitives.WriteInt16LittleEndian(span, val);
                    return Return(sizeof(short));

                case ushort val:
                    BinaryPrimitives.WriteUInt16LittleEndian(span, val);
                    return Return(sizeof(ushort));

                case int val:
                    BinaryPrimitives.WriteInt32LittleEndian(span, val);
                    return Return(sizeof(int));

                case uint val:
                    BinaryPrimitives.WriteUInt32LittleEndian(span, val);
                    return Return(sizeof(uint));

                case long val:
                    BinaryPrimitives.WriteInt64LittleEndian(span, val);
                    return Return(sizeof(long));

                case ulong val:
                    BinaryPrimitives.WriteUInt64LittleEndian(span, val);
                    return Return(sizeof(ulong));

#if NET8_0_OR_GREATER

                case Int128 val:
                    BinaryPrimitives.WriteInt128LittleEndian(span, val);
                    return Return(2 * sizeof(long));

                case UInt128 val:
                    BinaryPrimitives.WriteUInt128LittleEndian(span, val);
                    return Return(2 * sizeof(ulong));
#endif

                // floating point types
                case Half val:
                    BinaryPrimitives.WriteHalfLittleEndian(span, val);
                    return Return(sizeof(ushort));

                case float val:
                    BinaryPrimitives.WriteSingleLittleEndian(span, val);
                    return Return(sizeof(float));

                case double val:
                    BinaryPrimitives.WriteDoubleLittleEndian(span, val);
                    return Return(sizeof(double));

                case decimal val:
                    Decimal.GetBits(val, _bufferdec);
                    BinaryPrimitives.WriteInt32LittleEndian(span.Slice(0 * sizeof(int)), _bufferdec[0]);
                    BinaryPrimitives.WriteInt32LittleEndian(span.Slice(1 * sizeof(int)), _bufferdec[1]);
                    BinaryPrimitives.WriteInt32LittleEndian(span.Slice(2 * sizeof(int)), _bufferdec[2]);
                    BinaryPrimitives.WriteInt32LittleEndian(span.Slice(3 * sizeof(int)), _bufferdec[3]);

                    // decimal cloneDecimal = new Decimal(bits);
                    return Return(4 * sizeof(int));

                // DateTime and Timespan
                case DateTime val:
                    BinaryPrimitives.WriteInt64LittleEndian(span, val.ToUniversalTime().Ticks);
                    return Return(sizeof(long));

                case TimeSpan val:
                    BinaryPrimitives.WriteInt64LittleEndian(span, val.Ticks);
                    return Return(sizeof(long));

                // string and char
                case char val:
                    BinaryPrimitives.WriteUInt16LittleEndian(span, val);
                    return Return(sizeof(ushort));

                case string val:

                    if (val == "")
                    {
                        span[0] = 0x80;
                        return Return(1);
                    }
                    else
                    {
                        // TODO check what happens on overflow
                        var len = _encoding.GetBytes(val, span);
                        return Return(len);
                    }

                // null
                case null:
                    return Return(0);

                // default
                default:
                    throw new NotSupportedException("The type is not supported.");
            }
        }

        #endregion

        #region Deserialization

        public T Deserialize<T>(ReadOnlySpan<byte> data)
        {
            Perf.CallCount();

            if (data.Length == 0)
            {
                return default;
            }

            var type = typeof(T);

            if (type == typeof(byte[]))
            {
                return (T)(object)data.ToArray();
            }
            else if (type == typeof(sbyte))
            {
                return (T)(object)data[0];
            }
            else if (type == typeof(byte))
            {
                return (T)(object)data[0];
            }
            else if (type == typeof(short))
            {
                return (T)(object)BinaryPrimitives.ReadInt16LittleEndian(data);
            }
            else if (type == typeof(ushort))
            {
                return (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(data);
            }
            else if (type == typeof(int))
            {
                return (T)(object)BinaryPrimitives.ReadInt32LittleEndian(data);
            }
            else if (type == typeof(uint))
            {
                return (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(data);
            }
            else if (type == typeof(long))
            {
                return (T)(object)BinaryPrimitives.ReadInt64LittleEndian(data);
            }
            else if (type == typeof(ulong))
            {
                return (T)(object)BinaryPrimitives.ReadUInt64LittleEndian(data);
            }
#if NET8_0_OR_GREATER
            else if (type == typeof(Int128))
            {
                return (T)(object)BinaryPrimitives.ReadInt128LittleEndian(data);
            }
            else if (type == typeof(UInt128))
            {
                return (T)(object)BinaryPrimitives.ReadUInt128LittleEndian(data);
            }
#endif
            else if (type == typeof(Half))
            {
                return (T)(object)BinaryPrimitives.ReadHalfLittleEndian(data);
            }
            else if (type == typeof(float))
            {
                return (T)(object)BinaryPrimitives.ReadSingleLittleEndian(data);
            }
            else if (type == typeof(double))
            {
                return (T)(object)BinaryPrimitives.ReadDoubleLittleEndian(data);
            }
            else if (type == typeof(decimal))
            {
                _bufferdec[0] = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(0 * sizeof(int)));
                _bufferdec[1] = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(1 * sizeof(int)));
                _bufferdec[2] = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(2 * sizeof(int)));
                _bufferdec[3] = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(3 * sizeof(int)));

                return (T)(object) new decimal(_bufferdec);
            }
            else if (type == typeof(DateTime))
            {
                return (T)(object)new DateTime(BinaryPrimitives.ReadInt64LittleEndian(data), DateTimeKind.Utc);
            }
            else if (type == typeof(TimeSpan))
            {
                return (T)(object)new TimeSpan(BinaryPrimitives.ReadInt64LittleEndian(data));
            }
            else if (type == typeof(char))
            {
                return (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(data);
            }
            else if (type == typeof(string))
            {
                if (data.Length == 1 && data[0] == 0x80)
                {
                    return (T)(object)"";
                }

                return (T)(object)_encoding.GetString(data);
            }
            else
            {
                throw new NotSupportedException("The type is not supported.");
            }
        }

        #endregion
    }
}

