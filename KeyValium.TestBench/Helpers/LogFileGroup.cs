using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeyValium.TestBench.Helpers
{
    public class LogFileGroup
    {
        public LogFileGroup(string basename)
        {
            BaseName = basename;
        }

        public string BaseName
        {
            get;
            private set;
        }

        public string BaseDirectory
        {
            get
            {
                return Path.GetDirectoryName(BaseName);
            }
        }

        public string BaseFilename
        {
            get
            {
                return Path.GetFileName(BaseName);
            }
        }

        public string CurrentLogfile
        {
            get
            {
                return BaseName + "." + string.Format("{0:0000}", _counter);
            }
        }

        private int _counter = 1;

        public void DeleteGroup()
        {
            foreach (var log in GetAllFiles())
            {
                File.Delete(log);
            }
        }

        internal List<string> GetAllFiles()
        {
            var files = Directory.GetFiles(BaseDirectory, BaseFilename + ".*");

            return files.ToList();
        }

        internal void LogAction(ActionType action, ulong? txid, PathToKey key, KVEntry entry)
        {
            CheckLogSize();

            using (var writer = new StreamWriter(CurrentLogfile, true, Encoding.UTF8))
            {
                var msg = string.Format("{0}: {1} : {2} : {3} : {4}", action, key != null ? key : txid, entry?.KeyLength, entry?.ValueSeed, entry?.ValueLength);
                writer.WriteLine(msg);
            }
        }

        internal void LogError(Exception ex)
        {
            using (var writer = new StreamWriter(CurrentLogfile, true, Encoding.UTF8))
            {
                var msg = string.Format("ERROR: {0}", ex.Message);
                writer.WriteLine(msg);
                writer.WriteLine(ex.StackTrace);
            }
        }

        private void CheckLogSize()
        {
            var maxsize = 2L * 1024 * 1024 * 1024 - 1;

            if (File.Exists(CurrentLogfile))
            {
                var fi = new FileInfo(CurrentLogfile);
                if (fi.Length > maxsize)
                {
                    _counter++;
                }
            }
        }

        internal void Save(ActionType action)
        {
            foreach (var log in GetAllFiles())
            {
                var newname = string.Format("{0}.{1:yyyyMMdd-HHmmss}.{2}.save", action, DateTime.Now, Path.GetFileName(log));

                var newfile = Path.Combine(BaseDirectory, newname);
                File.Move(log, newfile);
            }
        }
    }
}
