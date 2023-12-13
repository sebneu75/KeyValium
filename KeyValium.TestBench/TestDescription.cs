using KeyValium.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KeyValium.TestBench.Measure;

namespace KeyValium.TestBench
{
    public class TestDescription
    {
        public static string WorkingPath
        {
            get;
            set;
        }

        public static string ErrorPath
        {
            get
            {
                return Path.Combine(WorkingPath ?? "", "Errors");
            }
        }

        public static string SuccessPath
        {
            get
            {
                return Path.Combine(WorkingPath ?? "", "Successes");
            }
        }

        public TestDescription() : this("Unnamed")
        {
        }

        public TestDescription(string name)
        {
            Options = new DatabaseOptions();

            KeyCount = 16384;
            MinKeySize = 16;
            MaxKeySize = 16;
            MinValueSize = 128;
            MaxValueSize = 128;

            GenStrategy = KeyGenStrategy.Random;

            OrderInsert = KeyOrder.Random;
            OrderUpdate = KeyOrder.Random;
            OrderRead = KeyOrder.Random;
            OrderDelete = KeyOrder.Random;

            AllowDuplicateKeys = false;

            Name = name;

            Measure = new Measurer();
            Measure.TestDescription = this;

#if DEBUG
            Mode = "DEBUG";
#else
            Mode = "RELEASE";
#endif
        }

        public DatabaseOptions Options
        {
            get;
            set;
        }

        public string Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the Testdescription. Must be a valid Filename
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// optional token that will be included in the filename
        /// </summary>
        public string Token
        {
            get;
            set;
        }

        public Measurer Measure
        {
            get;
            set;
        }

        internal void Finish()
        {
            Measure.Finish();
        }

        private string _dbfilename;

        public string DbFilenameOld
        {
            get
            {
                if (_dbfilename == null)
                {
                    var keysize = "";
                    var valsize = "";
                    if (MinKeySize == MaxKeySize)
                    {
                        keysize = string.Format("[{0}]", MinKeySize);
                    }
                    else
                    {
                        keysize = string.Format("[{0}-{1}]", MinKeySize, MaxKeySize);
                    }

                    if (MinValueSize == MaxValueSize)
                    {
                        valsize = string.Format("[{0}]", MinValueSize);
                    }
                    else
                    {
                        valsize = string.Format("[{0}-{1}]", MinValueSize, MaxValueSize);
                    }

                    var sb = new StringBuilder();
                    sb.Append(Mode);
                    sb.Append("-");
                    sb.Append(Name);
                    sb.Append("-");

                    if (Token != null)
                    {
                        sb.Append(Token);
                        sb.Append("-");
                    }

                    if (ParameterName != null)
                    {
                        sb.Append(ParameterName);
                        sb.Append("-");
                        sb.Append(ParameterValue ?? "(null)");
                        sb.Append("-");
                    }

                    sb.Append(Options.PageSize);
                    sb.Append("-");
                    sb.Append(keysize);
                    sb.Append("-");
                    sb.Append(valsize);
                    sb.Append("-");

                    sb.Append(KeyCount);
                    sb.Append("-");
                    sb.Append(CommitSize);

                    sb.Append("-");
                    sb.Append(AllowDuplicateKeys ? "1" : "0");
                    sb.Append("-");

                    sb.Append((int)GenStrategy);
                    sb.Append((int)OrderDelete);
                    sb.Append((int)OrderInsert);
                    sb.Append((int)OrderRead);
                    sb.Append((int)OrderUpdate);

                    sb.Append(".kvlm");

                    //var filename = string.Format("{0}-{1}-{2}-{3}.btree", Name, DbPageSize, keysize, valsize);

                    _dbfilename = Path.Combine(WorkingPath, sb.ToString());
                }

                return _dbfilename;
            }
        }

