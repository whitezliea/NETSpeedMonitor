using NETSpeedMonitor.CoreZ.Windows.IPInfo;
using NETSpeedMonitor.CoreZ.Windows.NetInfo;
using NETSpeedMonitor.CoreZ.Linux.NetInfo;
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
        public static void NetSpeedMonitor_work()
        {
            LoggerWorker.Instance._logger.Information("work init!");
            if (OperatingSystem.IsWindows())
            {
                LoggerWorker.Instance._logger.Information("Running on Windows.");
                _init_work_Windows(); //初始化数据
            }
            else if (OperatingSystem.IsLinux())
            {
                LoggerWorker.Instance._logger.Information("Running on Windows.");
                _init_work_Linux();
            }
            else
            {
                LoggerWorker.Instance._logger.Debug("not support os!");
                return;
            }
            
            
            ThreadHandler._all_Thread_start(); //开始多线程定时更新数据
            SharppcapHandler.Instance.CaputreHandler(); //开始捕获数据
            Console.ReadLine(); //输入任一键，结束程序
            _mutli_work_stop(); //关闭计时器
            _end_work();        //打印统计数据
        }

        [SupportedOSPlatform("windows")]
        static void _init_work_Windows()
        {
            //1. Process PID<-->PropName 
            //ProcessList.Instance.GetAllProcessName();
            NetInfoWorkerW.proc_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //2. get all MAC
            NetInfoWorkerW.mac_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //3. get all tcp
            LocalNetInfo.Instance.GetTcpAllList();
            NetInfoWorkerW.tcp_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //4. get all udp
            LocalNetInfo.Instance.GetUdpAllList();
            NetInfoWorkerW.udp_work();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
            //5. get Proxy Info
            NetworkEnvWarpper.NetEnvHandler_Init();
            LoggerWorker.Instance._logger.Verbose("-------------------------------------------------");
        }

        [SupportedOSPlatform("Linux")]
        static void _init_work_Linux()
        {
            NetInfoWrokerL.proc_work();
            NetInfoWrokerL.mac_work();
            NetInfoWrokerL.inode_work();
            NetInfoWrokerL.tcp_work();
            NetInfoWrokerL.udp_work();
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
