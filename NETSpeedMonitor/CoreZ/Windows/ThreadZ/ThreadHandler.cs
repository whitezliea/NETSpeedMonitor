using NETSpeedMonitor.Command.Windows.PrintInfo;
using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.CoreZ.Windows.PacpZ;
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
            Console.WriteLine("Thread start");
            ProcessInfo_Update();
            MACInfo_Update();
            TCPinfo_Update();
            UDPinfo_Update();
            PcapinfoZ_update();
            TrafficInfo_print();
            Console.WriteLine("Thread end");
        }

        private static void ProcessInfo_Update()
        {
            //var timer = new Timer
            //{
            //    AutoReset = true,
            //    Interval = 1000,    //1s更新
            //    Enabled = true
            //};
            proc_timer.Interval = 1000;
            proc_timer.Enabled = true;
            proc_timer.AutoReset = true;
            proc_timer.Elapsed += Timer_Elapsed;
            proc_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.proc_work(false);
                //Console.WriteLine("processID<--->processName 表更新 表大小："+CoreDataWorker.ProcDict.Count);
            }
        }

        private static void MACInfo_Update()
        {
            //var timer = new Timer
            //{
            //    AutoReset = true,
            //    Interval = 1000 * 60 * 60 * 2, //2 小时更新
            //    Enabled = true
            //};
            mac_timer.Interval = 1000 * 10;//60 * 60 * 2;
            mac_timer.Enabled = true;
            mac_timer.AutoReset = true;
            mac_timer.Elapsed += Timer_Elapsed;
            mac_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.mac_work(false);
                //Console.WriteLine("MACList 表更新 表大小：" + CoreDataWorker.MACList.Count);
                //foreach (var data in CoreDataWorker.pid2Traffic)
                //{
                //    CoreDataWorker.ProcDict.TryGetValue(data.Key, out var ProcName);
                //    ProcName = (ProcName == null) ? "unknown" : ProcName;
                //    Console.WriteLine($"进程{ProcName}-->的上行总流量为{data.Value.Item1}bytes，下行总流量为{data.Value.Item2}bytes");
                //}
            }
        }

        private static void TCPinfo_Update()
        {
            //var timer3 = new Timer
            //{
            //    AutoReset = true,
            //    Interval = 2000,    //2s更新
            //    Enabled = true
            //};
            tcp_timer.Interval = 200;
            tcp_timer.Enabled = true;
            tcp_timer.AutoReset = true;
            tcp_timer.Elapsed += Timer_Elapsed;
            tcp_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.tcp_work(false);
                //Console.WriteLine("connection2pid 表更新 表大小：" + CoreDataWorker.connection2pid.Count);
                if (CoreDataWorker.connection2pid.Count > 10240)
                {
                    CoreDataWorker.connection2pid.Clear();
                }
            }
        }

        private static void UDPinfo_Update()
        {
            //var timer = new Timer
            //{
            //    AutoReset = true,
            //    Interval = 2000,    //2s更新
            //    Enabled = true
            //};
            udp_timer.Interval = 200;
            udp_timer.Enabled = true;
            udp_timer.AutoReset = true;
            udp_timer.Elapsed += Timer_Elapsed;
            udp_timer.Start();
            void Timer_Elapsed(object? sender, ElapsedEventArgs e)
            {
                CoreDataWorker.udp_work(false);
                //Console.WriteLine("Udp2pid 表更新 表大小：" + CoreDataWorker.Udp2pid.Count);
                //Console.Clear();

                //防止内存泄漏
                if (CoreDataWorker.Udp2pid.Count > 2048)
                {
                    CoreDataWorker.Udp2pid.Clear();
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
                //Console.WriteLine("packetsQueue的表大小" + CoreDataWorker.packetsQueue.Count);
                while (!CoreDataWorker.packetsQueue.IsEmpty)
                {
                    CoreDataWorker.packetsQueue.TryDequeue(out var packet);
                    PcapinfoZWarpper.PcapinfoZ_Unpack(in packet);
                }
                //Console.Clear();
                
                //防止内存泄漏
                if (CoreDataWorker.packetsQueue.Count > 500000)
                {
                    CoreDataWorker.packetsQueue.Clear();
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
