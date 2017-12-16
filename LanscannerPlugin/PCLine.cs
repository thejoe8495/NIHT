using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NIHT.Plugins.Lanscanner {
    public class PCLine {
        public long ID { get; set; }
        public IPAddress IP { get; set; }
        public string IPString { get; set; }
        public string Hostname { get; set; }
        public string Ports { get; set; }
        public string Mac { get; set; }
        public bool Ping { get; set; }
        public bool ARP { get; set; }
    }
}
