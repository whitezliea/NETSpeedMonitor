using NETSpeedMonitor.CoreZ.Windows.DataWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.Command.Windows.PrintInfo
{
    internal class PrintInfoWrapper
    {
        public static void print_pid2traffic()
        {
            Console.Clear();
            //
            List<(int, string, uint, uint, ulong)> printList = new();
            foreach (var item in CoreDataWorker.pid2Traffic)               //ConcurrentDictionary 的 foreach 在遍历时是线程安全的。
            {
                CoreDataWorker.ProcDict.TryGetValue(item.Key, out var ProcName);
                ProcName = (ProcName == null) ? "unknown" : ProcName;
                uint upvalue = 0;
                uint downvalue = 0;
                ulong sumvalue = 0;
                var result = CoreDataWorker.pid2Traffic_old.TryGetValue(item.Key, out var value);
                if (result) //之前存在
                {
                    upvalue = (uint)((item.Value.Item1 > value.Item1) ? item.Value.Item1 - value.Item1 : 0); //上行流量
                    downvalue = (uint)((item.Value.Item2 > value.Item2) ? item.Value.Item2 - value.Item2 : 02); //下行流量
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


            Console.WriteLine($"{"进程id",-10}\t\t{"进程名",-25}\t\t{"UpSpeed",-14}\t\t{"DownSpeed",-14}\t\t");
            foreach (var item in printList)
            {
                Console.WriteLine($"{item.Item1,-10}\t\t" +
                                  $"{item.Item2,-25}\t\t" +
                                  $"{((item.Item3 / 1024.00).ToString("F2") + "KB/s"),-14}\t\t" +
                                  $"{((item.Item4 / 1024.00).ToString("F2") + "KB/s"),-14}\t\t"
                                  );
            }
        }
    }
}
