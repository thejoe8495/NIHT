using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NIHT.Plugins.Base {
    public interface PluginInterface {
        string Name { get; }
        string TabHeader { get; }
        object getTabContent();
        string Action(int action, object extra);
        
    }
}
