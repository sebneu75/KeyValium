namespace KeyValium.Exceptions
{
    public class KeyValiumException : Exception
    {
        internal KeyValiumException(ErrorCodes code, string msg) : this(code, msg, null)
        {
            Perf.CallCount();
        }

        internal KeyValiumException(ErrorCodes code, string msg, Exception inner) : base(msg, inner)
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
