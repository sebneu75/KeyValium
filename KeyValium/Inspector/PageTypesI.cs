namespace KeyValium.Inspector
{
    public enum PageTypesI : ushort
    {
        Unknown,
        FileHeader,
        Meta,
        FsIndex,
        FsLeaf,
        FreeSpace,
        FreeSpaceInUse,
        DataIndex,
        DataLeaf,
        DataOverflow,
        DataOverflowCont,
    }
}
