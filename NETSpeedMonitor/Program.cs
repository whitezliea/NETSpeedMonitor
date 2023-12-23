using NETSpeedMonitor.CoreZ.Windows.DataWork;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NETSpeedMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Running on Windows.");
                NETSpeedInit.NetSpeedMonitor_work();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Running on Linux.");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine("Running on macOS.");
            }
            else
            {
                Console.WriteLine("Unknown operating system.");
            }
        }
    }
}
