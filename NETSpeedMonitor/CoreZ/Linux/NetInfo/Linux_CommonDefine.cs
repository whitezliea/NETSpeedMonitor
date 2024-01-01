namespace NETSpeedMonitor.CoreZ;
using NETSpeedMonitor.myLogger;

class Linux_CommonDefine
{
    public class LinuxSystemInfo
    {
        public static string pidinfo = "/proc";
        public static string tcpinfo = "/proc/net/tcp";
        public static string udpinfo = "/proc/net/udp";

        LinuxSystemInfo()
        {
            LoggerWorker.Instance._logger.Debug("读取进程信息路径" + pidinfo);
            LoggerWorker.Instance._logger.Debug("读取tcp信息路径" + tcpinfo);
            LoggerWorker.Instance._logger.Debug("读取udp信息路径" + udpinfo);
        }
    }


}