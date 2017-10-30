using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NIHT.Plugins.Base {
    public class LanInfo : NetworkTools, PluginInterface {
        #region IPlugin Members 
        Grid grd;
        public string Name {
            get {
                return "laninfo";
            }
        }

        public string TabHeader {
            get {
                return "Lan Infos";
            }
        }

        public object getTabContent() {
            grd = new Grid();
            grd.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Button btntest = new Button() {
                Name = "btn_" ,
                Content = "Start Test",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 0, 5, 0),
                Height = 30
            };
            btntest.SetValue(Grid.ColumnProperty, 0);
            btntest.SetValue(Grid.RowProperty, 0);
            btntest.Click += btntest_Click;
            grd.Children.Add(btntest);
            createline(grd, 1, "1 Ping Gateway", GetDefaultGateway(),false);
            createline(grd, 2, "2 Ping DNS Server", GetDNSServer(), false);
            createline(grd, 3, "3 Ping DNS Google", "8.8.8.8", false);
            createline(grd, 4, "4 NSLookup DNS Systemstandard", "www.google.de", true);
            createline(grd, 5, "5 NSLookup DNS Google", "www.google.de", true);
            Canvas canv;
            IWebProxy proxy = WebRequest.GetSystemWebProxy();
            if (proxy.GetProxy(new Uri("http://www.asfdfasdas.neto")).Host != "www.asfdfasdas.neto") {
                createline(grd, 6, "6 Ping Proxyserver", proxy.GetProxy(new Uri("http://www.asfdfasdas.neto")).Host, true);
                FindChild<TextBox>(grd, "txt_res6").Text = proxy.GetProxy(new Uri("http://www.asfdfasdas.neto")).AbsoluteUri;
            } else {
                createline(grd, 6, "6 Ping Proxyserver", "", false);
                canv = FindChild<Canvas>(grd, "poi_6");
                canv.Children.Add(getElipse(""));
            }

            IPAddress[] IpA = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            createline(grd, 7, "7 Eigene IP Adresse", string.Join("; ",  IpA.Select(m => m.ToString())), true);
            canv = FindChild<Canvas>(grd, "poi_7");
            canv.Children.Add(getElipse(""));
            return grd;
        }
        public string GetDefaultGateway() {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                // .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault().ToString();
        }
        public string GetDNSServer() {
            return string.Join("; ", NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.DnsAddresses)
                .Select(g => g?.ToString())
                .Where(a => a != null));
                // .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                
        }
        private void createline(Grid grd, int row, string name, string inhalt, bool resultbox) {
            grd.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Label lbl = new Label();
            lbl.SetValue(Grid.ColumnProperty, 0);
            lbl.SetValue(Grid.RowProperty, row);
            lbl.Content = name;
            lbl.Name = "lbl_" + row;
            grd.Children.Add(lbl);
            Canvas canv = new Canvas() {
                Height = 30,
                Width = 30,
                Name = "poi_" + row
            };
            canv.SetValue(Grid.ColumnProperty, 1);
            canv.SetValue(Grid.RowProperty, row);
            canv.Children.Add(getElipse("yellow"));
            grd.Children.Add(canv);
            TextBox txt = new TextBox();
            txt.SetValue(Grid.ColumnProperty, 2);
            txt.SetValue(Grid.RowProperty, row);
            txt.Name = "txt_" + row;
            txt.Text = inhalt;
            grd.Children.Add(txt);
            if (resultbox) {
                txt = new TextBox();
                txt.SetValue(Grid.ColumnProperty, 3);
                txt.SetValue(Grid.RowProperty, row);
                txt.Name = "txt_res" + row;
                grd.Children.Add(txt);
            }
        }

        private void btntest_Click(object sender, RoutedEventArgs e) {
            Canvas canv = FindChild<Canvas>(grd, "poi_1");
            canv.Children.Add(getElipse(getpingresultcolor("txt_1")));
            canv = FindChild<Canvas>(grd, "poi_2");
            canv.Children.Add(getElipse(getpingresultcolor("txt_2")));
            canv = FindChild<Canvas>(grd, "poi_3");
            canv.Children.Add(getElipse(getpingresultcolor("txt_3")));

            canv = FindChild<Canvas>(grd, "poi_4");
            string res = DNSLookup(FindChild<TextBox>(grd, "txt_4").Text);
            if (res != "") {
                canv.Children.Add(getElipse("green"));
                FindChild<TextBox>(grd, "txt_res4").Text = res;
            } else
                canv.Children.Add(getElipse("red"));
            canv = FindChild<Canvas>(grd, "poi_5");
            res = DNSLookup(FindChild<TextBox>(grd, "txt_5").Text);
            if (res != "") {
                canv.Children.Add(getElipse("green"));
                FindChild<TextBox>(grd, "txt_res5").Text = res;
            } else
                canv.Children.Add(getElipse("red"));
            canv = FindChild<Canvas>(grd, "poi_6");
            canv.Children.Add(getElipse(getpingresultcolor("txt_6")));
        }
        private string getpingresultcolor(string feldname) {
            string ip = FindChild<TextBox>(grd, feldname).Text;
            if (ip == "")
                return "";
            else if (Ping(ip))
                return "green";
            else
                return "red";
        }

        private Ellipse getElipse(string Farbe) {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            if (Farbe == "red") {
                brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            } else if (Farbe == "green") {
                brush = new SolidColorBrush(Color.FromRgb(34, 177, 76));
            } else if (Farbe == "yellow") {
                brush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            }
            return new Ellipse() {
                Fill = brush,
                Height = 15,
                Width = 15,
                Stroke = brush
            };
        }

        public string Action(int action, object extra) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
