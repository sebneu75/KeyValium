using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    public class KvArrayValue
    {
        #region Constructors

        public KvArrayValue(ReadOnlyMemory<byte> val)
        {
            _bytes = val;
            Type = KvArrayTypes.Raw;
        }

        public KvArrayValue(Stream val)
        {
            _stream = val;
            Type = KvArrayTypes.Raw;
        }

        public KvArrayValue(string val)
        {
            _stringval = val;
            Type = KvArrayTypes.String;
        }

        public KvArrayValue(long val)
        {
            _longval = val;
            Type = KvArrayTypes.Long;
        }

        #endregion

        #region Variables

        public readonly KvArrayFlags Flags = KvArrayFlags.None; // Always None for now

        public readonly KvArrayTypes Type;

        private string _stringval;

        private long _longval;

        private ReadOnlyMemory<byte> _bytes;

        private Stream _stream;

        #endregion

        #region Properties

        #endregion
    }
}
