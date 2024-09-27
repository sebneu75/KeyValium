using KeyValium.Inspector;
using KeyValium.Inspector.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class HexView : UserControl
    {
        public HexView()
        {
            InitializeComponent();

            UpdateMetrics();
        }

        private PageMap _pagemap;

        [Browsable(false)]
        internal PageMap PageMap
        {
            get
            {
                return _pagemap;
            }
            set
            {
                if (_pagemap != value)
                {
                    _pagemap = value;

                    CreateRenderMaps(_pagemap);

                    Invalidate();
                }
            }
        }

        internal TextRange HighLightedRange
        {
            get
            {
                return _rmhex?.HighLightedRange;
            }
            private set
            {
                if (_rmhex?.HighLightedRange != value)
                {
                    _rmhex.HighLightedRange = value;
                    _rmtext.HighLightedRange = value;

                    Invalidate();
                }
            }
        }

        internal TextRange SelectedRange
        {
            get
            {
                return _rmhex.SelectedRange;
            }
            private set
            {
                if (_rmhex.SelectedRange != value)
                {
                    _rmhex.SelectedRange = value;
                    _rmtext.SelectedRange = value;

                    Invalidate();
                }
            }
        }

        private RenderMap _rmhex;
        private RenderMap _rmtext;

        private void CreateRenderMaps(PageMap pagemap)
        {
            if (pagemap == null)
            {
                _rmhex = null;
                _rmtext = null;

                return;
            }

            _rmhex = new RenderMap(pagemap, true);
            _rmtext = new RenderMap(pagemap, false);
        }

        #region Metrics

        private void UpdateMetrics()
        {
            using (var g = this.CreateGraphics())
            {
                _offsetleft = 0;
                _offsetwidth = TextRenderer.MeasureText(g, new string('W', 5), Font, Size.Empty, _textformatflags).Width + _padding.Horizontal;

                _hexleft = _offsetleft + _offsetwidth;
                _hexwidth = TextRenderer.MeasureText(g, new string('W', RenderMap.HexLineLength), Font, Size.Empty, _textformatflags).Width + _padding.Horizontal;

                _textleft = _hexleft + _hexwidth;
                _textwidth = TextRenderer.MeasureText(g, new string('W', RenderMap.TextLineLength), Font, Size.Empty, _textformatflags).Width + _padding.Horizontal;

                _charsize = TextRenderer.MeasureText(g, "W", Font, Size.Empty, _textformatflags);
            }
        }

        private TextFormatFlags _textformatflags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix; // | TextFormatFlags.TextBoxControl;
        private Size _charsize;

        private void HexView_FontChanged(object sender, EventArgs e)
        {
            UpdateMetrics();
        }

        #endregion

        private Padding _padding = new Padding(8, 4, 8, 4);

        private int _offsetleft = -1;
        private int _offsetwidth = -1;

        private int _hexleft = -1;
        private int _hexwidth = -1;

        private int _textleft = -1;
        private int _textwidth = -1;

        //private int OffsetColumnLeft
        //{
        //    get
        //    {
        //        return Padding.Left + _offsetleft * _textsize.Width;
        //    }
        //}

        //private int HexColumnLeft
        //{
        //    get
        //    {
        //        return Padding.Left + _hexleft * _textsize.Width;
        //    }
        //}

        //private int TextColumnLeft
        //{
        //    get
        //    {
        //        return Padding.Left + _textleft * _textsize.Width;
        //    }
        //}

        #region Painting

        private void HexView_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.FillRectangle(Brushes.Bisque, e.ClipRectangle);

            if (PageMap == null)
            {
                return;
            }

            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //e.Graphics.TranslateTransform(0, AutoScrollPosition.Y);

            Render(e.Graphics);

            AutoScrollMinSize = new Size(_textleft + _textwidth + _padding.Horizontal, _rmhex.LineCount * _charsize.Height + _padding.Vertical);
        }

        private void Render(Graphics g)
        {
            // Render offset background
            var bg = new Rectangle(AutoScrollPosition.X, -_charsize.Height, _offsetwidth, ClientSize.Height + _charsize.Height);
            g.FillRectangle(new SolidBrush(SystemColors.ControlLight), bg);

            for (int i = 0; i < _rmhex.LineCount; i++)
            {
                var y = i * _charsize.Height + _padding.Top + AutoScrollPosition.Y;

                if (IsVisible(y))
                {
                    RenderOffset(g, i, y);
                    RenderHex(g, i, y);
                    RenderText(g, i, y);
                }
            }
        }

        private bool IsVisible(int y)
        {
            return y >= -_charsize.Height && y <= ClientSize.Height + _charsize.Height;
        }

        private void RenderOffset(Graphics g, int i, int y)
        {
            var x = _offsetleft + AutoScrollPosition.X;

            var text = string.Format("{0:X4}:", i * _rmhex.BytesPerLine);
            //var textsize = TextRenderer.MeasureText(g, text, Font, Size.Empty, _textformatflags);
            var rect = new Rectangle(x, y, _offsetwidth - 4, _charsize.Height);

            TextRenderer.DrawText(g, text, Font, rect, this.ForeColor, _textformatflags | TextFormatFlags.Right);
        }


        private void RenderHex(Graphics g, int i, int y)
        {
            var renderlist = _rmhex.GetTextRenderList(i);
            var x = _hexleft + _padding.Left + AutoScrollPosition.X;

            foreach (var item in renderlist.Items)
            {
                var text = _rmhex.Text.Substring(item.StartOffset, item.Length);
                var textsize = TextRenderer.MeasureText(g, text, Font, Size.Empty, _textformatflags);
                var rect = new Rectangle(x, y, textsize.Width, textsize.Height);

                if (item.BackColor.HasValue)
                {
                    g.FillRectangle(new SolidBrush(item.BackColor.Value), rect);
                }

                TextRenderer.DrawText(g, text, Font, rect, item.ForeColor.Value, _textformatflags);
                x += textsize.Width;
            }
        }

        private void RenderText(Graphics g, int i, int y)
        {
            var renderlist = _rmtext.GetTextRenderList(i);

            var x = _textleft + _padding.Left + AutoScrollPosition.X;

            foreach (var item in renderlist.Items)
            {
                var text = _rmtext.Text.Substring(item.StartOffset, item.Length);
                var textsize = TextRenderer.MeasureText(g, text, Font, Size.Empty, _textformatflags);
                var rect = new Rectangle(x, y, textsize.Width, textsize.Height);

                if (item.BackColor.HasValue)
                {
                    g.FillRectangle(new SolidBrush(item.BackColor.Value), rect);
                }

                TextRenderer.DrawText(g, text, Font, rect, item.ForeColor.Value, _textformatflags);

                x += textsize.Width;
            }
        }

        #endregion

        #region Mouse and Timer Events

        private void HexView_MouseClick(object sender, MouseEventArgs e)
        {
            //SelectedRange = GetHexRangeAt(e.Location, _rmhex)?.Parent;
        }

        private void HexView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void HexView_MouseEnter(object sender, EventArgs e)
        {

        }

        private void HexView_MouseLeave(object sender, EventArgs e)
        {
            if (_rmhex == null)
            {
                return;
            }

            timer.Stop();
            toolTip.Hide(this);

            HighLightedRange = null;
        }

        private int _oldmousex = -1;
        private int _oldmousey = -1;

        private void HexView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rmhex == null)
            {
                return;
            }

            if (e.X != _oldmousex || e.Y != _oldmousey)
            {
                HighLightedRange = GetHexRangeAt(e.Location, _rmhex);

                _oldmousex = e.X;
                _oldmousey = e.Y;

                toolTip.Hide(this);
                timer.Stop();
                timer.Start();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var item = HighLightedRange;
            if (item != null)
            {
                var msg = string.Format("{0}", item.FullName);
                toolTip.Show(msg, this, _oldmousex + 16, _oldmousey + 16);
            }

            timer.Stop();
        }

        #endregion

        #region Helpers

        private TextRange GetHexRangeAt(Point mouseloc, RenderMap map)
        {
            var x = mouseloc.X - AutoScrollPosition.X - _hexleft - _padding.Left;
            var y = mouseloc.Y - AutoScrollPosition.Y - _padding.Top;

            var line = y / _charsize.Height;
            var ch = x / _charsize.Width;

            var item = map.GetRange(ch, line);

            return item;
        }

        #endregion
    }
}
