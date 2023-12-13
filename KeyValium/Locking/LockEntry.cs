using System.Buffers.Binary;
using System.Text;

namespace KeyValium.Locking
{
    unsafe class LockEntry
    {
        public LockEntry(Memory<byte> data, int index)
        {
            Perf.CallCount();

            if (data.Length != LockFile.ENTRY_SIZE)
            {
                throw new InvalidOperationException("Entry size mismatch.");
            }

            Index = index;
            _data = data;
        }

        public readonly int Index;

        internal Memory<byte> _data;

        /// <summary>
        /// 0x00 : 1 Byte Transaction Type
        ///        0x00 - None (free slot)
        ///        0x01 - Read
        ///        0x02 - Write
        /// </summary>
        public byte Type
        {
            get
            {
                Perf.CallCount();

                return _data.Span[0];
            }
            set
            {
                Perf.CallCount();

                _data.Span[0] = value;
            }
        }

        /// <summary>
        /// 0x04 : 4 Byte ProcessId
        /// </summary>
        public int ProcessId
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadInt32LittleEndian(_data.Slice(0x04).Span);
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteInt32LittleEndian(_data.Slice(0x04).Span, value);
            }
        }

        /// <summary>
        /// 0x08 : 8 Byte Oid
        /// </summary>
        public ulong Oid
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt64LittleEndian(_data.Slice(0x08).Span);
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt64LittleEndian(_data.Slice(0x08).Span, value);
            }
        }

        /// <summary>
        /// 0x10 : 16 Byte MachineId
        /// </summary>
        public Span<byte> MachineId
        {
            get
            {
                Perf.CallCount();

                return _data.Slice(0x10, 0x10).Span;
            }
        }

        /// <summary>
        /// 0x20 : 8 Byte Tid
        /// </summary>
        public KvTid Tid
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt64LittleEndian(_data.Slice(0x20).Span);
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt64LittleEndian(_data.Slice(0x20).Span, value);
            }
        }

        /// <summary>
        /// 0x28 : 8 Byte Expires
        /// </summary>
        public DateTime ExpiresUtc
        {
            get
            {
                Perf.CallCount();

                var utc= BinaryPrimitives.ReadInt64LittleEndian(_data.Slice(0x28).Span);
                return DateTime.FromFileTimeUtc(utc);
            }
            set
            {
                Perf.CallCount();

                var utc = value.ToFileTimeUtc();
                BinaryPrimitives.WriteInt64LittleEndian(_data.Slice(0x28).Span, utc);
            }
        }

        public override string ToString()
        {
            Perf.CallCount();

            var sb = new StringBuilder();

            sb.AppendFormat("Index: {0} ", Index);
            sb.AppendFormat("Type: {0} ", Type);
            sb.AppendFormat("MachineId: {0} ", Util.GetHexString(MachineId));
            sb.AppendFormat("ProcessId: {0} ", ProcessId);
            sb.AppendFormat("Oid: {0} ", Oid);
            sb.AppendFormat("Tid: {0} ", Tid);
            sb.AppendFormat("ExpiresUtc: {0:yyyy-MM-dd-HH:mm:ss}", ExpiresUtc);

            return sb.ToString();
        }
    }
}
