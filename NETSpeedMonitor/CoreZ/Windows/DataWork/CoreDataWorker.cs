using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.NetInfo;
using NETSpeedMonitor.CoreZ.Windows.PacpZ;
using NETSpeedMonitor.CoreZ.Windows.ProcInfo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.DataWork
{
    internal class CoreDataWorker
    {
        //pid <--> pName
        public static ConcurrentDictionary<int, string> ProcDict = new();
        // ((localaddr,localport))<-->pid ==> 适配多网卡情况
        public static ConcurrentDictionary<(string, int), int> connection2pid = new(); // ==> 考虑线程安全问题 ConcurrentDictionary<TKey,TValue>

        // 创建 Dictionary，键是 packet_pid，值是包含两个元素的列表 第一个值表示下行包总和，第二个值表示上行包总和
        public static ConcurrentDictionary<int, (ulong, ulong)> pid2Traffic = new();  // ==> 考虑线程安全问题 ConcurrentDictionary<TKey,TValue>
        public static Dictionary<int, (ulong, ulong)> pid2Traffic_old = new();  // ==> 考虑线程安全问题 ConcurrentDictionary<TKey,TValue>

        //(localport) <----> List(localaddr,pid) // ==> 只看端口，不看ip
        public static ConcurrentDictionary<int, HashSet<(string, int)>> Udp2pid = new();

        public static HashSet<string> MACList = new();

        public static ConcurrentQueue<PcapinfoZ> packetsQueue = new ConcurrentQueue<PcapinfoZ>();

        // （proxyaddress,proxyport) <----> pid
        public static ConcurrentDictionary<(string, int), int> ProxyInfo = new();

        //public static HashSet<IPAddress> IPList = new();

        public static void proc_work(bool isPrint = true)
        {
            if (!ProcInfoWarpper.GetProcessInfoList(ref ProcDict, isPrint))
            {
                Console.WriteLine("error");
                return;
            }
            Console.WriteLine();
        }
        public static void mac_work(bool isPrint = true)
        {
            LocalNetInfo.Instance.GetAllMacList(ref MACList, isPrint);
            Console.WriteLine();
        }
        public static void udp_work(bool isPrint = true)
        {
            List<UDP_INFO_PID> udpinfolist = new();
            if (!NetInfoWrapper.GetUdpRowList(ref udpinfolist))
            {
                Console.WriteLine("get udp list error");
            }

            foreach (var udpinfo in udpinfolist)
            {
                if (isPrint)
                {
                    ProcDict.TryGetValue(udpinfo.dwpid, out var ProcName);
                    ProcName = (ProcName == null) ? "unknown" : ProcName;
                    Console.WriteLine("{0}<-->{1}-->{2}:{3}",
                    udpinfo.dwpid,
                    ProcName,
                    udpinfo.dwlocaladdr,
                    udpinfo.dwlocalport);
                }

                var result = Udp2pid.TryGetValue(udpinfo.dwlocalport, out var oldvalue);
                if (!result) //没有找到变量
                {
                    HashSet<(string, int)> udptmp = new();
                    udptmp.Add((udpinfo.dwlocaladdr, udpinfo.dwpid));
                    Udp2pid.TryAdd(udpinfo.dwlocalport, udptmp);
                }
                else
                {
                    // 防止内存泄漏
                    if (oldvalue?.Count > 1024)
                        oldvalue.Clear();
                    //if(!Udp2pid[udpinfo.dwlocalport].Contains((udpinfo.dwlocaladdr, udpinfo.dwpid)))
                    oldvalue?.Add((udpinfo.dwlocaladdr, udpinfo.dwpid));
                }

            }
            Console.WriteLine();
        }
        public static void tcp_work(bool isPrint = true)
        {


            List<TCP_INFO_PID> tcpinfolist = new();
            if (!NetInfoWrapper.GetTcpRowList(ref tcpinfolist))
            {
                Console.WriteLine("get tcp list error");
                return;
            }

            foreach (var tcpinfo in tcpinfolist)
            {
                int proc_id = tcpinfo.dwpid;

                var result = ProxyInfo.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var value1);
                if (result) //local
                {
                    if (tcpinfo.dwpid != 0) //检测到了代理软件进程pid
                    {

                        ProxyInfo.TryUpdate((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport),
                             tcpinfo.dwpid, value1);
                    }
                    else                    //使用之前的pid
                    {
                        // ProxyInfo.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var oldvalue);
                        proc_id = value1;
                    }
                }


                result = ProxyInfo.TryGetValue((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport), out var value2);
                if (result) //remote
                {
                    if (tcpinfo.dwpid != 0) //检测到了代理软件进程pid
                    {
                        ProxyInfo.TryUpdate((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport),
                             tcpinfo.dwpid, value2);
                    }
                    else                    //使用之前的pid
                    {
                        //ProxyInfo.TryGetValue((tcpinfo.dwremoteaddr, tcpinfo.dwremoteport), out var oldvalue);
                        proc_id = value2;
                    }
                }


                if (isPrint)
                {
                    ProcDict.TryGetValue(proc_id, out var ProcName);
                    ProcName = (ProcName == null) ? "unknown" : ProcName;
                    Console.WriteLine("{0}<-->{1}-->{2}:{3}<==>{4}:{5} => {6}",
                        proc_id,
                        ProcName,
                        tcpinfo.dwlocaladdr,
                        tcpinfo.dwlocalport,
                        tcpinfo.dwremoteaddr,
                        tcpinfo.dwremoteport,
                        NetInfoWrapper.convert_state(tcpinfo.dwstate));
                }

                result = connection2pid.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var oldvalue);
                if (!result)//没有键值，进行插入
                {
                    connection2pid.TryAdd((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), proc_id);
                }
                else  // 存在键值，进行更新
                {
                    connection2pid.TryUpdate((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport),
                        proc_id, //new value
                        oldvalue);    //old value
                }

                //if (!connection2pid.ContainsKey((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport)))        
                //{
                //    connection2pid.TryAdd((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport),
                //                        proc_id);
                //}
                //else
                //{
                //    connection2pid.TryGetValue((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), out var oldvalue);
                //    connection2pid.TryUpdate((tcpinfo.dwlocaladdr, tcpinfo.dwlocalport), 
                //                            proc_id, //new value
                //                            oldvalue);    //old value
                //    //connection2pid[(tcpinfo.dwlocaladdr, tcpinfo.dwlocalport)] = tcpinfo.dwpid;
                //}
                //else
                //{
                //    Console.WriteLine("TCP Existing Port multiplexing!");
                //}
            }
            Console.WriteLine();

        }
    }
}
