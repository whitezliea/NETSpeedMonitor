using System.Runtime.Versioning;
using NetSpeed_Linux.NetInfo;
using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.ProcInfo;
using NETSpeedMonitor.myLogger;

namespace NETSpeedMonitor.CoreZ.Linux.NetInfo;

[SupportedOSPlatform("Linux")]
public class NetInfoWrokerL
{
    public static void proc_work()
    {
        if (!ProcInfoWarpper.GetProcessInfoList(ref CoreDataWorker.ProcDict))
        {
            LoggerWorker.Instance._logger.Error("GetProcessInfoList error!");
            return;
        }
    }
    public static void mac_work()
    {
        LocalNetInfo.Instance.GetAllMacList(ref  CoreDataWorker.MACList);
    }

    public static void tcp_work()
    {
        TcpfileInfo.tcpInfoRead();
    }

    public static void udp_work()
    {
        UdpfileInfo.udpInfoRead();
    }

    public static void inode_work()
    {
        SystemInfoZ.Inode2pid_work();
    }
}