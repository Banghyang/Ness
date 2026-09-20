using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NessServer.Core
{
    public class ConfigImport
    {
        public string Ip { get; }
        public int Port { get; }
        public string ServerPassword { get; set; }
        public Crypt Crypt { get; set; }
        public int MaxClients { get; set; }

        public ConfigImport()
        {
            string[] config = File.ReadAllLines("Config.txt");
            Ip = config[0];
            Port = int.Parse(config[1]);
            MaxClients = int.Parse(config[2]);
        }
    }


}
