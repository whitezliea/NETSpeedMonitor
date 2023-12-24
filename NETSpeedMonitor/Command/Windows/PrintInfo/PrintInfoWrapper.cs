using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.Command.Windows.PrintInfo
{
    internal class PrintInfoWrapper
    {
        public string GetSize(long zbytes)
        {
            string[] units = { "", "K", "M", "G", "T", "P" };

            foreach (string unit in units)
            {
                if (zbytes < 1024)
                {
                    return $"{zbytes:F2}{unit}b";
                }
                zbytes /= 1024;
            }

            return $"{zbytes:F2}Eb"; // If the size is extremely large
        }

        public static void print_pid2traffic()
        {
            //防止内存泄漏
            if (CoreDataWorker.pid2Traffic.Count > 1000)
            {
                CoreDataWorker.pid2Traffic.Clear();
                CoreDataWorker.pid2Traffic_old.Clear();
                LoggerWorker.Instance._logger.Warning("流量统计表已满，需要重置");
                return;
            }

            Console.Clear();
            //
            List<(int, string, uint, uint, ulong)> printList = new();
            foreach (var item in CoreDataWorker.pid2Traffic)               //ConcurrentDictionary 的 foreach 在遍历时是线程安全的。
            {
                if (item.Key == 0) //过滤未知流量
                    continue;

                CoreDataWorker.ProcDict.TryGetValue(item.Key, out var ProcName);
                ProcName = (ProcName == null) ? "unknown" : ProcName;
                uint upvalue = 0;
                uint downvalue = 0;
                ulong sumvalue = 0;
                var result = CoreDataWorker.pid2Traffic_old.TryGetValue(item.Key, out var value);
                if (result) //之前存在
                {
                    upvalue = (uint)((item.Value.Item1 > value.Item1) ? item.Value.Item1 - value.Item1 : 0); //上行流量
                    downvalue = (uint)((item.Value.Item2 > value.Item2) ? item.Value.Item2 - value.Item2 : 0); //下行流量
                    sumvalue = upvalue + downvalue;
                    //更新
                    CoreDataWorker.pid2Traffic_old[item.Key] = (item.Value.Item1, item.Value.Item2);
                }
                else        //不存在
                {
                    upvalue = (uint)item.Value.Item1;//上行流量
                    downvalue = (uint)item.Value.Item2; //下行流量
                    sumvalue = upvalue + downvalue;
                    //更新
                    CoreDataWorker.pid2Traffic_old.Add(item.Key, (item.Value.Item1, item.Value.Item2));
                }
                printList.Add((item.Key, ProcName, upvalue, downvalue, sumvalue));
            }

            printList.Sort((x, y) => y.Item5.CompareTo(x.Item5));

            int count = 0;
            Console.WriteLine($"{"进程id",-10}\t\t{"进程名",-25}\t\t{"UpSpeed",-14}\t\t{"DownSpeed",-14}\t\t");
            foreach (var item in printList)
            {
                if (count > 16)
                    break;
                count++;
                Console.WriteLine($"{item.Item1,-10}\t\t" +
                                  $"{item.Item2,-25}\t\t" +
                                  $"{((item.Item3 / 1024.00).ToString("F2") + "KB/s"),-14}\t\t" +
                                  $"{((item.Item4 / 1024.00).ToString("F2") + "KB/s"),-14}\t\t"
                                  );
            }
            Console.WriteLine("Press Enter to stop capturing...");
        }
    }
}
