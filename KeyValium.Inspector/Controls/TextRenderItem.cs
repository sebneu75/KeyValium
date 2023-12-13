using System.Drawing;

namespace KeyValium.Inspector.Controls
{
    internal class TextRenderItem
    {
        public TextRenderItem(int start, int end, RenderStyle style)
        {
            StartOffset = start;
            EndOffset = end;
            ForeColor = style?.ForeColor;
            BackColor = style?.BackColor;
        }

        public TextRenderItem(int start, int end) : this(start, end, null)
        {
        }

        public int StartOffset
        {
            get;
            set;
        }

        public int EndOffset
        {
            get;
            set;
        }

        public int Length
        {
            get
            {
                return EndOffset - StartOffset + 1;
            }
        }

        public Color? BackColor
        {
            get;
            set;
        }

        public Color? ForeColor
        {
            get;
            set;
        }
    }
}
