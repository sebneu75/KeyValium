using KeyValium.Inspector;
using KeyValium.Pages.Entries;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KeyValium.Inspector
{
    internal class Display
    {
        public static string Format(object val)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", val);
        }

        public static string FormatNumber(object val)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0}", val);
        }

        public static string FormatHex(uint? val)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:x8}", val);
        }

        public static string FormatHex(ushort? val)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:x4}", val);
        }

        public static string FormatHex(byte[] val, bool shownull = false, int maxlen = 0)
        {
            if (val == null)
            {
                return shownull ? "<null>" : "";
            }

            if (val.Length == 0)
            {
                return shownull ? "<0>" : "";
            }

            var len = maxlen == 0 ? val.Length : Math.Min(val.Length, maxlen);

            var sb = new StringBuilder(2 * len);

            for (int i = 0; i < len; i++)
            {
                sb.Append(val[i].ToString("x2"));
            }

            if (val.Length > len)
            {
                sb.Append("...");
            }

            var ret = sb.ToString();

            return ret;
        }

        internal static string FormatFlags(ushort flags)
        {
            var f = "";
            f += (flags & EntryFlags.HasSubtree) != 0 ? "S" : "";
            f += (flags & EntryFlags.IsOverflow) != 0 ? "O" : "";
            f += (flags & EntryFlags.HasValue) != 0 ? "V" : "";

            if (f == "")
            {
                f = "-";
            }

            return string.Format("{0} (0x{1:x4})", f, flags);
        }

        internal static string FormatString(byte[] bytes, Encoding enc)
        {
            if (bytes == null)
            {
                return "";
            }

            try
            {
                var result = enc.GetString(bytes);
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static JsonSerializerOptions _jsonoptions;

        static JsonSerializerOptions JsonOptions
        {
            get
            {
                if (_jsonoptions == null)
                {
                    _jsonoptions = new JsonSerializerOptions();
                    _jsonoptions.WriteIndented = true;
                    _jsonoptions.ReferenceHandler = ReferenceHandler.Preserve;
                }

                return _jsonoptions;
            }
        }

        internal static string FormatJson(byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }

            try
            {
                var item = JsonSerializer.Deserialize<JsonElement>(bytes, JsonOptions);
                return JsonSerializer.Serialize(item, JsonOptions);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        internal static string FormatInteger(byte[] bytes, bool bigendian)
        {
            if (bytes == null)
            {
                return "";
            }

            switch (bytes.Length)
            {
                case 0:
                    return "";
                case 1:
                    return bytes[0].ToString();
                case 2:
                    return (bigendian ? BinaryPrimitives.ReadInt16BigEndian(bytes) : BinaryPrimitives.ReadInt16LittleEndian(bytes)).ToString();
                case 4:
                    return (bigendian ? BinaryPrimitives.ReadInt32BigEndian(bytes) : BinaryPrimitives.ReadInt32LittleEndian(bytes)).ToString();
                case 8:
                    return (bigendian ? BinaryPrimitives.ReadInt64BigEndian(bytes) : BinaryPrimitives.ReadInt64LittleEndian(bytes)).ToString();

                default:
                    return "???";
            }
        }

        private static Encoding _encoding = Encoding.GetEncoding("windows-1252");

        internal static string FormatHexDump(byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }

            var sb = new StringBuilder();
            var bytesperline = 32;

            for (int i = 0; i < bytes.Length; i += bytesperline)
            {
                // write Offset
                sb.Append(i.ToString("X4"));
                sb.Append(": ");

                int k = 0;

                // write Hex    
                for (k = 0; k < bytesperline && (i + k < bytes.Length); k++)
                {
                    sb.Append(bytes[i + k].ToString("X2"));
                    sb.Append(" ");
                }

                sb.Append(new string(' ', (bytesperline - k) * 3));

                // write Text
                for (k = 0; k < bytesperline && (i + k < bytes.Length); k++)
                {
                    var ch = _encoding.GetString(new byte[] { bytes[i + k] })[0];

                    if (char.IsControl(ch) || char.IsWhiteSpace(ch))
                    {
                        sb.Append('.');
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
