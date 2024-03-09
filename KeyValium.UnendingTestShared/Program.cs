namespace KeyValium.UnendingTestShared
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var id = args.Length > 0 ? args[0] : "";

            var watcher = new FolderWatcher(id);
            watcher.Watch();
        }
    }
}
