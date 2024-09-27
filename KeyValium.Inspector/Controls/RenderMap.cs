using KeyValium.Inspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text;

namespace KeyValium.Inspector.Controls
{
    internal class RenderMap
    {
        public const int HexLineLength = 32 * 2 + 31 + 3 * 2;
        public const int TextLineLength = 32 + 3;

        public RenderMap(PageMap pagemap, bool hex)
        {
            BytesPerLine = 32;
            PageMap = pagemap;

            CreateText(hex);
            CreateMap(hex);
        }

        private void CreateMap(bool hex)
        {
            var textstart = TextOffsetFromByteOffset(PageMap.Map.AbsoluteOffset, hex, true);
            var textend = TextOffsetFromByteOffset(PageMap.Map.AbsoluteOffset + PageMap.Map.Length - 1, hex, false);
            TextRange = new TextRange(textstart, textend, PageMap.Map);

            CreateChildren(PageMap.Map, TextRange, hex);
        }

        private void CreateChildren(ByteRange map, TextRange textrange, bool hex)
        {

            foreach (var child in map.Children)
            {
                var textstart = TextOffsetFromByteOffset(child.AbsoluteOffset, hex, true);
                var textend = TextOffsetFromByteOffset(child.AbsoluteOffset + child.Length - 1, hex, false);
                var textchild = textrange.AddChild(textstart, textend, child);

                CreateChildren(child, textchild, hex);
            }
        }

        private Encoding _encoding = Encoding.GetEncoding("windows-1252");

        private void CreateText(bool hex)
        {
            if (hex)
            {
                CreateHex();
            }
            else
            {
                CreateText();
            }
        }

        private void CreateHex()
        {
            var sb = new StringBuilder();

            // Hex
            for (int i = 0; i < PageMap.Bytes.Length; i++)
            {
                if (i % 32 != 0 && i % 8 == 0)
                {
                    sb.Append("- ");
                }

                sb.AppendFormat("{0:X2}", PageMap.Bytes[i]);

                if ((i + 1) % 32 != 0)
                {
                    sb.Append(" ");
                }
            }

            Text = sb.ToString();
            LineLength = HexLineLength;
        }

        private void CreateText()
        {
            var sb = new StringBuilder();

            // Text
            for (int i = 0; i < PageMap.Bytes.Length; i++)
            {
                if (i % 32 != 0 && i % 8 == 0)
                {
                    sb.Append(" ");
                }

                var ch = _encoding.GetString(new byte[] { PageMap.Bytes[i] })[0];

                if (char.IsControl(ch) || char.IsWhiteSpace(ch))
                {
                    sb.Append('.');
                }
                //else if (ch == '\u00ad')
                //{
                //    sb.Append('.');
                //}
                else
                {
                    sb.Append(ch);
                }
            }

            Text = sb.ToString();
            LineLength = TextLineLength;
        }

        private int TextOffsetFromByteOffset(int byteoffset, bool hex, bool start)
        {
            if (hex)
            {
                var to = byteoffset / BytesPerLine * LineLength;
                byteoffset %= BytesPerLine;
                to += byteoffset * 3 + byteoffset / 8 * 2;

                return start ? to : to + 1;
            }
            else
            {
                var to = byteoffset / BytesPerLine * LineLength;
                byteoffset %= BytesPerLine;
                to += byteoffset + byteoffset / 8;

                return to;
            }
        }

        //private int GetHexRow(int offset)
        //{
        //    return offset / 32;
        //}

        //private int GetHexStartColumn(int offset)
        //{
        //    offset %= 32;

        //    var ret = offset * 3;
        //    ret += offset / 8 * 2;

        //    return ret;
        //}

        //private int GetHexEndColumn(int offset)
        //{
        //    return GetHexStartColumn(offset) + 1;
        //}

        //private int GetTextRow(int offset)
        //{
        //    return offset / 32;
        //}

        //private int GetTextStartColumn(int offset)
        //{
        //    offset %= 32;

        //    var ret = offset + offset / 8;

        //    return ret;
        //}

        //private int GetTextEndColumn(int offset)
        //{
        //    return GetTextStartColumn(offset);
        //}

        public readonly PageMap PageMap;

        public int BytesPerLine
        {
            get;
            private set;
        }

        public int LineLength
        {
            get;
            private set;
        }

        public int LineCount
        {
            get
            {
                Debug.Assert(Text.Length % LineLength == 0, "LineCount not multiple of LineLength!");
                return Text.Length / LineLength;
            }
        }

        public string Text
        {
            get;
            private set;
        }

        public TextRange TextRange
        {
            get;
            private set;
        }

        public TextRange HighLightedRange
        {
            get;
            internal set;
        }

        public TextRange SelectedRange
        {
            get;
            internal set;
        }

        /// <summary>
        /// gets all textranges for the given line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal TextRenderList GetTextRenderList(int line)
        {
            var start = line * LineLength;
            var end = start + LineLength - 1;

            var ranges = TextRange.GetOverlappingChildren(start, end);

            var ret = new TextRenderList(start, end);

            foreach (var range in ranges)
            {
                var item = new TextRenderItem(range.StartOffset, range.EndOffset, GetRenderStyle(range));
                ret.Merge(item);
            }

            return ret;
        }

        private RenderStyle GetRenderStyle(TextRange range)
        {
            if (range == SelectedRange)
            {
                return RenderStyles.Selection;
            }

            if (range == HighLightedRange)
            {
                if (range.Name.StartsWith("Entry[") ||
                    range.Name.StartsWith("EntryOffsets") ||
                    range.Name.StartsWith("Header"))
                {
                    return RenderStyles.HighlightedParent;
                }
                else if (range.Name=="Content")
                {
                    return RenderStyles.Default;
                }
                else
                {
                    return RenderStyles.Highlighted;
                }
            }

            if (range == HighLightedRange?.Parent)
            {
                switch (range.ByteRange.Name)
                {
                    case "Header":
                    case "Entry":
                    case "EntryOffsets":
                        return RenderStyles.HighlightedParent;

                    default:
                        return RenderStyles.Default;
                }
            }

            switch (range.ByteRange.Name)
            {
                case "Header":
                    return RenderStyles.Header;

                case "FreeSpace":
                    return RenderStyles.FreeSpace;

                case "Branch":
                    return RenderStyles.Branch;

                case "Entry":
                    return RenderStyles.Entry[range.ByteRange.Index & 1];

                case "EntryOffsets":
                    return RenderStyles.EntryOffsets;

                default:
                    return RenderStyles.Default;
            }
        }


        internal TextRange GetRange(int col, int row)
        {
            if (col < 0 || col >= LineLength || row < 0 || row >= LineCount)
            {
                return null;
            }            

            var offset = row * LineLength + col;

            return TextRange?.GetRangeAtOffset(offset);


            //TextRange lastitem = null;

            //var queue = new Queue<TextRange>();
            //queue.Enqueue(TextRange);

            //while (queue.Count > 0)
            //{
            //    var item = queue.Dequeue();
            //    if (item.Overlaps(offset, offset))
            //    {
            //        if (item.Children.Count == 0)
            //        {
            //            return item;
            //        }

            //        lastitem = item;

            //        foreach (var child in item.Children)
            //        {
            //            queue.Enqueue(child);
            //        }
            //    }
            //}

            //return lastitem;
        }
    }
}
