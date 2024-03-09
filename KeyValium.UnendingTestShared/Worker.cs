using KeyValium.Frontends;
using KeyValium.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestShared
{
    internal class Worker
    {
        public Worker()
        {

        }

        public static void Run(string jobfile)
        {
            try
            {
                var dbfilename = "";
                var lines = new List<string[]>();

                using (var reader = new StreamReader(jobfile))
                {
                    dbfilename = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var vals = line.Split('|');
                        if (vals.Length == 3)
                        {
                            lines.Add(vals);
                        }
                    }
                }

                DoWork(dbfilename, lines);
            }
            catch (Exception ex)
            {
                try
                {
                    using (var writer = new StreamWriter(jobfile + ".error"))
                    {
                        writer.WriteLine(ex);
                    }
                }
                catch(Exception ex2) 
                { 
                    Console.WriteLine(ex2);                
                }
            }
            finally
            {
                File.Delete(jobfile);
            }
        }

        private static void DoWork(string dbfilename, List<string[]> lines)
        {
            var rnd = new Random();

            var options = new DatabaseOptions();
            options.InternalSharingMode = InternalSharingModes.SharedNetwork;

            using (var md = KvMultiDictionary.Open(dbfilename, options))
            {
                using (var dict1 = md.EnsureDictionary<string, string>("Dictionary1"))
                using (var dict2 = md.EnsureDictionary<string, string>("Dictionary2"))
                using (var dict3 = md.EnsureDictionary<string, string>("Dictionary3"))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i];

                        switch (line[0])
                        {
                            case "1":
                                dict1[line[1]] = line[2];
                                break;
                            case "2":
                                dict2[line[1]] = line[2];
                                break;
                            case "3":
                                dict3[line[1]] = line[2];
                                break;
                            default:
                                break;
                        }

                        if ((i + 1) % 1 == 0)
                        {
                            Console.WriteLine("Processed line {0}/{1}.", i + 1, lines.Count);
                        }

                        // random delay
                        //Thread.Sleep(rnd.Next(1000) + 50);
                    }
                }
            }
        }
    }
}
