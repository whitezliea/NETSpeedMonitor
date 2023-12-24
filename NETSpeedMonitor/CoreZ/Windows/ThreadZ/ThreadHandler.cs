using NETSpeedMonitor.Command.Windows.PrintInfo;
using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.CoreZ.Windows.PacpZ;
using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NETSpeedMonitor.CoreZ.Windows.ThreadZ
{
    internal class ThreadHandler
    {
        public static Timer proc_timer = new Timer();
        public static Timer mac_timer = new Timer();
        public static Timer tcp_timer = new Timer();
        public static Timer udp_timer = new Timer();
        public static Timer pcap_timer = new Timer();
        public static Timer print_timer = new Timer();

        public static void _all_Thread_start()
        {
            LoggerWorker.Instance._logger.Information("Thread start");
            ProcessInfo_Update();
            MACInfo_Update();
            TCPinfo_Update();
            UDPinfo_Update();
            PcapinfoZ_update();
            TrafficInfo_print();
            LoggerWorker.Instance._logger.Information("Thread end");
        }

        private static void ProcessInfo_Update()
        {
            proc_timer.Interval = 1000;
            proc_timer.Enabled = true;
            proc_timer.AutoReset = true;
            proc_timer.Elapsed += Timer_Elapsed;
            proc_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.proc_work();
                LoggerWorker.Instance._logger.Debug("processID<--->processName 表更新 表大小："+CoreDataWorker.ProcDict.Count);
            }
        }

        private static void MACInfo_Update()
        {
            mac_timer.Interval = 1000 * 60;//60 * 60 * 2;
            mac_timer.Enabled = true;
            mac_timer.AutoReset = true;
            mac_timer.Elapsed += Timer_Elapsed;
            mac_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.mac_work();
                LoggerWorker.Instance._logger.Debug("MACList 表更新 表大小：" + CoreDataWorker.MACList.Count);
            }
        }

        private static void TCPinfo_Update()
        {
            tcp_timer.Interval = 200;
            tcp_timer.Enabled = true;
            tcp_timer.AutoReset = true;
            tcp_timer.Elapsed += Timer_Elapsed;
            tcp_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.tcp_work(false);
                LoggerWorker.Instance._logger.Debug("connection2pid 表更新 表大小：" + CoreDataWorker.connection2pid.Count);
                if (CoreDataWorker.connection2pid.Count > 10240)
                {
                    CoreDataWorker.connection2pid.Clear();
                    LoggerWorker.Instance._logger.Warning("tcp连接表已满，强制清空防止占用过多内存");
                }
            }
        }

        private static void UDPinfo_Update()
        {
            udp_timer.Interval = 200;
            udp_timer.Enabled = true;
            udp_timer.AutoReset = true;
            udp_timer.Elapsed += Timer_Elapsed;
            udp_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.udp_work();
                LoggerWorker.Instance._logger.Debug("Udp2pid 表更新 表大小：" + CoreDataWorker.Udp2pid.Count);
                //防止内存泄漏
                if (CoreDataWorker.Udp2pid.Count > 2048)
                {
                    CoreDataWorker.Udp2pid.Clear();
                    LoggerWorker.Instance._logger.Warning("udp连接表已满，强制清空防止占用过多内存");
                }
            }
        }

        private static void PcapinfoZ_update()
        {
            pcap_timer.Interval = 900;
            pcap_timer.Enabled = true;
            pcap_timer.AutoReset = true;
            pcap_timer.Elapsed += Timer_Elapsed;
            pcap_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                LoggerWorker.Instance._logger.Debug("packetsQueue的表大小" + CoreDataWorker.packetsQueue.Count);
                while (!CoreDataWorker.packetsQueue.IsEmpty)
                {
                    CoreDataWorker.packetsQueue.TryDequeue(out var packet);
                    PcapinfoZWarpper.PcapinfoZ_Unpack(in packet);
                }   
                //防止内存泄漏
                if (CoreDataWorker.packetsQueue.Count > 500000)
                {
                    CoreDataWorker.packetsQueue.Clear();
                    LoggerWorker.Instance._logger.Warning("捕获数据包缓存队列已满，强制清空防止占用过多内存");
                }
            }
        }

        private static void TrafficInfo_print()
        {
            pcap_timer.Interval = 1000;
            pcap_timer.Enabled = true;
            pcap_timer.AutoReset = true;
            pcap_timer.Elapsed += Timer_Elapsed;
            pcap_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                PrintInfoWrapper.print_pid2traffic();
            }
        }
    }
}
