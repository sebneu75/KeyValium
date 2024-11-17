using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyValium.TestBench.Shared
{
    public class SharedTestInfo
    {
        // the folder containing the control files
        public string NetworkPath;

        // the machine to run the test on
        public List<MachineInfo> Machines = new List<MachineInfo>();

        public string ToolsSourceDirectory;

        /// <summary>
        /// returns the first machine
        /// </summary>
        [JsonIgnore]
        public MachineInfo Machine
        {
            get
            {
                return Machines.FirstOrDefault();
            }
        }

        // number of processes per machine
        public int ProcessCount;

        // given by controller
        public string Token;

        // databases to use in each process
        public List<DatabaseInfo> DatabaseInfos = new List<DatabaseInfo>();

        /// <summary>
        /// returns the first database
        /// </summary>
        [JsonIgnore]
        public DatabaseInfo DatabaseInfo
        {
            get
            {
                return DatabaseInfos.FirstOrDefault();
            }
        }

        [JsonIgnore]
        private string _dbfilename = null;

        [JsonIgnore]
        public string DbFilename
        {
            get
            {
                if (_dbfilename == null)
                {
                    var path = DatabaseInfo.SharingMode == InternalSharingModes.SharedNetwork ? NetworkPath : Path.Combine(Machine.LocalPath, "Data");

                    _dbfilename = Path.Combine(path, DatabaseInfo.Filename);
                }

                return _dbfilename;
            }
        }

        [JsonIgnore]
        private string _uncdbfilename = null;

        [JsonIgnore]
        public string UncDbFilename
        {
            get
            {
                if (_uncdbfilename == null)
                {
                    var path = DatabaseInfo.SharingMode == InternalSharingModes.SharedNetwork ? NetworkPath : Path.Combine(Machine.RemotePath, "Data");

                    _uncdbfilename = Path.Combine(path, DatabaseInfo.Filename);
                }

                return _uncdbfilename;
            }
        }

        [JsonIgnore]
        private string _localfilename = null;

        [JsonIgnore]
        public string LocalDbFilename
        {
            get
            {
                if (_localfilename == null)
                {
                    var path = DatabaseInfo.SharingMode == InternalSharingModes.SharedNetwork ? NetworkPath : Path.Combine(Machine.LocalPath, "Data");

                    _localfilename = string.Format("{0}-{1}", Machine.Name, DatabaseInfo.Filename);
                }

                return _localfilename;
            }
        }

        /// <summary>
        /// Pattern for Controlfiles
        /// </summary>
        [JsonIgnore]
        public string ControlFilePattern
        {
            get
            {
                return string.Format("{0}-{1}-{2}-*.kvjob", Machine.Name, DatabaseInfo.Filename, Token);
            }
        }

        public string GetControlFileName(int cycle)
        {
            var name = string.Format("{0}-{1}-{2}-{3:0000}.kvjob", Machine.Name, DatabaseInfo.Filename, Token, cycle);

            return Path.Combine(NetworkPath, name);
        }

        [JsonIgnore]
        private string _txlog = null;

        [JsonIgnore]
        public string WriteTxLog
        {
            get
            {
                if (_txlog == null)
                {
                    _txlog = DbFilename + ".writes.log";
                }

                return _txlog;
            }
        }

        [JsonIgnore]
        private string _unctxlog = null;

        [JsonIgnore]
        public string UncWriteTxLog
        {
            get
            {
                if (_unctxlog == null)
                {
                    _unctxlog = UncDbFilename + ".writes.log";
                }

                return _unctxlog;
            }
        }

        public string GetReadTxLog(string threadname)
        {
            return DbFilename + ".reads." + threadname + ".log";
        }

        /// <summary>
        /// returns one instance of SharedTestInfo per DatabaseInfo 
        /// </summary>
        /// <returns></returns>
        public List<SharedTestInfo> SplitByDatabase()
        {
            var ret = new List<SharedTestInfo>();

            foreach (var dbi in DatabaseInfos)
            {
                var sti = new SharedTestInfo();
                sti.NetworkPath = NetworkPath;
                sti.Machines = Machines;
                sti.ProcessCount = ProcessCount;
                sti.DatabaseInfos.Add(dbi);
                sti.Token = Token;

                ret.Add(sti);
            }

            return ret;
        }

        /// <summary>
        /// returns one instance of SharedTestInfo per Machine
        /// </summary>
        /// <returns></returns>
        public List<SharedTestInfo> SplitByMachine()
        {
            var ret = new List<SharedTestInfo>();

            foreach (var machine in Machines)
            {
                var sti = new SharedTestInfo();
                sti.NetworkPath = NetworkPath;
                sti.Machines.Add(machine);
                sti.ProcessCount = ProcessCount;
                sti.DatabaseInfos = DatabaseInfos;
                sti.Token = Token;

                ret.Add(sti);
            }

            return ret;
        }

        /// <summary>
        /// returns one instance of SharedTestInfo per Process. Token is set to process index
        /// </summary>
        /// <returns></returns>
        public List<SharedTestInfo> SplitByProcess()
        {
            var ret = new List<SharedTestInfo>();

            foreach (var machine in Machines)
            {
                for (int i = 0; i < ProcessCount; i++)
                {
                    var sti = new SharedTestInfo();
                    sti.NetworkPath = NetworkPath;
                    sti.Machines.Add(machine);
                    sti.ProcessCount = 1;
                    sti.DatabaseInfos = DatabaseInfos;
                    sti.Token = i.ToString("0000");

                    ret.Add(sti);
                }
            }

            return ret;
        }
    }
}


