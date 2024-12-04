using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace KeyValium
{
    /// <summary>
    /// Represents a stream of bytes. Allows unified handling of byte pointers, byte arrays and byte streams
    /// The source can be a bytepointer, a byte array or a stream.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    internal ref struct ValInfo
    {
        /// <summary>
        /// creates a ValInfo from a span
        /// </summary>
        internal ValInfo(ReadOnlySpan<byte> val)
        {
            _span = val;
            Length = val.Length;
        }

        /// <summary>
        /// creates a ValInfo from a span
        /// </summary>
        internal ValInfo(ReadOnlySpan<byte> prefix, ReadOnlySpan<byte> val)
        {
            _prefix = prefix;
            _span = val;
            Length = val.Length + prefix.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads stream.Length bytes from the start of the stream (the complete stream)
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream)
        {
            _stream = stream;
            _stream.Seek(0, SeekOrigin.Begin);
            Length = stream.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads stream.Length bytes from the start of the stream (the complete stream)
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(ReadOnlySpan<byte> prefix, Stream stream)
        {
            _prefix = prefix;
            _stream = stream;
            _stream.Seek(0, SeekOrigin.Begin);
            Length = stream.Length + prefix.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads lenght bytes from the current position of the stream
        /// stream need not be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream, long length)
        {
            _stream = stream;
            Length = length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads lenght bytes from the current position of the stream
        /// stream need not be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(ReadOnlySpan<byte> prefix, Stream stream, long length)
        {
            _prefix = prefix;
            _stream = stream;
            Length = length + prefix.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads length bytes starting from position from the stream
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream, long position, long length)
        {
            _stream = stream;
            _stream.Seek(position, SeekOrigin.Begin);
            Length = length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads length bytes starting from position from the stream
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(ReadOnlySpan<byte> prefix, Stream stream, long position, long length)
        {
            _prefix = prefix;
            _stream = stream;
            _stream.Seek(position, SeekOrigin.Begin);
            Length = length + prefix.Length;
        }

        internal readonly ReadOnlySpan<byte> _prefix;

        internal readonly ReadOnlySpan<byte> _span;

        internal readonly Stream _stream;

        internal long _position = 0;

        internal readonly long Length;

        internal int CopyToWithoutPrefix(Span<byte> target)
        {
            if (_stream == null)
            {
                _span.Slice((int)_position, target.Length).CopyTo(target);
                _position += target.Length;

                return target.Length;
            }
            else
            {
                return _stream.Read(target);
            }
        }

        internal int CopyTo(Span<byte> target)
        {
            var bytescopied = 0;

            if (_position < _prefix.Length)
            {
                // copy prefix
                var prefixlen = Math.Min(_prefix.Length - (int)_position, target.Length);
                _prefix.Slice((int)_position, prefixlen).CopyTo(target);
                _position += prefixlen;
                bytescopied += prefixlen;
                target = target.Slice((int)_position);
            }

            // check target length otherwise if prefix is longer than target Slice will result in negative start
            if (_position >= _prefix.Length)
            {
                if (_stream == null)
                {
                    // copy span
                    _span.Slice((int)_position - _prefix.Length, target.Length).CopyTo(target);
                    _position += target.Length;
                    bytescopied += target.Length;
                }
                else
                {
                    // copy stream
                    var bytesread = _stream.Read(target);
                    _position += bytesread;
                    bytescopied += bytesread;
                }
            }

            return bytescopied;
        }
    }
}
