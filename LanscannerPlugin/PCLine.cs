using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIHT.Plugins.Lanscanner {
    public class PCLine {
        public long ID { get; set; }
        public string IP { get; set; }
        public string Hostname { get; set; }
        public string Ports { get; set; }
    }
}
