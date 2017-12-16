using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Dns;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Gre;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Igmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.IpV6;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIHT.Plugins.Lanscanner {
    class PcapWrapper {


        /// <summary>
        /// This function build an ARP over Ethernet packet.
        /// </summary>
        private static Packet BuildArpPacket() {
            EthernetLayer ethernetLayer =
                new EthernetLayer {
                    Source = new MacAddress("36:63:35:66:62:62"),
                    Destination = new MacAddress("FF:FF:FF:FF:FF:FF"),
                    EtherType = EthernetType.None, // Will be filled automatically.
                };

            ArpLayer arpLayer =
                new ArpLayer {
                    ProtocolType = EthernetType.IpV4,
                    Operation = ArpOperation.Request,
                    SenderHardwareAddress = new byte[] { 36, 63, 35, 66, 62, 62 }.AsReadOnly(), // 03:03:03:03:03:03.
                    SenderProtocolAddress = new byte[] { 192, 168, 10, 203 }.AsReadOnly(), // 1.2.3.4.
                    TargetHardwareAddress = new byte[] { 255, 255, 255, 255, 255, 255 }.AsReadOnly(), // 04:04:04:04:04:04.
                    TargetProtocolAddress = new byte[] { 255, 255, 255, 255 }.AsReadOnly(), // 11.22.33.44.
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);

            return builder.Build(DateTime.Now);
        }

        /// <summary>
        /// This function build a VLanTaggedFrame over Ethernet with payload packet.
        /// </summary>
        private static Packet BuildVLanTaggedFramePacket() {
            EthernetLayer ethernetLayer =
                new EthernetLayer {
                    Source = new MacAddress("01:01:01:01:01:01"),
                    Destination = new MacAddress("02:02:02:02:02:02"),
                    EtherType = EthernetType.None, // Will be filled automatically.
                };

            VLanTaggedFrameLayer vLanTaggedFrameLayer =
                new VLanTaggedFrameLayer {
                    PriorityCodePoint = ClassOfService.Background,
                    CanonicalFormatIndicator = false,
                    VLanIdentifier = 50,
                    EtherType = EthernetType.IpV4,
                };

            PayloadLayer payloadLayer =
                new PayloadLayer {
                    Data = new Datagram(Encoding.ASCII.GetBytes("hello world")),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, vLanTaggedFrameLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }
    }
}
