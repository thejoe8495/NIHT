using NIHT.Plugins.Base;
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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Net.Sockets;

namespace NIHT.Plugins.Lanscanner {
    public class LanScanner : NetworkTools, PluginInterface {
        #region IPlugin Members 
        Grid grd;
        public string Name {
            get {
                return "lanscanner";
            }
        }

        public string TabHeader {
            get {
                return "Lan Scanner";
            }
        }
        private ObservableCollection<PCLine> dataAdapter = new ObservableCollection<PCLine>();
        DataGrid data;
        private static readonly object ItemsLock = new object();

        public object getTabContent() {
            grd = new Grid();
            grd.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            grd.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Label lbl = new Label();
            lbl.SetValue(Grid.ColumnProperty, 0);
            lbl.SetValue(Grid.RowProperty, 0);
            lbl.Content = "von";
            grd.Children.Add(lbl);
            TextBox txt = new TextBox();
            txt.SetValue(Grid.ColumnProperty, 1);
            txt.SetValue(Grid.RowProperty, 0);
            txt.Name = "txt_start";
            txt.Text = "";
            txt.TextChanged += txt_ip_changed;
            IPAddress[] IpA = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            string endip = "";
            foreach (IPAddress item in IpA) {
                if (item.AddressFamily == AddressFamily.InterNetwork) {
                    txt.Text = GetNetworkAddress(item, CreateByHostBitLength(24)).ToString();
                    endip = GetBroadcastAddress(item, CreateByHostBitLength(24)).ToString();
                }
            }
            grd.Children.Add(txt);
            lbl = new Label();
            lbl.SetValue(Grid.ColumnProperty, 2);
            lbl.SetValue(Grid.RowProperty, 0);
            lbl.Content = "bis";
            grd.Children.Add(lbl);
            txt = new TextBox();
            txt.SetValue(Grid.ColumnProperty, 3);
            txt.SetValue(Grid.RowProperty, 0);
            txt.Name = "txt_end";
            txt.Text = endip;
            grd.Children.Add(txt);
            lbl = new Label();
            lbl.SetValue(Grid.ColumnProperty, 4);
            lbl.SetValue(Grid.RowProperty, 0);
            lbl.Content = "/";
            grd.Children.Add(lbl);
            txt = new TextBox();
            txt.SetValue(Grid.ColumnProperty, 5);
            txt.SetValue(Grid.RowProperty, 0);
            txt.Name = "txt_subnetz";
            txt.Text = "24";
            txt_ip_changed(null, null);
            grd.Children.Add(txt);
            Button btntest = new Button() {
                Name = "btn_test",
                Content = "Scan",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 0, 5, 0),
                Height = 30
            };
            btntest.SetValue(Grid.ColumnProperty, 6);
            btntest.SetValue(Grid.RowProperty, 0);
            btntest.Click += btntest_Click;
            grd.Children.Add(btntest);
            grd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            data = new DataGrid() {
                AutoGenerateColumns = false,
                CanUserAddRows = false
            };
            data.Columns.Add(new DataGridTextColumn() { Header = "IP", SortMemberPath = "ID", Binding = new Binding("IP"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.Columns.Add(new DataGridTextColumn() { Header = "Hostname", Binding = new Binding("Hostname"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.Columns.Add(new DataGridTextColumn() { Header = "Ports", Binding = new Binding("Ports"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.SetValue(Grid.ColumnSpanProperty, 7);
            data.SetValue(Grid.RowProperty, 1);
            BindingOperations.EnableCollectionSynchronization(dataAdapter, ItemsLock);
            Binding binding = new Binding();
            binding.Source = dataAdapter;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            bindingexp = data.SetBinding(DataGrid.ItemsSourceProperty, binding);
            grd.Children.Add(data);

            return grd;
        }

        private void txt_ip_changed(object sender, TextChangedEventArgs e) {
            TextBox txt_start = NetworkTools.FindChild<TextBox>(grd, "txt_start");
            TextBox txt_subnetz = NetworkTools.FindChild<TextBox>(grd, "txt_subnetz");
            try {
                IPAddress start = IPAddress.Parse(txt_start.Text);
                IPAddress end = GetBroadcastAddress(start, CreateByHostBitLength(int.Parse(txt_subnetz.Text)));
                TextBox txt_end = NetworkTools.FindChild<TextBox>(grd, "txt_end");
                txt_end.Text = end.ToString();
            } catch {

            }

        }

        public IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask) {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++) {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
        public IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask) {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++) {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }


        public IPAddress CreateByHostBitLength(int netPartLength) {

            if (netPartLength < 2)
                throw new ArgumentException("Number of hosts is to large for IPv4");

            Byte[] binaryMask = new byte[4];

            for (int i = 0; i < 4; i++) {
                if (i * 8 + 8 <= netPartLength)
                    binaryMask[i] = (byte)255;
                else if (i * 8 > netPartLength)
                    binaryMask[i] = (byte)0;
                else {
                    int oneLength = netPartLength - i * 8;
                    string binaryDigit =
                        String.Empty.PadLeft(oneLength, '1').PadRight(8, '0');
                    binaryMask[i] = Convert.ToByte(binaryDigit, 2);
                }
            }
            return new IPAddress(binaryMask);
        }

        BindingExpressionBase bindingexp;
        private async void btntest_Click(object sender, RoutedEventArgs e) {
            dataAdapter.Clear();
            Button button = (Button)sender;
            button.IsEnabled = false;
            TextBox txt_start = NetworkTools.FindChild<TextBox>(grd, "txt_start");
            TextBox txt_end = NetworkTools.FindChild<TextBox>(grd, "txt_end");
            int[] scanports = { 80, 443, 8443, 8080, 3128 };
            IEnumerable<IPAddress> ips = GetAllIP(IPAddress.Parse(txt_start.Text).GetAddressBytes(), IPAddress.Parse(txt_end.Text).GetAddressBytes());
            List<Task> tasks = new List<Task>();
            foreach (var item in ips) {
                Task t = Task.Run(() => {
                    if (Ping(item.ToString(), 1)) {
                        List<int> ports = new List<int>();
                        foreach (int curport in scanports) {
                            IPEndPoint ipEo = new IPEndPoint(BitConverter.ToUInt32(item.GetAddressBytes(), 0), curport);
                            try {
                                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // Verbindung vorbereiten
                                sock.Connect(ipEo);  // Verbindung aufbauen
                                ports.Add(curport);  // Positiv Text ausgeben
                                sock.Close();  //Verbindung schließen wird nicht mehr gebraucht
                            } catch (Exception) { }
                        }
                        string dns;
                        try {
                            dns = Dns.GetHostEntry(item.ToString()).HostName;
                        } catch (SocketException) {
                            dns = "";
                        }
                        dataAdapter.Add(new PCLine() { ID = BitConverter.ToUInt32( item.GetAddressBytes(),0), IP = item.ToString(), Hostname = dns, Ports = string.Join(",", ports) });
                    }
                });
                tasks.Add(t);
            }
            await Task.WhenAll(tasks.ToArray());
            button.IsEnabled = true;
        }

        public IEnumerable<IPAddress> GetAllIP(byte[] beginIP, byte[] endIP) {
            int capacity = 1;
            for (int i = 0; i < 4; i++)
                capacity *= endIP[i] - beginIP[i] + 1;

            List<IPAddress> ips = new List<IPAddress>(capacity);
            for (int i0 = beginIP[0]; i0 <= endIP[0]; i0++) {
                for (int i1 = beginIP[1]; i1 <= endIP[1]; i1++) {
                    for (int i2 = beginIP[2]; i2 <= endIP[2]; i2++) {
                        for (int i3 = beginIP[3]; i3 <= endIP[3]; i3++) {
                            ips.Add(new IPAddress(new byte[] { (byte)i0, (byte)i1, (byte)i2, (byte)i3 }));
                        }
                    }
                }
            }

            return ips;
        }


        public string Action(int action, object extra) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
