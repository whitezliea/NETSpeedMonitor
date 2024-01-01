using System.Net;
using System.Runtime.Versioning;
using NETSpeedMonitor.CoreZ;
using NETSpeedMonitor.myLogger;
using NETSpeedMonitor.CoreZ.Windows.DataWork;

namespace NetSpeed_Linux.NetInfo;

[SupportedOSPlatform("Linux")]
internal class UdpfileInfo
{

    public static void udpInfoRead()
    {
        string udpinfoFile = Linux_CommonDefine.LinuxSystemInfo.udpinfo;

        //使用StreamReader打开文件
        using (StreamReader reader = new StreamReader(udpinfoFile))
        {
            if (reader != null)
            {
                // 读取并忽略第一行
                reader.ReadLine();
                //逐行读取文件内容
                string line;
                while ((line = reader.ReadLine()!) != null)
                {
                    //Console.WriteLine(line);
                    ParseFileInfo(line, out var i);
                }
            }
        }
    }

    static void ParseFileInfo(in string info, out string[] infoList)
    {
        infoList = info.Split(' ')
                        .Where(part => !string.IsNullOrWhiteSpace(part))
                        .ToArray();
        //Console.WriteLine(infoList.Length);
        // sl  local_address rem_address   st tx_queue rx_queue tr tm->when    retrnsmt   uid  timeout inode ref pointer drops     
        // 0     1                 2       3        4                 5           6         7        8    9      
        if (infoList.Length < 12)
            return;
        // for (int i = 0; i < infoList.Length; i++)
        // {
        //     Console.WriteLine($"序号{i}<--->{infoList[i]}");
        // }
        ConvertToIPv4Info(infoList[1], out var localipv4);
        ConvertToIPv4Info(infoList[2], out var remoteipv4);
        int inode_num = Convert.ToInt32(infoList[9]);
        LoggerWorker.Instance._logger.Information($"udp连接:本地地址为{localipv4}<---->远程地址为{remoteipv4}<---->连接状态为{infoList[3]}<---->inode编号{inode_num}");

         if (CoreDataWorker.inode2pid.TryGetValue(inode_num, out var pid))
         {
             if (CoreDataWorker.connection2pid.TryGetValue(localipv4, out var oldPid))
             {
                 CoreDataWorker.connection2pid.TryUpdate(localipv4, pid, oldPid);
             }
             else
             {
                 CoreDataWorker.connection2pid.TryAdd(localipv4, pid);
             }
         }
         else
         {
             LoggerWorker.Instance._logger.Debug($"{inode_num} 无法找到pid");
         }
    }


    static void ConvertToIPv4Info(in string hexinfo, out (string, int) ipv4info)
    {
        var _ipv4 = hexinfo.Split(':');
        // 将十六进制转换为十进制
        long decimalIpAddress = Convert.ToInt64(_ipv4[0], 16);

        // 将十进制IPv4地址转换为IPAddress对象
        IPAddress ipAddress = new IPAddress(decimalIpAddress);

        // 输出标准的IPv4格式
        //Console.WriteLine($"Hex: {hexinfo} => IPv4: {ipAddress}");
        int port = Convert.ToInt32(_ipv4[1], 16);
        ipv4info = (ipAddress.ToString(), port);
    }
}