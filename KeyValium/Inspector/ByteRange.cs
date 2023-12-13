namespace KeyValium.Inspector
{
    public class ByteRange
    {
        public ByteRange(string name, int offset, int length, int index, Type type, object value, string displayvalue)
        {
            Children = new List<ByteRange>();

            Name = name;
            Offset = offset;
            Length = length;
            Index = index;
            Type = type;
            Value = value;
            DisplayValue = displayvalue;
        }

        public string Name
        {
            get;
            private set;
        }

        public Type Type
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }

        public string DisplayValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Offset relative to Parent
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }

        public int AbsoluteOffset
        {
            get
            {
                var ret = Offset;
                var parent = Parent;
                while (parent != null)
                {
                    ret += parent.Offset;
                    parent = parent.Parent;
                }

                return ret;
            }
        }

        public int Length
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public ByteRange AddChild(ByteRange range)
        {
            range.Parent = this;
            Children.Add(range);

            return range;
        }

        public ByteRange AddChild(string name, int offset, int length, int index, Type type, object value, string displayvalue)
        {
            var range = new ByteRange(name, offset, length, index, type, value, displayvalue);

            range.Parent = this;
            Children.Add(range);

            return range;
        }

        public ByteRange AddChild(string name, int offset, int length, Type type, object value, string displayvalue)
        {
            var range = new ByteRange(name, offset, length, -1, type, value, displayvalue);

            range.Parent = this;
            Children.Add(range);

            return range;
        }


        public List<ByteRange> Children
        {
            get;
            private set;
        }

        public ByteRange Parent
        {
            get;
            private set;
        }
    }
}
