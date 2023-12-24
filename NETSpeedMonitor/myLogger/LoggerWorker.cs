using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.myLogger
{
    internal class LoggerWorker
    {
        private static LoggerWorker _instance = null!;
        private static readonly object _lock = new object();
        public readonly ILogger _logger;
        private LoggerWorker()
        {
            //https://stackoverflow.com/questions/2104099/c-sharp-if-then-directives-for-debug-vs-release
#if DEBUG    //Debug模式
            _logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
           .WriteTo.File("netspeedmonitor_debug.log",/*rollingInterval: RollingInterval.Day,*/ fileSizeLimitBytes: 10485760)
           .CreateLogger();
            //_logger = Log.Logger;
#else       //Realse模式
            _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.File("netspeedmonitor_realese.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760)
            .CreateLogger();
#endif
        }


        public void logger_end()
        {
            // 关闭Logger
            _logger.Information("CLOSE LOGGER!");
            Log.CloseAndFlush();
        }

        /// <summary>
        /// 懒汉式单例模式
        /// </summary>
        public static LoggerWorker Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LoggerWorker();
                    }
                    return _instance;
                }
            }
        }
    }
}
