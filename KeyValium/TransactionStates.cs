namespace KeyValium
{
    /// <summary>
    /// The states of a transaction.
    /// </summary>
    public enum TransactionStates
    {
        Active,
        Committed,
        RolledBack,
        Disposed,
        Failed
    }
}
