using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector
{
    public class EntryInfo
    {
        internal EntryInfo()
        {
        }

        public int Index
        {
            get;
            internal set;
        }

        public ushort Offset
        {
            get;
            internal set;
        }

        public ushort Flags
        {
            get;
            internal set;
        }

        public ushort KeyLength
        {
            get;
            internal set;
        }

        public byte[] Key
        {
            get;
            internal set;
        }

        public ushort? InlineValueLength
        {
            get;
            internal set;
        }

        public byte[] InlineValue
        {
            get;
            internal set;
        }

        public KvPagenumber? SubTree
        {
            get;
            internal set;
        }

        public ulong? TotalCount
        {
            get;
            internal set;
        }

        public ulong? LocalCount
        {
            get;
            internal set;
        }

        public KvPagenumber? OverflowPage
        {
            get;
            internal set;
        }

        public ulong? OverflowLength
        {
            get;
            internal set;
        }

        public byte[] OverflowValue
        {
            get;
            internal set;
        }

        public KvPagenumber? LeftBranch
        {
            get;
            internal set;
        }

        public KvPagenumber? RightBranch
        {
            get;
            internal set;
        }

        public KvTid? Tid
        {
            get;
            internal set;
        }

        public KvPagenumber? FirstPage
        {
            get;
            internal set;
        }

        public KvPagenumber? LastPage
        {
            get;
            internal set;
        }

        public ulong? PageCount
        {
            get
            {
                if (FirstPage.HasValue && LastPage.HasValue)
                {
                    return LastPage.Value - FirstPage.Value + 1;
                }

                return null;
            }
        }
    }
}
