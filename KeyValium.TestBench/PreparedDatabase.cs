using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.TestBench
{
    public abstract class PreparedDatabase : IDisposable
    {
        public const string WORKINGPATH = @"c:\!KeyValium";

        public static string DUMPFILE = Path.Combine(WORKINGPATH, "dump");

        static PreparedDatabase()
        {
            Directory.CreateDirectory(WORKINGPATH);
            TestDescription.WorkingPath = WORKINGPATH;
        }

        public PreparedDatabase()
            : this("Unnamed")
        {
        }

        public PreparedDatabase(string name)
        {
            Description = new TestDescription(name);
        }

        public PreparedDatabase(TestDescription description)
        {
            Description = description;
        }

        public TestDescription Description
        {
            get;
            private set;
        }

        public abstract void CreateNewDatabase(bool forcecreatekeys, bool insertkeys, KeyOrder order = KeyOrder.Ascending, int skip = 0);

        public abstract void OpenDatabase();

        public abstract void PrepareBeginRT();

        public abstract void FinishBeginRT();

        public abstract void PrepareCommitRT();

        public abstract void FinishCommitRT();

        public abstract void PrepareRollbackRT();

        public abstract void FinishRollbackRT();

        public abstract void PrepareBeginWT();

        public abstract void FinishBeginWT();

        public abstract void PrepareCommitWT();

        public abstract void FinishCommitWT();

        public abstract void PrepareRollbackWT();

        public abstract void FinishRollbackWT();

        public abstract void PrepareInsert();

        public abstract void FinishInsert();

        public abstract void PrepareUpdate();

        public abstract void FinishUpdate();

        public abstract void PrepareDelete();

        public abstract void FinishDelete();

        public abstract void PrepareRead();

        public abstract void FinishRead();




        public abstract void BeginRT();
        public abstract void CommitRT();
        public abstract void RollbackRT();
        public abstract void BeginWT();
        public abstract void CommitWT();
        public abstract void RollbackWT();
        public abstract void Insert();
        public abstract void Update();
        public abstract void Delete();
        public abstract void Read();
        public abstract void Seek();
        public abstract void ItForward();
        public abstract void ItBackward();

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~PreparedDatabase()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
