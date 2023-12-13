namespace KeyValium.Pages
{
    internal enum PageStates
    {
        None = 0,
        Dirty = 1,
        DirtyAtParent = 2,
        Loose = 3,
        Spilled = 4,
        Free = 5,
    }

    //public static class PageStates
    //{

    //    // None (clean)
    //    public const int None = 0x00;

    //    // Dirty
    //    public const int Dirty = 0x01;

    //    // DirtyAtParent
    //    public const int DirtyAtParent = 0x02;

    //    // Loose
    //    public const int Loose = 0x03;

    //    // Spilled
    //    public const int Spilled = 0x04;

    //    // Free
    //    public const int Free = 0x05;

    //    public static string GetName(int pagestate)
    //    {
    //        switch (pagestate)
    //        {
    //            case PageStates.None:
    //                return "None";
    //            case PageStates.Dirty:
    //                return "Dirty";
    //            case PageStates.DirtyAtParent:
    //                return "DirtyAtParent";
    //            case PageStates.Loose:
    //                return "Loose";
    //            case PageStates.Spilled:
    //                return "Spilled";
    //            case PageStates.Free:
    //                return "Free";
    //        }

    //        return "Unknown";
    //    }
    //}
}
