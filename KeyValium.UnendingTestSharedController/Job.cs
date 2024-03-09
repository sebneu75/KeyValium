using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.UnendingTestSharedController
{
    internal class Job
    {
        public Job() 
        {
            Clients = new List<string>();
            JobFiles = new List<string>();
        }    

        public string Folder
        {
            get; set;
        }

        public string DatabaseFile
        {
            get; set;
        }

        public List<string> Clients
        {
            get;
            private set;
        }

        public List<string> JobFiles
        {
            get;
            private set;
        }

        public void Start()
        {
            //DeleteFile(DatabaseFile);
            //DeleteFile(DatabaseFile+".lock");
            //DeleteFile(DatabaseFile + ".lock.lock");


            // delete database
            if (File.Exists(Folder)) 
            { 
            
            }
        }
    }
}
