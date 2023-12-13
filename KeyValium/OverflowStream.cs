using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System;

namespace KeyValium
{
    internal sealed class OverflowStream : Stream
    {
        public OverflowStream(Transaction tx, KvPagenumber pageno, ulong length)
        {
            Perf.CallCount();

            _pageno = pageno;
            _length = (long)length;
            Version = tx.GetVersion();

            _pagesize = (int)tx.PageSize;
        }

        internal readonly KvPagenumber _pageno;
        internal long _length;
        internal long _position;
        internal readonly TxVersion Version;
        internal int _pagesize;

        private void Validate()
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating OverflowStream.");

            Version.Validate();
        }

        public override bool CanRead
        {
            get
            {
                Perf.CallCount();

                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                Perf.CallCount();

                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                Perf.CallCount();

                return false;
            }
        }


        public override long Length
        {
            get
            {
                Perf.CallCount();

                return _length;
            }
        }


        public override long Position
        {
            get
            {
                Perf.CallCount();

                return _position;
            }
            set
            {
                Perf.CallCount();

                Seek(value, SeekOrigin.Begin);
            }
        }


        public override void Flush()
        {
            Perf.CallCount();

            // do nothing
        }

        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            Perf.CallCount();

            Validate();

            lock (Version.Tx.TxLock)
            {
                if (_position == _length)
                {
                    return 0;
                }

                var bytesread = 0;

                // get initial pagenumber
                KvPagenumber pageno = _pageno + (((ulong)_position + UniversalHeader.HeaderSize) / (ulong)_pagesize);
                var pageoffset = (int)(((ulong)_position + UniversalHeader.HeaderSize) % (ulong)_pagesize);

                var repeat = true;

                while (repeat)
                {
                    using (var page = Version.Tx.GetPage(pageno, pageno == _pageno, out _, false))
                    {
                        var numbytes = _pagesize - pageoffset;

                        if (offset + numbytes > buffer.Length)
                        {
                            numbytes = buffer.Length - offset;
                            repeat = false;
                        }

                        if (numbytes > count)
                        {
                            numbytes = count;
                            repeat = false;
                        }

                        if (_position + numbytes > _length)
                        {
                            numbytes = (int)(_length - _position);
                            repeat = false;
                        }

                        // TODO make faster, get rid of copy
                        page.Bytes.Span.Slice(pageoffset, numbytes).CopyTo(buffer.AsSpan(offset, numbytes));

                        //Buffer.BlockCopy(page.Handle.Bytes, pageoffset, buffer, offset, numbytes);

                        bytesread += numbytes;
                        _position += numbytes;
                        offset += numbytes;
                        count -= numbytes;

                        KvDebug.Assert(_position <= _length, "Position is greater length!");
                        KvDebug.Assert(count >= 0, "Count is negative!");

                        pageoffset = 0;
                        pageno++;

                        if (_position == _length)
                        {
                            repeat = false;
                        }
                    }
                }

                return bytesread;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Perf.CallCount();

            Validate();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0 || offset > _length)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    _position = offset;
                    break;

                case SeekOrigin.End:
                    if (_length + offset < 0 || _length + offset > _length)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    _position = _length + offset;
                    break;

                case SeekOrigin.Current:
                    if (_position + offset < 0 || _position + offset > _length)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    _position += offset;
                    break;
            }

            return _position;
        }

        public override void SetLength(long value)
        {
            Perf.CallCount();

            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Perf.CallCount();

            throw new InvalidOperationException();
        }
    }
}
