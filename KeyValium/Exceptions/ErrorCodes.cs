namespace KeyValium.Exceptions
{
    public enum ErrorCodes
    {
        InvalidFileFormat = 1,
        InvalidVersion = 2,
        InvalidParameter = 3,
        InvalidPageType = 4,
        InvalidSplitIndex = 5,
        InvalidIndex = 6,
        InvalidFlags = 7,
        InvalidPageState = 8,
        InvalidCursor = 9,

        PageNotFound = 20,
        PagenumberMismatch = 21,
        UnhandledPageType = 22,
        UnhandledPageState = 23,
        UnhandledEmptyPage = 24,

        VMSFailed = 40,

        KeyMismatch = 50,
        KeyAlreadyExists = 51,
        KeyNotFound = 52,
        KeyTooShort = 53,
        KeyTooLong = 54,

        NodeIsFull = 60,

        TransactionIsReadonly = 70,
        TransactionHasChildren = 71,
        TransactionCommitted = 72,
        TransactionRolledBack = 73,
        TransactionFailed = 74,

        FreeSpaceEntryExists = 80,

        InternalError = 500,
    }
}


