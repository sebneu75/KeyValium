using KeyValium.Frontends.TreeArray;
using System.Buffers.Binary;
using System.Text;

namespace KeyValium.Frontends.Serializers
{
    internal class KvArraySerializer
    {
        public KvArraySerializer(uint maxkeylength)
        {
            Perf.CallCount();

            MaxKeyLength = (int)maxkeylength;

            _encoding = new UTF8Encoding(false, true);

            _buffer = new byte[maxkeylength * KvArrayTx.MaxKeys];
        }

        private readonly int MaxKeyLength;

        private readonly Encoding _encoding;

        private readonly byte[] _buffer;

        #region Serialization

        #endregion

        #region Deserialization

        internal ReadOnlyMemory<byte>[] GetPathBytes(ref KvArrayKey[] indices)
        {
            if (indices.Length <= 1)
            {
                return null;
            }

            var mem = _buffer.AsMemory();

            var ret = new ReadOnlyMemory<byte>[indices.Length - 1];

            for (int i = 0; i < indices.Length - 1; i++)
            {
                ret[i] = SerializeKey(ref mem, ref indices[i]);
                mem = mem.Slice(ret[i].Length);
            }

            return ret;
        }

        internal ReadOnlyMemory<byte> GetKeyBytes(ref KvArrayKey[] indices)
        {
            var mem = _buffer.AsMemory();

            var ret = SerializeKey(ref mem, ref indices[indices.Length - 1]);

            return ret;
        }

        private ReadOnlyMemory<byte> SerializeKey(ref Memory<byte> mem, ref KvArrayKey key)
        {
            var span = mem.Span;

            BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)key.Flags); // 2 bytes flags
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(sizeof(ushort)), (ushort)key.Type); // 2 bytes type

            var len = sizeof(ushort) + sizeof(ushort);

            var tempspan = span.Slice(len);

            switch (key.Type)
            {
                case KvArrayTypes.Long:
                    BinaryPrimitives.WriteInt64BigEndian(tempspan, key.LongValue);
                    len += sizeof(long);
                    break;
                case KvArrayTypes.String:
                    // TODO check what happens on overflow
                    len += _encoding.GetBytes(key.StringValue, tempspan);
                    break;
                default:
                    throw new NotSupportedException("Unhandled key type.");
            }

            return mem.Slice(0, len);
        }

        private KvArrayKey DeserializeKey(ref ReadOnlySpan<byte> span)
        {
            var flags = (KvArrayFlags)BinaryPrimitives.ReadUInt16BigEndian(span); // 2 bytes flags
            var type = (KvArrayTypes)BinaryPrimitives.ReadUInt16BigEndian(span.Slice(sizeof(ushort))); // 2 bytes type

            var len = sizeof(ushort) + sizeof(ushort);

            var tempspan = span.Slice(len);

            switch (type)
            {
                case KvArrayTypes.Long:
                    var longval = BinaryPrimitives.ReadInt64BigEndian(tempspan);
                    return new KvArrayKey(longval);

                case KvArrayTypes.String:
                    // TODO check what happens on overflow
                    var strval = _encoding.GetString(tempspan);
                    return new KvArrayKey(strval);

                default:
                    throw new NotSupportedException("Unhandled key type.");
            }
        }

        //internal KvArrayValue DeserializeValue(ref ValueRef val)
        //{
        //    ReadPrefix(ref val, out var prefixlen, out var kind);

        //}

        private void ReadPrefix(ref ValueRef val, out ushort prefixlen, out KvArrayTypes kind)
        {
            if (val.IsInlineValue)
            {
                prefixlen = BinaryPrimitives.ReadUInt16LittleEndian(val.ValueSpan);
                if (prefixlen != sizeof(ushort))
                {
                    throw new NotSupportedException("Prefixlen other than 2 not supported.");
                }

                var prefix = BinaryPrimitives.ReadUInt16LittleEndian(val.ValueSpan.Slice(sizeof(ushort)));
                kind = (KvArrayTypes)prefix;
            }
            else
            {
                var buffer = new byte[sizeof(ushort)];
                val.ValueStream.ReadExactly(buffer);

                prefixlen = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                if (prefixlen != sizeof(ushort))
                {
                    throw new NotSupportedException("Prefixlen other than 2 not supported.");
                }

                val.ValueStream.ReadExactly(buffer);
                var prefix = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                kind = (KvArrayTypes)prefix;


            }
        }

        #endregion
    }
}

