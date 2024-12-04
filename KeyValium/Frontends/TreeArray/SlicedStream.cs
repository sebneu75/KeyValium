using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Frontends.TreeArray
{
    /// <summary>
    /// Represents a slice of a stream
    /// </summary>
    internal class SlicedStream : Stream
    {
        internal SlicedStream(Stream stream, long start) : this(stream, start, stream.Length - start)
        {
        }

        internal SlicedStream(Stream stream, long start, long len)
        {
            _stream = stream;
            _start = start;
            _length = len;

            if (len < 0 || start + len > _stream.Length)
            {
                throw new ArgumentException("start and/or len out of range");
            }

            Seek(0, SeekOrigin.Begin);
        }

        internal long _start;
        internal long _length;
        internal long _position;

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

            _stream.Seek(_position + _start, SeekOrigin.Begin);

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

            if (_position + count > _length)
            {
                count = (int)(_length - _position);

            }

            var len = _stream.Read(buffer, offset, count);

            _position += len;

            return len;
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
