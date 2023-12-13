using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector.Controls
{
    internal static class BlockViewColors
    {
        public static readonly Color Unknown = Color.Red;
        public static readonly Color Header = Color.DarkGray;
        public static readonly Color Meta = Color.Gray;

        public static readonly Color Fs = Color.LightGreen;
        public static readonly Color FsInUse = Color.Green;

        public static readonly Color FsIndex = Color.Gold;
        public static readonly Color Fsleaf = Color.DarkOrange;

        public static readonly Color DataIndex = Color.LightSteelBlue;
        public static readonly Color DataLeaf = Color.SteelBlue;

        public static readonly Color DataOverflow = Color.Magenta;
        public static readonly Color DataOverFlowCont = Color.DarkMagenta;

        public static readonly Color Border = Color.Black;
        public static readonly Color SelectedBorder = Color.BlueViolet;

        public static readonly Color FontForeGround = Color.Black;
    }
}
