using NIHT.Plugins.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NIHT.WPFApp {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        string[] dllFileNames = null;
        public MainWindow() {
            InitializeComponent();
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                dllFileNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "Plugin.*.dll");
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames) {
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }
            Type pluginType = typeof(PluginInterface);
            ICollection<Type> pluginTypes = new List<Type>();
            foreach (Assembly assembly in assemblies) {
                if (assembly != null) {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types) {
                        if (type.IsInterface || type.IsAbstract) {
                            continue;
                        } else {
                            if (type.GetInterface(pluginType.FullName) != null) {
                                pluginTypes.Add(type);
                            }
                        }
                    }
                }
            }
            ICollection<PluginInterface> plugins = new List<PluginInterface>(pluginTypes.Count);
            foreach (Type type in pluginTypes) {
                PluginInterface plugin = (PluginInterface)Activator.CreateInstance(type);
                plugins.Add(plugin);
                TabItem item = new TabItem() {
                    Name = "tab_" + plugin.Name,
                    Header = plugin.TabHeader
                };
                item.Content = plugin.getTabContent();
                tac_tabs.Items.Insert(0, item);

            }
        }

        private void btn_activateplugin_Click(object sender, RoutedEventArgs e) {

        }
    }
}
