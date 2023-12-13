namespace KeyValium.Inspector
{
    internal class FileMapProgress : IProgress<ulong>
    {
        public FileMapProgress(ulong total)
        {
            Total = total;
        }

        public void Report(ulong value)
        {
            Current = value;

            RaiseProgressChanged();
        }

        private void RaiseProgressChanged()
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> ProgressChanged;

        public ulong Current
        {
            get;
            private set;
        }

        public ulong Total
        {
            get;
            private set;
        }
    }
}
