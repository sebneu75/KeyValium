using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyValium.Inspector.Controls
{
    internal class TextRenderList
    {
        public TextRenderList(int min, int max)
        {
            Minimum = min;
            Maximum = max;
        }

        public int Minimum
        {
            get;
            private set;
        }

        public int Maximum
        {
            get;
            private set;
        }

        private SortedList<int, TextRenderItem> _ranges = new SortedList<int, TextRenderItem>();

        public IReadOnlyList<TextRenderItem> Items
        {
            get
            {
                return _ranges.Values.ToList();
            }
        }

        public void Merge(TextRenderItem item)
        {
            item.StartOffset = Math.Max(item.StartOffset, Minimum);
            item.EndOffset = Math.Min(item.EndOffset, Maximum);

            var mergee = _ranges.Values.FirstOrDefault(x => Contains(x, item));

            if (mergee == null)
            {
                _ranges.Add(item.StartOffset, item);
                return;
            }
            else
            {
                item.BackColor = item.BackColor ?? mergee.BackColor;
                item.ForeColor = item.ForeColor ?? mergee.ForeColor;

                _ranges.Remove(mergee.StartOffset);

                if (mergee.StartOffset == item.StartOffset && mergee.EndOffset == item.EndOffset)
                {
                    // replace full entry                    
                    _ranges.Add(item.StartOffset, item);
                }
                else if (mergee.StartOffset == item.StartOffset)
                {
                    mergee.StartOffset = item.EndOffset + 1;
                    _ranges.Add(item.StartOffset, item);
                    _ranges.Add(mergee.StartOffset, mergee);
                }
                else if (mergee.EndOffset == item.EndOffset)
                {
                    mergee.EndOffset = item.StartOffset - 1;
                    _ranges.Add(mergee.StartOffset, mergee);
                    _ranges.Add(item.StartOffset, item);
                }
                else
                {
                    var mergeeleft = new TextRenderItem(mergee.StartOffset, item.StartOffset - 1, new RenderStyle(mergee.BackColor, mergee.ForeColor));
                    var mergeeright = new TextRenderItem(item.EndOffset + 1, mergee.EndOffset, new RenderStyle(mergee.BackColor, mergee.ForeColor) );

                    _ranges.Add(mergeeleft.StartOffset, mergeeleft);
                    _ranges.Add(item.StartOffset, item);
                    _ranges.Add(mergeeright.StartOffset, mergeeright);
                }
            }
        }

        private bool Contains(TextRenderItem x, TextRenderItem other)
        {
            return x.StartOffset <= other.StartOffset && other.EndOffset <= x.EndOffset;
        }
    }
}
