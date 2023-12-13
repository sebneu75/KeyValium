namespace KeyValium.Exceptions
{
    public class KeyValiumException : Exception
    {
        public KeyValiumException(ErrorCodes code, string msg) : this(code, msg, null)
        {
            Perf.CallCount();
        }

        public KeyValiumException(ErrorCodes code, string msg, Exception inner) : base(msg, inner)
        {
            Perf.CallCount();

            ErrorCode = code;
        }

        public ErrorCodes ErrorCode
        {
            get;
            private set;
        }
    }
}
