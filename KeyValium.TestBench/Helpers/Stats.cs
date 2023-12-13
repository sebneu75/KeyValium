namespace KeyValium.TestBench.Helpers
{
    public class Stats
    {
        public long Deleted = 0;

        public long Existing = 0;

        public long Got = 0;

        public long Inserted = 0;

        public long Updated = 0;

        public long Upserted = 0;

        public long TxCount = 0;

        public long Errors = 0;

        internal void Reset()
        {
            Deleted = 0;
            Existing = 0;
            Got = 0;
            Inserted = 0;
            Updated = 0;
            Upserted = 0;
            TxCount = 0;
            Errors = 0;
        }
    }
}
