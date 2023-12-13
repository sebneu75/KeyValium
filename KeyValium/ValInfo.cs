using KeyValium.Memory;
using KeyValium.Pages.Headers;
using System;
using System.IO;

namespace KeyValium
{
    /// <summary>
    /// Represents a stream of bytes. Allows unified handling of byte pointers, byte arrays and byte streams
    /// The source can be a bytepointer, a byte array or a stream.
    /// </summary>
    internal ref struct ValInfo
    {
        /// <summary>
        /// creates a ValInfo from a span
        /// </summary>
        internal ValInfo(ReadOnlySpan<byte> val)
        {
            _span = val;
            _stream = null;
            Length = val.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads stream.Length bytes from the start of the stream (the complete stream)
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream)
        {
            _span = default;
            _stream = stream;
            _stream.Seek(0, SeekOrigin.Begin);
            Length = stream.Length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads lenght bytes from the current position of the stream
        /// stream need not be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream, long length)
        {
            _span = default;
            _stream = stream;
            Length = length;
        }

        /// <summary>
        /// creates a ValInfo from a stream
        /// reads length bytes starting from position from the stream
        /// stream must be seekable
        /// </summary>
        /// <param name="bytes"></param>
        internal ValInfo(Stream stream, long position, long length)
        {
            _span = default;
            _stream = stream;
            _stream.Seek(position, SeekOrigin.Begin);
            Length = length;
        }

        internal readonly ReadOnlySpan<byte> _span;

        internal readonly Stream _stream;
        
        internal int _position = 0;

        internal readonly long Length;

        internal int CopyTo(Span<byte> target)
        {
            if (_stream == null)
            {
                _span.Slice(_position, target.Length).CopyTo(target);
                _position += target.Length;

                return target.Length;
            }
            else
            {
                return _stream.Read(target);
            }
        }
    }
}
