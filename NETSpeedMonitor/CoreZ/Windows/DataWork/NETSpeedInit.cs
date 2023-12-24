using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.NetInfo;
using NETSpeedMonitor.CoreZ.Windows.PacpZ;
using NETSpeedMonitor.CoreZ.Windows.ProxyInfo;
using NETSpeedMonitor.CoreZ.Windows.ThreadZ;
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
    internal class NETSpeedInit
    {
        [SupportedOSPlatform("windows")]
        public static void NetSpeedMonitor_work()
        {
            LoggerWorker.Instance._logger.Information("work init!");
            _init_work(); //初始化数据
            ThreadHandler._all_Thread_start(); //开始多线程定时更新数据
            SharppcapHandler.Instance.CaputreHandler(); //开始捕获数据
            Console.ReadLine(); //输入任一键，结束程序
            _mutli_work_stop(); //关闭计时器
            _end_work();        //打印统计数据
        }

        static void _init_work()
        {
            //1. Process PID<-->PropName 
            //ProcessList.Instance.GetAllProcessName();
            CoreDataWorker.proc_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //2. get all MAC
            CoreDataWorker.mac_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //3. get all tcp
            LocalNetInfo.Instance.GetTcpAllList();
            CoreDataWorker.tcp_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //4. get all udp
            LocalNetInfo.Instance.GetUdpAllList();
            CoreDataWorker.udp_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //5. get Proxy Info
            NetworkEnvWarpper.NetEnvHandler_Init();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
        }


        static void _mutli_work_stop()
        {
            ThreadHandler.proc_timer.Stop();
            ThreadHandler.mac_timer.Stop();
            ThreadHandler.tcp_timer.Stop();
            ThreadHandler.udp_timer.Stop();
            ThreadHandler.pcap_timer.Stop();
            ThreadHandler.print_timer.Stop();
        }

        static void _end_work()
        {
            LoggerWorker.Instance._logger.Information("all_end");
            foreach (var data in CoreDataWorker.pid2Traffic)
            {
                CoreDataWorker.ProcDict.TryGetValue(data.Key, out var ProcName);
                ProcName = (ProcName == null) ? "unknown" : ProcName;
                LoggerWorker.Instance._logger.Information($"进程{ProcName}-->的上行总流量为{data.Value.Item1}bytes，下行总流量为{data.Value.Item2}bytes");
            }
            LoggerWorker.Instance.logger_end();
            //Console.ReadLine();
        }
    }
}
