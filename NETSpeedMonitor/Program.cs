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
            //LoggerWorker.Instance._logger.Information("Hello, World!");
            NETSpeedInit.NetSpeedMonitor_work();

        }
    }
}
