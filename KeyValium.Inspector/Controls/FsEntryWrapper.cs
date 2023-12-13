using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector.Controls
{
    public class FsEntryWrapper
    {
        public FsEntryWrapper(FsEntry entry)
        {
            _entry = entry;
        }

        private FsEntry _entry;

        private DataLocation _location;

        //[OLVColumn(DisplayIndex = 5, Name = "Location", Width = 160, TextAlign = System.Windows.Forms.HorizontalAlignment.Right)]
        public DataLocation Location
        {
            get
            {
                if (_location==null)
                {
                    _location = new DataLocation(_entry.Pagenumber, _entry.Index);
                }

                return _location;
            }
        }

        //[OLVColumn(DisplayIndex = 1, Name = "Tid", Width = 128, TextAlign = System.Windows.Forms.HorizontalAlignment.Right)]
        public KvTid Tid
        {
            get
            {
                return _entry.Tid;
            }
        }

        //[OLVColumn(DisplayIndex = 2, Name = "First Page", Width = 128, TextAlign = System.Windows.Forms.HorizontalAlignment.Right)]
        public KvPagenumber FirstPage
        {
            get
            {
                return _entry.FirstPage;
            }
        }

        //[OLVColumn(DisplayIndex = 3, Name = "Last Page", Width = 128, TextAlign = System.Windows.Forms.HorizontalAlignment.Right)]
        public KvPagenumber LastPage
        {
            get
            {
                return _entry.LastPage;
            }
        }

        //[OLVColumn(DisplayIndex = 4, Name = "Page Count", Width = 128, TextAlign = System.Windows.Forms.HorizontalAlignment.Right)]
        public ulong PageCount
        {
            get
            {
                return _entry.LastPage - _entry.FirstPage + 1;
            }
        }
    }

    public class DataLocation : IComparable, IComparable<DataLocation>
    {
        public readonly KvPagenumber Pagenumber;

        public readonly ushort Index;

        public DataLocation(KvPagenumber pagenumber, ushort index)
        {
            Pagenumber = pagenumber;
            Index = index;
        }

        public int CompareTo(DataLocation other)
        {
            if (this.Pagenumber == other.Pagenumber)
            {
                if (this.Index == other.Index)
                {
                        return 0;
                }

                return this.Index < other.Index ? -1 : +1;
            }

            return this.Pagenumber < other.Pagenumber ? -1 : +1;
        }

        public int CompareTo(object obj)
        {
            var dl = obj as DataLocation;
            if (dl!=null)
            {
                return this.CompareTo(dl);
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1:0000}", Pagenumber, Index);
        }
    }
}
