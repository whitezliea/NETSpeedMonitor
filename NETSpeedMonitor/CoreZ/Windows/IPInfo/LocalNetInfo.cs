using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NETSpeedMonitor.myLogger;

namespace NETSpeedMonitor.CoreZ.Windows.IPInfo
{
    internal class LocalNetInfo
    {
        public static LocalNetInfo _instance = null!;
        private static readonly object _lock = new object();
        //public readonly List<PhysicalAddress> MACList = new();
        private LocalNetInfo()
        {

        }

        /// <summary>
        /// 懒汉式单例模式
        /// </summary>
        public static LocalNetInfo Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LocalNetInfo();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 获取本地计算机上的所有TCP连接信息
        /// </summary>
        public void GetTcpAllList()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            LoggerWorker.Instance._logger.Debug("Computer name: {0}", properties.HostName);
            LoggerWorker.Instance._logger.Debug("Domain name:   {0}", properties.DomainName);
            TcpConnectionInformation[] LocalNetInfo = properties.GetActiveTcpConnections();

            LoggerWorker.Instance._logger.Verbose("LocalAddress\t" + "LocalPort\t" + "RemoteAddress\t" + "RemotePort\t" + "State\t");
            // 输出每个连接的信息
            foreach (var tcpconnection in LocalNetInfo)
            {
                LoggerWorker.Instance._logger.Verbose(tcpconnection.LocalEndPoint.Address + "\t" +
                                  tcpconnection.LocalEndPoint.Port + "\t\t" +
                                  tcpconnection.RemoteEndPoint.Address + "\t" +
                                  tcpconnection.RemoteEndPoint.Port + "\t\t" +
                                  tcpconnection.State + "\t");
            }
;
            LoggerWorker.Instance._logger.Verbose("");
        }

        public void GetUdpAllList()
        {
            LoggerWorker.Instance._logger.Verbose("Active UDP Listeners");
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = properties.GetActiveUdpListeners();
            foreach (IPEndPoint e in endPoints)
            {
                LoggerWorker.Instance._logger.Verbose(e.ToString());
            }
            LoggerWorker.Instance._logger.Verbose("");
        }

        public void GetAllMacList(ref HashSet<string> MACList, bool isPrint = true)
        {
            // 获取当前计算机的所有网络接口
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            // 遍历每个网络接口并输出MAC地址
            foreach (var networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties properties = networkInterface.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            //IPList.Add(ip.Address);
                            LoggerWorker.Instance._logger.Verbose($"Interface: {networkInterface.Description}, IPv4 Address: {ip.Address}");
                        }
                    }
                }

                var tmp = networkInterface.GetPhysicalAddress();
                if (tmp == null || String.IsNullOrEmpty(tmp.ToString()))
                {
                    continue;
                }
                MACList.Add(tmp.ToString());
                if (isPrint)
                {
                    LoggerWorker.Instance._logger.Debug($"Interface: {networkInterface.Name}, MAC Address: {networkInterface.GetPhysicalAddress()}");
                }
            }
            LoggerWorker.Instance._logger.Verbose("");
        }
    }
}
