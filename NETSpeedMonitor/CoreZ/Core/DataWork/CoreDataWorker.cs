using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.NetInfo;
using NETSpeedMonitor.CoreZ.Windows.PacpZ;
using NETSpeedMonitor.CoreZ.Windows.ProcInfo;
using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.DataWork
{
    internal class CoreDataWorker
    {

        [SupportedOSPlatform("Linux")]
        public static ConcurrentDictionary<int, int> inode2pid = new(); //
        
        
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
    }
}
