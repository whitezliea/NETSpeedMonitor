using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.PacpZ
{
    internal class PcapinfoZWarpper
    {

        public static void pid2traffic_update(int pid, int packetLength, bool isUp = true)
        {
            if (isUp == true)
            {
                //pid2traffic计算上行流量
                var result = CoreDataWorker.pid2Traffic.TryGetValue(pid, out var oldvalue);
                if (!result)
                {
                    CoreDataWorker.pid2Traffic.TryAdd(pid, (0, 0));
                }
                else
                {
                    if (((ulong)packetLength + oldvalue.Item1) >= ulong.MaxValue)    // 如果超过统计最大值，则重置
                    {
                        CoreDataWorker.pid2Traffic.TryUpdate(pid,
                        (0, oldvalue.Item2),
                        oldvalue);
                    }
                    else
                    {
                        CoreDataWorker.pid2Traffic.TryUpdate(pid,
                            (oldvalue.Item1 + (ulong)packetLength, oldvalue.Item2),
                            oldvalue);
                    }
                }
            }
            else
            {
                //pid2traffic计算下行流量
                var result = CoreDataWorker.pid2Traffic.TryGetValue(pid, out var oldvalue);
                if (!result)
                {
                    CoreDataWorker.pid2Traffic.TryAdd(pid, (0, 0));
                }
                else
                {
                    if (oldvalue.Item2 + (ulong)packetLength > ulong.MaxValue)
                    {
                        CoreDataWorker.pid2Traffic.TryUpdate(pid,
                            (oldvalue.Item1, 0),
                             oldvalue);
                    }
                    else
                    {
                        CoreDataWorker.pid2Traffic.TryUpdate(pid,
                            (oldvalue.Item1, oldvalue.Item2 + (ulong)packetLength),
                            oldvalue);
                    }
                }
            }
        }

        public static void PcapinfoZ_Unpack(in PcapinfoZ packet)
        {
            var isTCP = packet.isTCP;
            var packetLength = packet.packetLength; //bytes
            var soucreMAC = packet.SourceMAC;
            var sourceIP = packet.SourceIP;
            var sourcePort = packet.SourcePort;
            var destionIP = packet.DestinationIP;
            var destPort = packet.DestinationPort;

            if (CoreDataWorker.MACList.Contains(soucreMAC)) //上行流量
            {
                if (isTCP) // 是TCP包
                {
                    var result = CoreDataWorker.connection2pid.TryGetValue(
                         ((sourceIP, sourcePort))        //如果是流量上行包，那么(Localaddr,localport)是源地址
                         , out var pid);
                    if (result == true  //&& !CoreDataWorker.IPList.Contains(destionIP) //目前来看，内部通信的情况很少，除非是内部测试，即便是代理流量也不会走环回回路
                        )
                    {
                        CoreDataWorker.ProcDict.TryGetValue(pid, out var ProcName);
                        ProcName = (ProcName == null) ? "unknown" : ProcName;
                        LoggerWorker.Instance._logger.Information($"找到进程{pid}?=>{result},进程名{ProcName},连接状态为{sourceIP}==>{destionIP},发送TCP上行流量包大小{packetLength}");
                        pid2traffic_update(pid, packetLength, true);
                    }
                    //else if (CoreDataWorker.IPList.Contains(destionIP))
                    //{
                    //    Console.WriteLine($"找到进程{pid}?=>{result},内部流量,发送TCP上行流量包大小{packetLength}");
                    //}
                    else
                    {
                        LoggerWorker.Instance._logger.Information($"没有找到进程{pid}?=>{result},连接状态为{sourceIP}==>{destionIP},发送TCP上行流量包大小{packetLength}");
                    }
                }
                else     //是UDP
                {
                    var result = CoreDataWorker.Udp2pid.TryGetValue((int)sourcePort, out var udplist);
                    int udppid = 0;
                    if (result == true)
                    {
                        foreach (var udpi in udplist!)
                        {
                            if (udpi.Item1 == sourceIP)
                            {
                                udppid = udpi.Item2;
                                break;
                            }
                            else if (udpi.Item1 == "0.0.0.0")
                            {
                                udppid = udpi.Item2;
                                break;
                            }
                        }
                        CoreDataWorker.ProcDict.TryGetValue(udppid, out var ProcName);
                        ProcName = (ProcName == null) ? "unknown" : ProcName;
                        LoggerWorker.Instance._logger.Information($"找到进程{udppid}?=>{result},进程名{ProcName},状态为{sourceIP}==>{destionIP},发送UDP上行流量包大小{packetLength}");
                        pid2traffic_update(udppid, packetLength, true);
                    }
                    else
                    {
                        LoggerWorker.Instance._logger.Information($"没有找到进程{udppid}?=>{result},状态为{sourceIP}==>{destionIP},发送UDP上行流量包大小{packetLength}");
                    }
                }
            }
            else                                        // 下行流量
            {
                if (isTCP)  //TCP包
                {
                    var result = CoreDataWorker.connection2pid.TryGetValue(
                        ((destionIP, (int)destPort))      //如果是流量下行包，那么(localaddr,localport)是目的地址  
                        , out var pid);
                    if (result == true)
                    {
                        CoreDataWorker.ProcDict.TryGetValue(pid, out var ProcName);
                        ProcName = (ProcName == null) ? "unknown" : ProcName;
                        LoggerWorker.Instance._logger.Information($"找到进程{pid}?=>{result},进程名{ProcName},连接状态为{sourceIP}==>{destionIP},发送TCP下行流量包大小{packetLength}");
                        pid2traffic_update(pid, packetLength, false);
                    }
                    else
                    {
                        LoggerWorker.Instance._logger.Information($"没有找到进程{pid}?=>{result},连接状态为{sourceIP}==>{destionIP},发送TCP下行流量包大小{packetLength}"); ;
                    }
                }
                else     //是UDP
                {
                    var result = CoreDataWorker.Udp2pid.TryGetValue((int)destPort, out var udplist);
                    int udppid = 0;
                    if (result == true)
                    {
                        foreach (var udpi in udplist!)
                        {
                            if (udpi.Item1 == destionIP)
                            {
                                udppid = udpi.Item2;
                                break;
                            }
                            else if (udpi.Item1 == "0.0.0.0")
                            {
                                udppid = udpi.Item2;
                                break;
                            }
                        }
                        CoreDataWorker.ProcDict.TryGetValue(udppid, out var ProcName);
                        ProcName = (ProcName == null) ? "unknown" : ProcName;
                        LoggerWorker.Instance._logger.Information($"找到进程{udppid}?=>{result},进程名{ProcName},状态为{sourceIP}==>{destionIP},发送UDP下行流量包大小{packetLength}");
                        pid2traffic_update(udppid, packetLength, false);
                    }
                    else
                    {
                        LoggerWorker.Instance._logger.Information($"没有找到进程{udppid}?=>{result},状态为{sourceIP}==>{destionIP},发送UDP下行流量包大小{packetLength}");
                    }
                }
            }         
        }
    }

    public struct PcapinfoZ
    {
        public string SourceMAC;
        public string DestinationMAC;
        public string SourceIP;
        public string DestinationIP;
        public int SourcePort;
        public int DestinationPort;
        public bool isTCP;
        public int packetLength;

        public PcapinfoZ(string sm, string dm, string si, string di, int sp, int dp, bool isT, int packetL)
        {
            SourceMAC = sm;
            DestinationMAC = dm;
            SourceIP = si;
            DestinationIP = di;
            SourcePort = sp;
            DestinationPort = dp;
            isTCP = isT;
            packetLength = packetL;
        }
    }
}
