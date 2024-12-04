using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    [StructLayout(LayoutKind.Auto)]
    internal ref struct LockHeader
    {
        internal const int HEADER_SIZE = 64;

        internal LockHeader(Span<byte> data)
        {
            Perf.CallCount();

            if (data.Length != HEADER_SIZE)
            {
                throw new InvalidOperationException("Header size mismatch.");
            }

            _data = data;
        }

        internal Span<byte> _data;

        /// <summary>
        /// 0x00 : 4 bytes Magic
        /// </summary>
        public uint Magic
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt32LittleEndian(_data);
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt32LittleEndian(_data, value);
            }
        }

        /// <summary>
        /// 0x04 : 2 Byte Pagetype
        /// </summary>
        public ushort PageType
        {
            get
            {
                Perf.CallCount();

                return BinaryPrimitives.ReadUInt16LittleEndian(_data.Slice(0x04));
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt16LittleEndian(_data.Slice(0x04), value);
            }
        }

        /// <summary>
        /// 0x06 : 2 Byte SharingMode
        /// </summary>
        public InternalSharingModes SharingMode
        {
            get
            {
                Perf.CallCount();

                return (InternalSharingModes)BinaryPrimitives.ReadUInt16LittleEndian(_data.Slice(0x06));
            }
            set
            {
                Perf.CallCount();

                BinaryPrimitives.WriteUInt16LittleEndian(_data.Slice(0x06), (ushort)value);
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

                return _data.Slice(0x10, 0x10);
            }
            set
            {
                Perf.CallCount();

                value.CopyTo(_data.Slice(0x10, 0x10));
            }
        }

        /// <summary>
        /// 0x20 : 16 Byte Hostname
        /// </summary>
        public Span<byte> MachineName
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
        /// 0x30 : 16 Byte LockGuid
        /// </summary>
        public Guid LockGuid
        {
            get
            {
                Perf.CallCount();

                return new Guid(_data.Slice(0x30, 0x10));
            }
            set
            {
                Perf.CallCount();

                if (!value.TryWriteBytes(_data.Slice(0x30, 0x10)))
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Could not write LockGuid.");
                }
            }
        }

        public override string ToString()
        {
            Perf.CallCount();

            var sb = new StringBuilder();

            sb.AppendFormat("Magic: {0:X8} ", Magic);
            sb.AppendFormat("PageType: {0} ", PageType);
            sb.AppendFormat("SharingMode: {0} ", SharingMode);
            sb.AppendFormat("MachineId: {0} ", Util.GetHexString(MachineId));
            sb.AppendFormat("MachineName: {0} ", Util.GetHexString(MachineName));
            sb.AppendFormat("LockGuid: {0} ", LockGuid);

            return sb.ToString();
        }
    }
}

