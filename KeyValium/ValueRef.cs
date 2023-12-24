using System.Security.Cryptography;
using System.Text;

namespace KeyValium
{
    public ref struct ValueRef
    {
        internal ValueRef(Transaction tx, AnyPage page, int index)
        {
            Perf.CallCount();

            Version = tx.GetVersion();

            if (page != null)
            {
                var entry = page.AsContentPage.GetEntryAt(index);

                _key = entry.KeyBytes.ReadOnlySpan;

                if ((entry.Flags & EntryFlags.IsOverflow) != 0)
                {
                    Length = entry.OverflowLength;
                    _ovstream = tx.GetOverflowStream(entry.OverflowPageNumber, Length);
                }
                else if ((entry.Flags & EntryFlags.HasValue) != 0)
                {
                    IsInlineValue = true;
                    _inlinevalue = entry.InlineValueBytes.ReadOnlySpan;
                    Length = entry.InlineValueLength;
                }

                IsStreamRequired = Length > (ulong)Limits.MaxByteArraySize;
            }
            else
            {
                Version = default;
            }
        }

        public readonly bool IsInlineValue = false;

        public readonly bool IsStreamRequired;

        public readonly ulong Length = 0;

        internal readonly OverflowStream _ovstream;

        internal readonly TxVersion Version;

        internal readonly ReadOnlySpan<byte> _key;

        internal readonly ReadOnlySpan<byte> _inlinevalue;

        public bool IsValid
        {
            get
            {
                return Version.IsValid;
            }
        }

        internal void Validate()
        {
            Perf.CallCount();

            Logger.LogInfo(LogTopics.Validation, "Validating ValueRef.");

            Version.Validate();
        }

        public ReadOnlySpan<byte> Key
        {
            get
            {
                Perf.CallCount();

                Validate();

                return _key;
            }
        }

        public ReadOnlySpan<byte> ValueSpan
        {
            get
            {
                Perf.CallCount();

                Validate();

                return IsInlineValue ? _inlinevalue : GetAsByteArray();
            }
        }

        public byte[] Value
        {
            get
            {
                Perf.CallCount();

                Validate();

                return GetAsByteArray();
            }
        }

        internal byte[] GetAsByteArray()
        {
            Perf.CallCount();

            if (IsInlineValue)
            {
                return _inlinevalue.ToArray();
            }
            else if (_ovstream == null)
            {
                return null;
            }
            else
            {
                if (IsStreamRequired)
                {
                    throw new KeyValiumException(ErrorCodes.InternalError, "Value is too large and can only be accessed via stream interface.");
                }
                else
                {
                    return GetOverflowBytes();
                }
            }
        }

        public Stream ValueStream
        {
            get
            {
                Perf.CallCount();

                Validate();

                if (IsInlineValue)
                {
                    return new MemoryStream(_inlinevalue.ToArray(), false);
                }
                else
                {
                    return _ovstream;
                }
            }
        }

        private byte[] GetOverflowBytes()
        {
            Perf.CallCount();

            var ret = new byte[_ovstream.Length];

            _ovstream.Seek(0, SeekOrigin.Begin);
            var bytesread = _ovstream.Read(ret, 0, ret.Length);

            if (bytesread != ret.Length)
            {
                var msg = string.Format("Could not read enough data from stream: Length={0} Read={1}", ret.Length, bytesread);
                throw new NotSupportedException(msg);
            }

            return ret;
        }

        public string GetDebugInfo()
        {
            return string.Format("Length={0} IsValid={1} IsInlineValue={2} OFS.Length={3} OFS.Position={4} OFS.Page={5}",
                 Length, IsValid, IsInlineValue, _ovstream?.Length, _ovstream?.Position, _ovstream?._pageno);
        }
    }
}