        public string DbFilename
        {
            get
            {
                if (_dbfilename == null)
                {
                    var sb = new StringBuilder();
                    sb.Append(Name);
                    
                    if (Token != null)
                    {
                        sb.Append("-");
                        sb.Append(Token);
                    }

                    sb.Append(".kvlm");

                    _dbfilename = Path.Combine(WorkingPath, sb.ToString());
                }

                return _dbfilename;
            }
        }

        public List<KeyValuePair<byte[], byte[]>> GenerateKeys(long pos, long count)
        {
            return KeyValueGenerator.Generate(Options.PageSize, pos, count, GenStrategy, MinKeySize, MaxKeySize, MinValueSize, MaxValueSize);
        }

        public uint PageSize
        {
            get
            {
                return Options.PageSize;
            }
            set
            {
                Options.PageSize = value;
            }
        }

        public long KeyCount
        {
            get;
            set;
        }

        public KeyGenStrategy GenStrategy
        {
            get;
            set;
        }

        public int MinKeySize
        {
            get;
            set;
        }

        public int MaxKeySize
        {
            get;
            set;
        }

        public int MinValueSize
        {
            get;
            set;
        }

        public int MaxValueSize
        {
            get;
            set;
        }

        public KeyOrder OrderInsert
        {
            get;
            set;
        }

        public KeyOrder OrderUpdate
        {
            get;
            set;
        }

        public KeyOrder OrderRead
        {
            get;
            set;
        }

        public KeyOrder OrderDelete
        {
            get;
            set;
        }

        public bool AllowDuplicateKeys
        {
            get;
            set;
        }

        public int CommitSize
        {
            get;
            set;
        }

        public int TxCount
        {
            get;
            set;
        }

        public TestDescription Copy()
        {
            var ret = new TestDescription();
            ret.AllowDuplicateKeys = AllowDuplicateKeys;
            ret.CommitSize = CommitSize;
            ret.GenStrategy = GenStrategy;
            ret.KeyCount = KeyCount;
            ret.MaxKeySize = MaxKeySize;
            ret.MaxValueSize = MaxValueSize;
            ret.MinKeySize = MinKeySize;
            ret.MinValueSize = MinValueSize;
            ret.Mode = Mode;
            ret.Name = Name;
            ret.Options = Options.Copy();
            ret.OrderDelete = OrderDelete;
            ret.OrderInsert = OrderInsert;
            ret.OrderRead = OrderRead;
            ret.OrderUpdate = OrderUpdate;
            ret.PageSize = PageSize;
            ret.ParameterName = ParameterName;
            ret.ParameterValue = ParameterValue;
            ret.Token = Token;
            ret.TxCount = TxCount;

            return ret;
        }

        #region Serialization

        private static JsonSerializerOptions _seroptions;

        private static JsonSerializerOptions SerializationOptions
        {
            get
            {
                if (_seroptions == null)
                {
                    _seroptions = new JsonSerializerOptions()
                    {
                        AllowTrailingCommas = true,
                        IgnoreReadOnlyProperties = false,
                        IncludeFields = false,
                        ReferenceHandler = ReferenceHandler.Preserve,
                        WriteIndented = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                    };
                }

                return _seroptions;
            }
        }

        public string ParameterName
        {
            get;
            set;
        }

        public string ParameterValue
        {
            get;
            set;
        }

        public static List<TestDescription> LoadAll(string path = null)
        {
            path = path ?? WorkingPath;

            var ret = new List<TestDescription>();

            var files = Directory.GetFiles(path, "*.td");
            foreach (var file in files)
            {
                var td = Load(file);
                ret.Add(td);
            }

            return ret;
        }

        public static TestDescription Load(string path)
        {
            try
            {
                using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return JsonSerializer.Deserialize<TestDescription>(reader, SerializationOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading: " + ex.Message);
            }

            return null;
        }

        public void Save(string path = null)
        {
            try
            {
                path = path ?? WorkingPath;

                var file = Path.Combine(path, Path.GetFileName(DbFilename) + ".td");

                using (var writer = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    JsonSerializer.Serialize(writer, this, SerializationOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving: " + ex.Message);
            }
        }

        #endregion
    }
}
