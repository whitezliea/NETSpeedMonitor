using System.Runtime.Versioning;
using NETSpeedMonitor.CoreZ;
using NETSpeedMonitor.CoreZ.Windows.DataWork;

namespace NetSpeed_Linux.NetInfo;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using NETSpeedMonitor.myLogger;

[SupportedOSPlatform("Linux")]
class SystemInfoZ
{
    [DllImport("libc", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int readlink(string path, StringBuilder buf, int bufsiz);
    
    public static void Inode2pid_work()   //proc
    {
        string directoryPath = Linux_CommonDefine.LinuxSystemInfo.pidinfo;

        if (Directory.Exists(directoryPath))
        {
            List<string> pid_List = Directory.GetDirectories(directoryPath).ToList();
            foreach (string dir in pid_List)  //每个
            {
                //Console.WriteLine(dir);
                proc_fd_check(in dir);
            }
        }
        else
        {
            LoggerWorker.Instance._logger.Debug("目录不存在");
        }

        // string test = "/proc/970/fd/257";
        // proc_fd_readlink(in test);
    }

    private static void proc_fd_check(in string linkPath)    ////proc/pid
    {
        List<string> link_fd_path_list = Directory.GetDirectories(linkPath).ToList();

        foreach (var link_fd_path in link_fd_path_list)
        {
            if (link_fd_path.Equals(string.Concat(linkPath, "/fd")))
            {
                int pid_num = ExtractNumber(link_fd_path, false);
                if (pid_num == -1)
                    return;
                LoggerWorker.Instance._logger.Debug("  | fd:==>" + link_fd_path);
                LoggerWorker.Instance._logger.Debug("pid_num: " + pid_num);
                try
                {
                    proc_fd_number_check(in link_fd_path,in pid_num);
                }
                catch (Exception ex)
                {
                    LoggerWorker.Instance._logger.Error(ex.Message);
                }
            }
        }
    }
    private static void proc_fd_number_check(in string link_fd_path,in int pid_num) //proc/pid/fd/info_num
    {
        List<string> link_fd_number_list = Directory.GetFiles(link_fd_path).ToList();
        string result = "";
        foreach (var link_fd_number in link_fd_number_list)
        {
            result = proc_fd_readlink(in link_fd_number);
            if (result.Contains("socket"))
            {
                int inode_num = ExtractNumber(result, true);
                LoggerWorker.Instance._logger.Debug("|");
                LoggerWorker.Instance._logger.Debug("---找到 tcp的innode信息" + inode_num);
                if (CoreDataWorker.inode2pid.TryGetValue(inode_num, out var oldValue))
                {
                    CoreDataWorker.inode2pid.TryUpdate(inode_num, pid_num, oldValue);
                }
                else
                {
                    CoreDataWorker.inode2pid.TryAdd(inode_num, pid_num);
                }
                //break;
            }
        }
        LoggerWorker.Instance._logger.Debug("-----------------------");

    }
    private static string proc_fd_readlink(in string linkPath)//proc/pid/fd/info_num/info
    {
        string result = "";
        //设置缓冲区
        const int initialBufferSize = 128;
        int bufferSize = initialBufferSize;
        StringBuilder buffer;
        int bytesRead = -1;
        // do
        // {
        //     buffer = new StringBuilder(bufferSize);
        //     bytesRead = readlink(linkPath, buffer, bufferSize);

        //     // 如果 bytesRead 等于缓冲区大小减1，表示缓冲区可能太小，尝试扩大缓冲区
        //     if (bytesRead >= bufferSize - 1)
        //     {
        //         bufferSize *= 2;
        //         //Console.WriteLine("bytesRead: " + bytesRead + " bufferSize:" + bufferSize);
        //     }
        //     //Console.WriteLine("bytesRead: " + bytesRead + " bufferSize:" + bufferSize);
        // } while (bytesRead >= bufferSize - 1);
        try
        {
            buffer = new StringBuilder(bufferSize); // need trycatch
            bytesRead = readlink(linkPath, buffer, bufferSize);

            if (bytesRead != -1)
            {
                try
                {
                    // 读取成功，打印链接的目标路径
                    result = buffer.ToString(0, bytesRead);
                    return result;
                }
                catch (Exception ex)
                {
                    LoggerWorker.Instance._logger.Information(ex.Message);
                    LoggerWorker.Instance._logger.Information("buffer size " + buffer.Length + " bytesRead: " + bytesRead + " buffer =>:" + buffer);
                    // Console.WriteLine(ex);
                }
                LoggerWorker.Instance._logger.Debug($"Symbolic link {linkPath} points to: {result}");
            }
            else
            {
                // 读取失败，输出错误信息
                LoggerWorker.Instance._logger.Debug($"Failed to read symbolic link {linkPath}. Error code: {Marshal.GetLastWin32Error()}");
            }
        }
        catch (Exception ex)
        {
            LoggerWorker.Instance._logger.Error(ex.Message);
        }
        return result;
    }

    static int ExtractNumber(string input, bool isInode = false)
    {
        Regex regex;
        if (isInode == true)
        {
            regex = new Regex(@"socket:\[(\d+)\]");
        }
        else
        {
            regex = new Regex(@"/proc/(\d+)/fd");
        }
        Match match = regex.Match(input);

        // 检查是否匹配到
        if (match.Success)
        {
            // 提取匹配到的数字部分
            string numberString = match.Groups[1].Value;

            // 尝试将字符串转换为整数
            if (int.TryParse(numberString, out int result))
            {
                return result;
            }
            else
            {
                LoggerWorker.Instance._logger.Error("Failed to parse the number.");
            }
        }
        else
        {
            LoggerWorker.Instance._logger.Debug("String format does not match.");
        }
        return -1;
    }
}