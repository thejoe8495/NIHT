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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

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
        const int MAXLEN_PHYSADDR = 8;

        // Define the MIB_IPNETROW structure.
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Declare the GetIpNetTable function.
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(IntPtr pIpNetTable, [MarshalAs(UnmanagedType.U4)] ref int pdwSize, bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);
        
        const int ERROR_INSUFFICIENT_BUFFER = 122;
        public void arptable() {
            int bytesNeeded = 0;
            int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);
            if (result != ERROR_INSUFFICIENT_BUFFER) {
                throw new Win32Exception(result);
            }
            IntPtr buffer = IntPtr.Zero;
            try {
                buffer = Marshal.AllocCoTaskMem(bytesNeeded);
                result = GetIpNetTable(buffer, ref bytesNeeded, false);
                if (result != 0)
                    return;
                
                int entries = Marshal.ReadInt32(buffer);
                
                IntPtr currentBuffer = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));

                MIB_IPNETROW[] table = new MIB_IPNETROW[entries];
                for (int index = 0; index < entries; index++) 
                    table[index] = (MIB_IPNETROW)Marshal.PtrToStructure(new IntPtr(currentBuffer.ToInt64() + (index * Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));

                for (int index = 0; index < entries; index++) {
                    MIB_IPNETROW row = table[index];
                    IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                    if (!macs.Keys.Contains(ip))
                        macs.Add(ip, row.mac0.ToString("X2") + ":" + row.mac1.ToString("X2") + ":" + row.mac2.ToString("X2") + ":" + row.mac3.ToString("X2") + ":" + row.mac4.ToString("X2") + ":" + row.mac5.ToString("X2"));
                }
            } finally {
                // Release the memory.
                FreeMibTable(buffer);
            }
        }
        private Dictionary<IPAddress, string> macs = new Dictionary<IPAddress, string>();
        DispatcherTimer timer;
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
                if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
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
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(tick);
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
            data = new DataGrid() {
                AutoGenerateColumns = false,
                CanUserAddRows = false
            };
            data.Columns.Add(new DataGridTextColumn() { Header = "IP", SortMemberPath = "ID", Binding = new Binding("IP"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.Columns.Add(new DataGridTextColumn() { Header = "Hostname", Binding = new Binding("Hostname"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.Columns.Add(new DataGridTextColumn() { Header = "Ports", Binding = new Binding("Ports"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            data.Columns.Add(new DataGridTextColumn() { Header = "Mac", Binding = new Binding("Mac"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
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

        private void tick(object sender, EventArgs e) {
            data.Items.Refresh();
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
            Button button = (Button)sender;
            if ((string)button.Content == "Scan") {
                button.Content = "Stop";
                dataAdapter.Clear();
                Startscan();
            } else {
                button.Content = "Scan";
                button.IsEnabled = false;
                tokenSource.Cancel();
                button.IsEnabled = true;
            }

        }
        List<Task> tasks = new List<Task>();
        List<Thread> threads = new List<Thread>();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        private async void Startscan() {
            TextBox txt_start = NetworkTools.FindChild<TextBox>(grd, "txt_start");
            TextBox txt_end = NetworkTools.FindChild<TextBox>(grd, "txt_end");
            int[] scanports = { 80, 443, 8443, 8080, 3128 };
            IEnumerable<IPAddress> ips = GetAllIP(IPAddress.Parse(txt_start.Text).GetAddressBytes(), IPAddress.Parse(txt_end.Text).GetAddressBytes());
            foreach (var item in ips) {
                CancellationToken token = tokenSource.Token;
                Task t = Task.Factory.StartNew(() => {
                    if (Ping(item.ToString(), 1)) {
                        if (token.IsCancellationRequested) return;
                        PCLine line = new PCLine() { ID = BitConverter.ToUInt32(item.GetAddressBytes(), 0), IP = item, IPString = item.ToString(), Ping = true };
                        dataAdapter.Add(line);
                        List<int> ports = new List<int>();
                        foreach (int curport in scanports) {
                            IPEndPoint ipEo = new IPEndPoint(BitConverter.ToUInt32(item.GetAddressBytes(), 0), curport);
                            try {
                                Socket sock = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  // Verbindung vorbereiten
                                sock.Connect(ipEo);  // Verbindung aufbauen
                                ports.Add(curport);  // Positiv Text ausgeben
                                sock.Close();  //Verbindung schließen wird nicht mehr gebraucht
                            } catch (Exception) { }
                        }
                        line.Ports = string.Join(",", ports);
                        if (token.IsCancellationRequested) return;
                        string dns = "";
                        try {
                            dns = Dns.GetHostEntry(item.ToString()).HostName;
                        } catch (Exception) {
                        }
                        line.Hostname = dns;
                        arptable();
                        if (macs.Keys.Contains(item) && macs[line.IP] != "00:00:00:00:00:00")
                            line.Mac = macs[line.IP];
                    }
                }, token);
                tasks.Add(t);
            }
            await Task.WhenAll(tasks.ToArray());
            arptable();
            foreach (KeyValuePair<IPAddress, string> mac in macs) {
                bool foundkey = false;
                foreach (PCLine pcline in dataAdapter) {
                    if (mac.Key.ToString() == pcline.IPString) {
                        pcline.Mac = macs[pcline.IP];
                        foundkey = true;
                    }
                }
                if (foundkey == false && mac.Value != "00:00:00:00:00:00") {
                    PCLine line = new PCLine() { ID = BitConverter.ToUInt32(mac.Key.GetAddressBytes(), 0), IP = mac.Key, IPString = mac.Key.ToString(), Mac = mac.Value, ARP = true };
                    dataAdapter.Add(line);
                }
            }
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
