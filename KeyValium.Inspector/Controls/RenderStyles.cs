using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector.Controls
{
    internal class RenderStyles
    {
        public static readonly RenderStyle Default = new RenderStyle(null, Color.Black);

        public static readonly RenderStyle Header = new RenderStyle(Color.Thistle, Color.Black);

        public static readonly RenderStyle Branch = new RenderStyle(Color.Orange, Color.Black);

        public static readonly RenderStyle FreeSpace = new RenderStyle(Color.LightGreen, Color.Black);

        public static readonly RenderStyle[] Entry = new RenderStyle[]
        {
            new RenderStyle(Color.LightGray, Color.Black),
            new RenderStyle(Color.DarkGray, Color.Black),
        };

        public static readonly RenderStyle EntryOffsets = new RenderStyle(Color.Khaki, Color.Black);

        public static readonly RenderStyle Highlighted = new RenderStyle(Color.DarkBlue, Color.White);

        public static readonly RenderStyle HighlightedParent = new RenderStyle(Color.LightBlue, Color.Black);

        public static readonly RenderStyle Selection = new RenderStyle(Color.Purple, Color.White);
    }
}
