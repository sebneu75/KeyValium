using KeyValium.Pages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    /// <summary>
    /// Wraps a stream and adds a prefix to it
    /// </summary>
    internal class PrefixedStream : Stream
    {
        internal PrefixedStream(ReadOnlyMemory<byte> prefix, Stream stream)
        {
            _prefix = prefix;
            _stream = stream;
            _stream.Seek(0, SeekOrigin.Begin);
            _length = _stream.Length + _prefix.Length;
        }

        internal long _length;
        internal long _position;
        internal ReadOnlyMemory<byte> _prefix;
        internal Stream _stream;

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                Perf.CallCount();

                return true;
            }
        }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                Perf.CallCount();

                return true;
            }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                Perf.CallCount();

                return false;
            }
        }

        /// <summary>
        /// Returns the length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                Perf.CallCount();

                return _length;
            }
        }

        /// <summary>
        /// Returns or sets the current position of the stream.
        /// </summary>
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

        /// <summary>
        /// Sets the position of the stream.
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <returns>the position</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            Perf.CallCount();

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

            if (_position >= _prefix.Length)
            {
                _stream.Seek(_position - _prefix.Length, SeekOrigin.Begin);
            }
            else
            {
                _stream.Seek(0, SeekOrigin.Begin);
            }

            return _position;
        }

        /// <summary>
        /// Reads data from the stream
        /// </summary>
        /// <param name="buffer">a buffer</param>
        /// <param name="offset">offset in the buffer</param>
        /// <param name="count">number of bytes to read</param>
        /// <returns>number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Perf.CallCount();

            if (_position == _length)
            {
                return 0;
            }

            var prefixlen = 0;
            var streamlen = 0;

            if (_position < _prefix.Length)
            {
                prefixlen = _prefix.Length - (int)_position;
                prefixlen = Math.Min(prefixlen, count);

                var span = _prefix.Span.Slice((int)_position, prefixlen);
                span.CopyTo(buffer.AsSpan(offset, prefixlen));
            }

            if (count > prefixlen)
            {
                streamlen = count - prefixlen;
                streamlen = _stream.Read(buffer, offset + prefixlen, streamlen);
            }

            _position += prefixlen + streamlen;

            return prefixlen + streamlen;
        }

        public override void Flush()
        {
            // do nothing
        }

        /// <summary>
        /// Unsupported. Throws an exception.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException"></exception>
        public override void SetLength(long value)
        {
            Perf.CallCount();

            throw new NotSupportedException();
        }

        /// <summary>
        /// Unsupported. Throws an exception.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Perf.CallCount();

            throw new NotSupportedException();
        }
    }
}


