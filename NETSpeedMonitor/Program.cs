using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.myLogger;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NETSpeedMonitor
{
    internal class Program
    {
        // publish：dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false --configuration Release
        static void Main(string[] args)
        {
            LoggerWorker.Instance._logger.Information("Hello, World!");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                LoggerWorker.Instance._logger.Information("Running on Windows.");
                NETSpeedInit.NetSpeedMonitor_work();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LoggerWorker.Instance._logger.Information("Running on Linux.");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                LoggerWorker.Instance._logger.Information("Running on macOS.");
            }
            else
            {
                LoggerWorker.Instance._logger.Information("Unknown operating system.");
            }
        }
    }
}
