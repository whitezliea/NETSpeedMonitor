using System.Runtime.Versioning;
using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.ProcInfo;
using NETSpeedMonitor.myLogger;

namespace NETSpeedMonitor.CoreZ.Windows.NetInfo;

[SupportedOSPlatform("Windows")]
public class NetInfoWorkerW
{
    public static void proc_work()
    {
        if (!ProcInfoWarpper.GetProcessInfoList(ref CoreDataWorker.ProcDict))
        {
            LoggerWorker.Instance._logger.Error("GetProcessInfoList error!");
            return;
        }
        LoggerWorker.Instance._logger.Verbose("");
    }

    public static void mac_work()
    {
        LocalNetInfo.Instance.GetAllMacList(ref CoreDataWorker.MACList);
        LoggerWorker.Instance._logger.Verbose("");
    }

    public static void udp_work()
    {
        List<Win_NETInfo.UDP_INFO_PID> udpinfolist = new();
        if (!NetInfoWrapper.GetUdpRowList(ref udpinfolist))
        {
            LoggerWorker.Instance._logger.Error("get udp list error");
        }

        foreach (var udpinfo in udpinfolist)
        {
            CoreDataWorker.ProcDict.TryGetValue(udpinfo.dwpid, out var ProcName);
            ProcName = (ProcName == null) ? "unknown" : ProcName;
            LoggerWorker.Instance._logger.Verbose("{0}<-->{1}-->{2}:{3}",
                udpinfo.dwpid,
                ProcName,
                udpinfo.dwlocaladdr,
                udpinfo.dwlocalport);

            var result = CoreDataWorker.Udp2pid.TryGetValue(udpinfo.dwlocalport, out var oldvalue);
            if (!result) //没有找到变量
            {
                HashSet<(string, int)> udptmp = new();
                udptmp.Add((udpinfo.dwlocaladdr, udpinfo.dwpid));
                CoreDataWorker.Udp2pid.TryAdd(udpinfo.dwlocalport, udptmp);
            }
            else
            {
                // 防止内存泄漏
                if (oldvalue?.Count > 1024)
                {
                    oldvalue.Clear();
                    LoggerWorker.Instance._logger.Warning(udpinfo.dwlocaladdr + "的udp连接表已满，需要强制清理防止内存泄漏");
                }
                //if(!Udp2pid[udpinfo.dwlocalport].Contains((udpinfo.dwlocaladdr, udpinfo.dwpid)))
                oldvalue?.Add((udpinfo.dwlocaladdr, udpinfo.dwpid));
            }

        }
        LoggerWorker.Instance._logger.Verbose("");
    }

    public static void tcp_work()
    {
        List<Win_NETInfo.TCP_INFO_PID> tcpinfolist = new();
        if (!NetInfoWrapper.GetTcpRowList(ref tcpinfolist))
        {
            LoggerWorker.Instance._logger.Error("get tcp list error");
            return;
        }

        foreach (var tcpinfo in tcpinfolist)
        {
            int proc_id = tcpinfo.dwpid;

            var result = CoreDataWorker.ProxyInfo.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var value1);
            if (result) //local
            {
                if (tcpinfo.dwpid != 0) //检测到了代理软件进程pid
                {

                    CoreDataWorker.ProxyInfo.TryUpdate((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport),
                        tcpinfo.dwpid, value1);
                }
                else                    //使用之前的pid
                {
                    // ProxyInfo.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var oldvalue);
                    proc_id = value1;
                }
            }


            result = CoreDataWorker.ProxyInfo.TryGetValue((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport), out var value2);
            if (result) //remote
            {
                if (tcpinfo.dwpid != 0) //检测到了代理软件进程pid
                {
                    CoreDataWorker.ProxyInfo.TryUpdate((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport),
                        tcpinfo.dwpid, value2);
                }
                else                    //使用之前的pid
                {
                    //ProxyInfo.TryGetValue((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport), out var oldvalue);
                    proc_id = value2;
                }
            }

            CoreDataWorker.ProcDict.TryGetValue(proc_id, out var ProcName);
            ProcName = (ProcName == null) ? "unknown" : ProcName;
            LoggerWorker.Instance._logger.Verbose("{0}<-->{1}-->{2}:{3}<==>{4}:{5} => {6}",
                proc_id,
                ProcName,
                tcpinfo.dwlocaladdr,
                tcpinfo.dwlocalport,
                tcpinfo.dwremoteaddr,
                tcpinfo.dwremoteport,
                NetInfoWrapper.convert_state(tcpinfo.dwstate));

            result = CoreDataWorker.connection2pid.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var oldvalue);
            if (!result)//没有键值，进行插入
            {
                CoreDataWorker.connection2pid.TryAdd((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), proc_id);
            }
            else  // 存在键值，进行更新
            {
                CoreDataWorker.connection2pid.TryUpdate((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport),
                    proc_id, //new value
                    oldvalue);    //old value
            }
        }
        LoggerWorker.Instance._logger.Verbose("");
    }
}