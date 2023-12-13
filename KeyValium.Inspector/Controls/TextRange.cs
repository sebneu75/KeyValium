using KeyValium.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyValium.Inspector.Controls
{
    internal class TextRange
    {
        public TextRange(int start, int end, ByteRange byterange)
        {
            Children = new List<TextRange>();

            StartOffset = start;
            EndOffset = end;
            ByteRange = byterange;
        }

        public int StartOffset
        {
            get;
            private set;
        }

        public int EndOffset
        {
            get;
            private set;
        }

        public int Length
        {
            get
            {
                return EndOffset - StartOffset + 1;
            }
        }

        //public TextRange AddChild(TextRange range)
        //{
        //    range.Parent = this;
        //    Children.Add(range);

        //    return range;
        //}

        public TextRange AddChild(int start, int end, ByteRange byterange)
        {
            var range = new TextRange(start, end, byterange);

            range.Parent = this;
            Children.Add(range);

            if (range.Length == 0)
            {
                Console.WriteLine();
            }

            return range;
        }

        public List<TextRange> Children
        {
            get;
            private set;
        }

        public TextRange Parent
        {
            get;
            private set;
        }

        public ByteRange ByteRange
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                if (ByteRange.Index < 0)
                {
                    return ByteRange.Name;
                }

                return string.Format("{0}[{1}]", ByteRange.Name, ByteRange.Index);
            }
        }

        public string FullName
        {
            get
            {
                var list = new List<TextRange>();

                var item = this;
                while (item != null)
                {
                    list.Add(item);
                    item = item.Parent;
                }

                list.Reverse();

                var name = string.Join('.', list.Select(x => x.Name));

                return string.Format("{0} = {1}", name, ByteRange.DisplayValue);
            }
        }

        internal List<TextRange> GetOverlappingChildren(int start, int end)
        {
            var ret = new List<TextRange>();

            var queue = new Queue<TextRange>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (item.Overlaps(start, end))
                {
                    ret.Add(item);

                    foreach (var child in item.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return ret;
        }

        internal bool Overlaps(int start, int end)
        {
            return StartOffset <= start && start <= EndOffset ||
                   StartOffset <= end && end <= EndOffset ||
                   start <= StartOffset && StartOffset <= end ||
                   start <= EndOffset && EndOffset <= end;
        }
    }
}

