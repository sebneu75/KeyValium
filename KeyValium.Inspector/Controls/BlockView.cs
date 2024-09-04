using KeyValium.Inspector;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    public partial class BlockView : UserControl
    {
        public BlockView()
        {
            InitializeComponent();

            //this.SetStyle(ControlStyles.DoubleBuffer, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);

            this.DoubleBuffered = true;

            BlockSize = new Size(12, 12);
            BlockMargin = new Padding(1, 1, 1, 1);
            InnerMargin = new Padding(4, 4, 4, 4);
        }

        private short _selmetaindex;

        [Browsable(false)]
        internal short SelectedMetaIndex
        {
            get
            {
                return _selmetaindex;
            }
            set
            {
                if (_selmetaindex != value)
                {
                    _selmetaindex = value;
                    _numbercolumnwidth = -1;
                    Invalidate();
                }
            }
        }

        private FileMap _filemap;

        private int _numbercolumnwidth = -1;

        private int NumberColumnWidth
        {
            get
            {
                if (_numbercolumnwidth < 0 && BlockCount > 0)
                {
                    using (var g = this.CreateGraphics())
                    {
                        var size = g.MeasureString(string.Format(CultureInfo.InvariantCulture, "{0:N0}", BlockCount), this.Font);
                        _numbercolumnwidth = (int)size.Width + 4;
                    }
                }

                return _numbercolumnwidth;
            }
        }
        
        [Browsable(false)]
        internal FileMap FileMap
        {
            get
            {
                return _filemap;
            }
            set
            {
                _filemap = value;
                _numbercolumnwidth = -1;
                DoLayout();
            }
        }

        [Browsable(false)]
        public Size BlockSize
        {
            get;
            set;
        }

        [Browsable(false)]
        public Padding BlockMargin
        {
            get;
            set;
        }

        public Padding InnerMargin
        {
            get;
            set;
        }

        [Browsable(false)]
        private long BlockCount
        {
            get
            {
                return _filemap == null ? 0 : (long)_filemap.TotalPageCount;
            }
        }

        private long _selectedpage = -1;

        private long SelectedPage
        {
            get
            {
                return _selectedpage;
            }
            set
            {
                if (_selectedpage != value)
                {
                    _selectedpage = value;

                    // TODO
                    // Invalidate old Rect
                    // Invalidate new Rect
                    Invalidate();
                }
            }
        }

        #region Events

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void FileMapView_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.FillRectangle(Brushes.Bisque, e.ClipRectangle);

            if (BlockCount == 0)
            {
                return;
            }

            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //e.Graphics.TranslateTransform(0, AutoScrollPosition.Y);

            long currentline = (-AutoScrollPosition.Y - InnerMargin.Top) / _lineheight;

            var pos = currentline * _blocksperline;

            var x = NumberColumnWidth + InnerMargin.Left + BlockMargin.Left;
            var y = (AutoScrollPosition.Y + InnerMargin.Top) % _lineheight;  // firstline * _lineheight;

            var currentlineblocks = 0;

            while (pos < BlockCount && y < ClientSize.Height)
            {
                if (currentline % 2 == 0 && currentlineblocks == 0)
                {
                    // draw Blocknumber
                    var number = string.Format(CultureInfo.InvariantCulture, "{0:N0}", pos);

                    var rect = new RectangleF(InnerMargin.Left, y, NumberColumnWidth - 2, _lineheight);

                    var format = new StringFormat(StringFormatFlags.NoWrap);
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.None;

                    e.Graphics.DrawString(number, this.Font, brush_font, rect, format);
                }

                var brush = GetBrush(pos);
                e.Graphics.FillRectangle(brush, x, y, BlockSize.Width, BlockSize.Height);

                if (pos == SelectedPage)
                {
                    e.Graphics.DrawRectangle(pen_selectedborder, x, y, BlockSize.Width, BlockSize.Height);
                }
                else
                {
                    e.Graphics.DrawRectangle(pen_border, x, y, BlockSize.Width, BlockSize.Height);
                }

                x += BlockSize.Width + BlockMargin.Horizontal;
                currentlineblocks++;

                if (currentlineblocks >= _blocksperline)
                {
                    currentline++;
                    currentlineblocks = 0;
                    y += BlockSize.Height + BlockMargin.Vertical;
                    x = NumberColumnWidth + InnerMargin.Left + BlockMargin.Left;
                }

                pos++;
            }
        }

        private Pen pen_border = new Pen(BlockViewColors.Border);
        private Pen pen_selectedborder = new Pen(BlockViewColors.SelectedBorder, 4);

        private Brush brush_unknown = new SolidBrush(BlockViewColors.Unknown);
        private Brush brush_header = new SolidBrush(BlockViewColors.Header);
        private Brush brush_meta = new SolidBrush(BlockViewColors.Meta);
        private Brush brush_fs = new SolidBrush(BlockViewColors.Fs);
        private Brush brush_fsinuse = new SolidBrush(BlockViewColors.FsInUse);
        private Brush brush_fsindex = new SolidBrush(BlockViewColors.FsIndex);
        private Brush brush_fsleaf = new SolidBrush(BlockViewColors.Fsleaf);
        private Brush brush_dataindex = new SolidBrush(BlockViewColors.DataIndex);
        private Brush brush_dataleaf = new SolidBrush(BlockViewColors.DataLeaf);
        private Brush brush_dataoverflow = new SolidBrush(BlockViewColors.DataOverflow);
        private Brush brush_dataovercont = new SolidBrush(BlockViewColors.DataOverFlowCont);

        private Brush brush_font = new SolidBrush(BlockViewColors.FontForeGround);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Brush GetBrush(long pos)
        {
            var type = _filemap.GetPageType((short)SelectedMetaIndex, (KvPagenumber)pos);

            switch (type)
            {
                case PageTypesI.Unknown:
                    return brush_unknown;

                case PageTypesI.FileHeader:
                    return brush_header;

                case PageTypesI.Meta:
                    return brush_meta;

                case PageTypesI.FsIndex:
                    return brush_fsindex;

                case PageTypesI.FsLeaf:
                    return brush_fsleaf;

                case PageTypesI.FreeSpace:
                    return brush_fs;

                case PageTypesI.FreeSpaceInUse:
                    return brush_fsinuse;

                case PageTypesI.DataIndex:
                    return brush_dataindex;

                case PageTypesI.DataLeaf:
                    return brush_dataleaf;

                case PageTypesI.DataOverflow:
                    return brush_dataoverflow;

                case PageTypesI.DataOverflowCont:
                    return brush_dataovercont;
            }

            return brush_unknown;
        }

        private void FileMapView_Resize(object sender, EventArgs e)
        {
            DoLayout();
        }

        #endregion

        private int _blocksperline;
        private int _lineheight;
        private int _lines;

        private void DoLayout()
        {
            if (BlockCount > 0)
            {
                _blocksperline = (ClientSize.Width - InnerMargin.Horizontal - NumberColumnWidth) / (BlockSize.Width + BlockMargin.Horizontal);

                if (_blocksperline <= 0)
                {
                    // avoid division by zero
                    return;
                }

                _lineheight = BlockSize.Height + BlockMargin.Vertical;
                _lines = ((int)BlockCount + _blocksperline - 1) / _blocksperline;

                this.AutoScrollMinSize = new Size(0, (int)(_lines * _lineheight + InnerMargin.Vertical));

                //var x = ThumbMargin.Left;
                //var y = ThumbMargin.Top;
                //bool fullrow = false;

                //var maxx = 0;

                //foreach (var thumb in _thumbs)
                //{
                //    fullrow = false;

                //    thumb.Location = new Point(x, y);
                //    thumb.Size = ThumbSize;

                //    x += ThumbSize.Width;
                //    x += ThumbMargin.Right;

                //    maxx = Math.Max(maxx, x);

                //    if (panelContent.ClientSize.Width < x + ThumbSize.Width + ThumbMargin.Horizontal)
                //    {
                //        x = ThumbMargin.Left;
                //        y += ThumbSize.Height;
                //        y += ThumbMargin.Bottom;
                //        fullrow = true;
                //    }
                //}

                //y += fullrow ? 0 : ThumbSize.Height + ThumbMargin.Bottom;
            }

            this.Invalidate();
        }

        private void BlockView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SelectedPage = GetPage(e.X, e.Y);
            }
        }

        /// <summary>
        /// returns the Pagenumber displayed at x,y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private long GetPage(int x, int y)
        {
            if (BlockCount == 0)
                return -1;

            long firstline = (-AutoScrollPosition.Y - InnerMargin.Top) / _lineheight;

            var col = (x - InnerMargin.Left - NumberColumnWidth) / (BlockMargin.Horizontal + BlockSize.Width);
            if ((x - InnerMargin.Left - NumberColumnWidth) < 0 || col >= _blocksperline)
            {
                return -1;
            }

            var row = (y - (AutoScrollPosition.Y + InnerMargin.Top) % _lineheight) / (BlockMargin.Vertical + BlockSize.Height);

            var pageno = firstline * _blocksperline + row * _blocksperline + col;

            if (pageno >= BlockCount)
            {
                pageno = -1;
            }

            return pageno;
        }

        private int _mousex = -1;
        private int _mousey = -1;

        private void BlockView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X != _mousex || e.Y != _mousey)
            {
                _mousex = e.X;
                _mousey = e.Y;

                toolTip.Hide(this);
                timer.Stop();
                timer.Start();
            }
        }

        private void BlockView_MouseLeave(object sender, EventArgs e)
        {
            timer.Stop();
            toolTip.Hide(this);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var pageno = GetPage(_mousex, _mousey);
            if (pageno >= 0)
            {
                var msg = string.Format("Pagenumber: {0} ({1})", pageno, _filemap.GetPageType(SelectedMetaIndex, (KvPagenumber)pageno));
                toolTip.Show(msg, this, _mousex + 16, _mousey + 16);
            }

            timer.Stop();
        }

        private void BlockView_MouseEnter(object sender, EventArgs e)
        {
        }

        internal event EventHandler<ShowPageEventArgs> ShowPage;

        private void RaiseShowPage(KvPagenumber pageno)
        {
            ShowPage?.Invoke(this, new ShowPageEventArgs(pageno));
        }

        private void BlockView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedPage >= 0 && SelectedPage < BlockCount)
            {
                RaiseShowPage((KvPagenumber)SelectedPage);
            }
        }
    }
}
