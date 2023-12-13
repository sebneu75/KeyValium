namespace KeyValium.TestBench.Helpers
{
    public enum ActionType
    {
        None,

        BeginTx,
        CommitTx,
        RollbackTx,
        BeginChildTx,
        CommitChildTx,
        RollbackChildTx,
        Delete,
        Exists,
        Get,
        GetNext,
        GetPrevious,
        Insert,
        Update,
        Upsert,
        IterateForward,
        IterateBackward,
        CreateCursor,
        DeleteCursor,
        
        ERROR
    }
}
