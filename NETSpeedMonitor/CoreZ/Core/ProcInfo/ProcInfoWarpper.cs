using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.ProcInfo
{
    internal class ProcInfoWarpper
    {
        public static bool GetProcessInfoList(ref ConcurrentDictionary<int, string> ProInfoList)
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    if (process.Id == 0) continue;
                    string procName;
                    if (process.ProcessName.Contains(' ') && OperatingSystem.IsLinux())
                    {
                        procName = process.ProcessName.Split(' ')[0];
                    }
                    else
                    {
                        procName = process.ProcessName;
                    }
                    LoggerWorker.Instance._logger.Verbose($"{process.Id}<---->{procName}");
                    var result = ProInfoList.TryGetValue(process.Id, out var oldvalue);
                    if (result)
                    {
                        //ProInfoList[process.Id] = process.ProcessName;
                        ProInfoList.TryUpdate(process.Id,
                            procName,
                        oldvalue!);
                    }
                    else
                    {
                        ProInfoList.TryAdd(process.Id, procName);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerWorker.Instance._logger.Error(e.Message.ToString());
                return false;
            }
            return true;
        }
    }
}
