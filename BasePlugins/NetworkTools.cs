using Heijden.DNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NIHT.Plugins.Base {
    public class NetworkTools {
        public static bool Ping(string ip) {
            return Ping(ip, 4);
        }
        public static bool Ping(string ip, int menge) {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "abcdefghijklmnopqrstuvwxyzabcdef";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            int success = 0;
            Console.WriteLine("Ping wird ausgeführt für " + ip + " mit 32 Bytes Daten:");
            for (int i = 0; i < menge; i++) {
                PingReply reply = pingSender.Send(ip, timeout, buffer, options);
                if (reply.Status == IPStatus.Success) {
                    Console.WriteLine("Antwort von {0} Bytes = {1} Zeit {2}ms TTL = {3}", ip, reply.Buffer.Length, reply.RoundtripTime, reply.Options.Ttl);
                    success++;
                }else
                    Console.WriteLine("Zeitüberschreitung");
            }
            if (success > 0) return true;
            return false;
        }
        public static string DNSLookup(string hostNameOrAddress) {
            try {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);

                IPAddress[] ips = hostEntry.AddressList;

                return string.Join("; ", ips.Select(m => m.ToString()));
            } catch (System.Net.Sockets.SocketException) {
                return "";
            }
        }
        public static string DNSLookupCustom(string hostNameOrAddress, string DNSServer) {
            Resolver resolver = new Resolver();
            Stopwatch sw = new Stopwatch();

            sw.Start();
            Response response = resolver.Query(hostNameOrAddress, QType.A, QClass.IN);
            sw.Stop();

            if (response.Error != "" || (response.RecordsA.Length == 0 && response.RecordsAAAA.Length == 0)) {
                Console.WriteLine(";; " + response.Error);
                return "";
            } else {
                return response.RecordsA[0].ToString();
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject {
            T foundChild = null;
            System.Collections.IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children) {
                if (child is DependencyObject) {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name.IsEqual(childName)) {
                        try {
                            foundChild = (T)child;
                        } catch {
                        }
                    } else
                        foundChild = FindChild<T>(depChild, childName);
                    if (foundChild != null) return foundChild;
                }
            }
            return foundChild;
        }
    }
}
