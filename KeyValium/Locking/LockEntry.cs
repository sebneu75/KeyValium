using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyValium.Locking
{
    [StructLayout(LayoutKind.Auto)]
    internal ref struct LockEntry
    {
        internal const int ENTRY_SIZE = 64;

        internal LockEntry(Span<byte> data, int index)
        {
            Perf.CallCount();

            if (data.Length != ENTRY_SIZE)
            {
                throw new InvalidOperationException("Entry size mismatch.");
            }

            Index = index;
            _data = data;
        }

        public readonly int Index;

        internal Span<byte> _data;

        /// <summary>
        /// 0x00 : 1 Byte Transaction Type
        ///        0x00 - None (free slot)
        ///        0x01 - Read
        ///        0x02 - Write
        /// </summary>
        public ushort Type
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt16LittleEndian(_data);
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt16LittleEndian(_data, value);
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

                return BinaryPrimitives.ReadInt32LittleEndian(_data.Slice(0x04));
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteInt32LittleEndian(_data.Slice(0x04), value);
            }
        }

        /// <summary>
        /// 0x08 : 8 Byte Transaction Object Id
        /// </summary>
        public ulong Oid
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt64LittleEndian(_data.Slice(0x08));
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt64LittleEndian(_data.Slice(0x08), value);
            }
        }

        /// <summary>
        /// 0x10 : 8 Byte Transaction Id
        /// </summary>
        public KvTid Tid
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt64LittleEndian(_data.Slice(0x10));
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt64LittleEndian(_data.Slice(0x10), value);
            }
        }

        /// <summary>
        /// 0x18 : 8 Byte Expires
        /// </summary>
        public DateTime ExpiresUtc
        {
            get
            {
                Perf.CallCount();

                var utc = BinaryPrimitives.ReadInt64LittleEndian(_data.Slice(0x18));
                return DateTime.FromFileTimeUtc(utc);
            }
            set
            {
                Perf.CallCount();

                var utc = value.ToFileTimeUtc();
                BinaryPrimitives.WriteInt64LittleEndian(_data.Slice(0x18), utc);
            }
        }

        /// <summary>
        /// 0x20 : 16 Byte MachineId
        /// </summary>
        public Span<byte> MachineId
        {
            get
            {
                Perf.CallCount();

                return _data.Slice(0x20, 0x10);
            }
            set
            {
                Perf.CallCount();

                value.CopyTo(_data.Slice(0x20, 0x10));
            }
        }

        /// <summary>
        /// 0x30 : 16 Byte Hostname
        /// </summary>
        public Span<byte> MachineName
        {
            get
            {
                Perf.CallCount();

                return _data.Slice(0x30, 0x10);
            }
            set
            {
                Perf.CallCount();

                value.CopyTo(_data.Slice(0x30, 0x10));
            }
        }

        internal void Clear()
        {
            _data.Clear();
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
