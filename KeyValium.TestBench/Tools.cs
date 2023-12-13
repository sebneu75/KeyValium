using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace KeyValium.TestBench
{
    public class Tools
    {
        public static bool BytesEqual(byte[] b1, byte[] b2)
        {
            if (b1 == null)
            {
                return b2 == null;
            }

            if (b1.Length == 0)
            {
                return b2 != null && b2.Length == 0;
            }

            for (int i = 0; i < b1.Length; i++)
            {
                var val1 = b1[i];
                var val2 = b2[i];

                if (val1 != val2)
                {
                    Console.WriteLine("{0}: {1}!={2}", i, val1, val2);
                }
            }

            return MemoryExtensions.SequenceEqual<byte>(b1, b2);
        }

        public static byte[] GetBytes(string val)
        {
            return Encoding.UTF8.GetBytes(val);
        }

        public static string GetString(byte[] val)
        {
            if (val == null)
                return null;

            return Encoding.UTF8.GetString(val);
        }

        public static string GetHexString(ReadOnlySpan<byte> val)
        {
            if (val == null || val.Length == 0)
                return "<null>";

            var sb = new StringBuilder(val.Length * 2);

            for (int i = 0; i < val.Length; i++)
            {
                sb.AppendFormat("{0:x2}", val[i]);
            }

            return sb.ToString();
        }

        public static byte[] ParseHexString(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return null;
            }

            if (hex.ToLowerInvariant() == "<null>")
            {
                return null;
            }

            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The key value cannot have an odd number of digits: {0}", hex));
            }

            byte[] data = new byte[hex.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                var b = hex.Substring(index * 2, 2);
                data[index] = byte.Parse(b, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        #region WriteColor

        static object _consolelock = new object();

        static ConsoleColor _background;
        static ConsoleColor _foreground;

        static void SaveColors()
        {
            _background = Console.BackgroundColor;
            _foreground = Console.ForegroundColor;
        }

        static void RestoreColors()
        {
            Console.BackgroundColor = _background;
            Console.ForegroundColor = _foreground;
        }

        private static void WriteInternal(ConsoleColor color, Exception ex, string format, params object[] args)
        {
            lock (_consolelock)
            {
                try
                {
                    SaveColors();
                    Console.ForegroundColor = color;

                    Console.WriteLine(format, args);
                    if (ex != null)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    RestoreColors();
                }
            }
        }

        public static void WriteColor(ConsoleColor color, string format, params object[] args)
        {
            WriteInternal(color, null, format, args);
        }

        public static void WriteError(Exception ex, string format, params object[] args)
        {
            WriteInternal(ConsoleColor.Red, ex, format, args);
        }

        public static void WriteSuccess(string format, params object[] args)
        {
            WriteInternal(ConsoleColor.Green, null, format, args);
        }


        #endregion
    }
}
