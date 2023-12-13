namespace KeyValium.Logging
{
    [Flags]
    internal enum LogTopics
    {
        None = 0x0,

        Database = 0x0001,
        Transaction = 0x0002,
        Allocation = 0x0004,
        Split = 0x0008,
        Merge = 0x0010,
        Insert = 0x0020,
        Update = 0x0040,
        Delete = 0x0080,
        Freespace = 0x0100,
        Cursor = 0x0200,
        Tracking = 0x0400,
        DataAccess = 0x0800,
        Validation = 0x1000,
        Lock = 0x2000,
        Meta = 0x4000,

        All = 0xffff
    }
}
