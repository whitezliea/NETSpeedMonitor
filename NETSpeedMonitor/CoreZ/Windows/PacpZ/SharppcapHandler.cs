using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.myLogger;

namespace NETSpeedMonitor.CoreZ.Windows.PacpZ
{
    internal class SharppcapHandler
    {
        private static SharppcapHandler _instance = null!;
        private static readonly object _lock = new object();
        private readonly List<ICaptureDevice> Cdevices = new();
        private SharppcapHandler() 
        {
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            if (devices == null || devices.Count < 1)
            {
                LoggerWorker.Instance._logger.Error("No capture devices found");
                return;
            }

            foreach (var device in devices)
            {
                //Console.WriteLine(device.ToString());
                if (device.MacAddress != null)
                {
                    Cdevices.Add(device);
                }
            }
        }
        ~SharppcapHandler()
        {
            foreach (var Cdevice in Cdevices)
            {
                // 停止捕获数据包
                Cdevice.StopCapture();
            }
        }

        /// <summary>
        /// 懒汉式单例模式
        /// </summary>
        public static SharppcapHandler Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SharppcapHandler();
                    }
                    return _instance;
                }
            }
        }

        public void CaputreHandler(bool isCapture = true)
        {
            if (!isCapture)
            {
                LoggerWorker.Instance._logger.Warning("do not anything");
                return;
            }

            foreach (var Cdevice in Cdevices)
            {
                LoggerWorker.Instance._logger.Debug((Cdevice.ToString() == null ? "null" : Cdevice.ToString())!);
                //打开设备
                Cdevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPakcetArrival);
                int readTimeoutMilliseconds = 1000;
                Cdevice.Open(mode: DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);
                //开始捕获数据包
                Cdevice.StartCapture();
            }
            //LoggerWorker.Instance._logger.Verbose("Press Enter to stop capturing...");
            Console.ReadLine();
        }

        void device_OnPakcetArrival(object sender, PacketCapture e)
        {
            var rawPacket = e.GetPacket();

            PhysicalAddress soucreMAC = default!;
            PhysicalAddress destionMAC = default!;

            IPAddress sourceIP = default!;
            IPAddress destionIP = default!;

            ushort sourcePort = default!;
            ushort destPort = default!;

            int packetLength = 0;
            bool isTCP = true;

            #region 网络数据包解析
            if (rawPacket != null)
            {
                Packet packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
                LoggerWorker.Instance._logger.Debug("----------------------Data-----------------------------");
                LoggerWorker.Instance._logger.Debug("packet's TotalPacketLength: {0}", packet.TotalPacketLength);
                packetLength = packet.TotalPacketLength;
                //Source MAC <---> Des MAC | source port <---> remote port | packetLength
                var ethernetPacket = packet.Extract<EthernetPacket>();
                if (ethernetPacket != null)
                {
                    soucreMAC = ethernetPacket.SourceHardwareAddress;
                    destionMAC = ethernetPacket.DestinationHardwareAddress;
                    LoggerWorker.Instance._logger.Debug("-------------------MAC Layer!------------------------");
                    LoggerWorker.Instance._logger.Debug(ethernetPacket.ToString());
                    LoggerWorker.Instance._logger.Debug("MAC Layer:{0} <--> {1}",
                        ethernetPacket.SourceHardwareAddress,
                        ethernetPacket.DestinationHardwareAddress);
                    LoggerWorker.Instance._logger.Debug("-----------------------------------------------------");
                }
                if (ethernetPacket?.Type != EthernetType.IPv4)
                {
                    LoggerWorker.Instance._logger.Debug("It is not IPV4");
                    return;
                }
                var ipPacket = packet.Extract<IPPacket>();
                if (ipPacket != null)
                {
                    sourceIP = ipPacket.SourceAddress;
                    destionIP = ipPacket.DestinationAddress;
                    LoggerWorker.Instance._logger.Debug("-------------------IP Layer!-------------------------");
                    LoggerWorker.Instance._logger.Debug(ipPacket.ToString());
                    LoggerWorker.Instance._logger.Debug("IP Packet:{0} <--> {1}",
                        ipPacket.SourceAddress,
                        ipPacket.DestinationAddress);
                    LoggerWorker.Instance._logger.Debug("-----------------------------------------------------");
                }
                var udpPacket = packet.Extract<UdpPacket>();
                if (udpPacket != null)
                {
                    isTCP = false;
                    sourcePort = udpPacket.SourcePort;
                    destPort = udpPacket.DestinationPort;
                    LoggerWorker.Instance._logger.Debug("-------------------Transfer Layer!----------------------");
                    LoggerWorker.Instance._logger.Debug(udpPacket.ToString());
                    LoggerWorker.Instance._logger.Debug("UDP Packet:{0} <--> {1}",
                        udpPacket.SourcePort,
                        udpPacket.DestinationPort);
                    LoggerWorker.Instance._logger.Debug("----------------------------------------------------");
                }
                var tcpPacket = packet.Extract<TcpPacket>();
                if (tcpPacket != null)
                {
                    sourcePort = tcpPacket.SourcePort;
                    destPort = tcpPacket.DestinationPort;
                    LoggerWorker.Instance._logger.Debug("-------------------Transfer Layer packet!----------------------");
                    LoggerWorker.Instance._logger.Debug(tcpPacket.ToString());
                    LoggerWorker.Instance._logger.Debug("TCP Packet:{0}<-->{1}",
                        tcpPacket.SourcePort,
                        tcpPacket.DestinationPort
                        );
                    //LoggerWorker.Instance._logger.Debug("----------------------------------------------------");
                    // Console.WriteLine();
                }
            }
            #endregion

            CoreDataWorker.packetsQueue.Enqueue(new PcapinfoZ(soucreMAC.ToString(), destionMAC.ToString(),
                                               sourceIP.ToString(), destionIP.ToString(),
                                               sourcePort, destPort,
                                               isTCP, packetLength));
            return;

           
        }
    }
}


