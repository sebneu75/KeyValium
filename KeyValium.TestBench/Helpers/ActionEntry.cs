using System;
using System.Linq;

namespace KeyValium.TestBench.Helpers
{
    internal class ActionEntry
    {
        public ActionType Type;

        public KVEntry Entry;

        public PathToKey Key;

        public string Line;

        public static ActionEntry Parse(string line)
        {
            var ret = new ActionEntry();

            ret.Line = line;

            if (line.Trim().StartsWith('#'))
            {
                ret.Type = ActionType.None;
            }
            else
            {
                var vals = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                ret.Type = Enum.Parse<ActionType>(vals[0].Trim());

                if (ret.Type != ActionType.ERROR)
                {
                    ret.Key = GetKeyPath(vals[1]);

                    ret.Entry = new KVEntry();
                    ret.Entry.KeyLength = GetInt(vals[2]);
                    ret.Entry.Key = KeyValueGenerator.GetBytes(KeyGenStrategy.Sequential, ret.Key == null ? 0 : ret.Key.Path.Last(), ret.Entry.KeyLength, ret.Entry.KeyLength);
                    ret.Entry.ValueSeed = GetInt(vals[3]);
                    ret.Entry.ValueLength = GetInt(vals[4]);
                    ret.Entry.Value = KeyValueGenerator.GetSeededBytes(ret.Entry.ValueSeed, ret.Entry.ValueLength);
                }
            }
            return ret;
        }

        private static int GetInt(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return 0;
            }

            return int.Parse(val.Trim());
        }

        private static long GetLong(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return 0;
            }

            return long.Parse(val.Trim());
        }

        private static PathToKey GetKeyPath(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return null;
            }

            var path = new List<long>();

            var vals = val.Split(',');

            foreach (var k in vals)
            {
                path.Add(long.Parse(k.Trim()));
            }

            return new PathToKey(path);
        }
    }
}