namespace KeyValium.Pages.Entries
{
    internal static class EntryFlags
    {
        public const ushort None = 0x0000;

        // Value Flags
        public const ushort HasValue = 0x0001;
        public const ushort IsOverflow = 0x0002;

        // Key Flags
        public const ushort HasSubtree = 0x0100;        
    }
}