/*
认定上行流量：本地IP向外发送数据
1.首先确定MAC地址，如果源MAC地址是本地的，则是向外发送数据

认定下行流量：外部服务器像内部发送数据
1.首先确定MAC地址，如果源MAC地址不是本地的，则是向外发送数据


 */



/*
 SAMPLE DATA:
----------------------Data-----------------------------
packet's TotalPacketLength: 107
-------------------MAC Layer!------------------------
[EthernetPacket: SourceHardwareAddress=d8:bb:c1:de:cb:a0, DestinationHardwareAddress=54:52:84:8d:e2:32, Type=IPv4][IPv4Packet: SourceAddress=192.168.3.22, DestinationAddress=175.36.187.234, HeaderLength=5, Protocol=Udp, TimeToLive=128][UDPPacket: SourcePort=38097, DestinationPort=48320]
MAC Layer:D8BBC1DECBA0 <--> 5452848DE232
-----------------------------------------------------
-------------------IP Layer!-------------------------
[IPv4Packet: SourceAddress=192.168.3.22, DestinationAddress=175.36.187.234, HeaderLength=5, Protocol=Udp, TimeToLive=128][UDPPacket: SourcePort=38097, DestinationPort=48320]
IP Packet:192.168.3.22 <--> 175.36.187.234
-----------------------------------------------------
-------------------Transfer Layer!----------------------
[UDPPacket: SourcePort=38097, DestinationPort=48320]
UDP Packet:38097 <--> 48320
----------------------------------------------------

----------------------Data-----------------------------
packet's TotalPacketLength: 55
-------------------MAC Layer!------------------------
[EthernetPacket: SourceHardwareAddress=d8:bb:c1:de:cb:a0, DestinationHardwareAddress=54:52:84:8d:e2:32, Type=IPv4][IPv4Packet: SourceAddress=192.168.3.22, DestinationAddress=183.131.147.28, HeaderLength=5, Protocol=Tcp, TimeToLive=128][TCPPacket: SourcePort=51277, DestinationPort=443, Flags={ack[2151969857 (0x80447441)]}]
MAC Layer:D8BBC1DECBA0 <--> 5452848DE232
-----------------------------------------------------
-------------------IP Layer!-------------------------
[IPv4Packet: SourceAddress=192.168.3.22, DestinationAddress=183.131.147.28, HeaderLength=5, Protocol=Tcp, TimeToLive=128][TCPPacket: SourcePort=51277, DestinationPort=443, Flags={ack[2151969857 (0x80447441)]}]
IP Packet:192.168.3.22 <--> 183.131.147.28
-----------------------------------------------------
-------------------Transfer Layer packet!----------------------
[TCPPacket: SourcePort=51277, DestinationPort=443, Flags={ack[2151969857 (0x80447441)]}]
TCP Packet:51277<-->443
----------------------------------------------------
 */ 