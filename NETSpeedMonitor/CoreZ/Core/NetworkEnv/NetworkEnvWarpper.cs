using NETSpeedMonitor.CoreZ.Windows.DataWork;
using NETSpeedMonitor.myLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.ProxyInfo
{
    internal class NetworkEnvWarpper
    {
        public static void NetEnvHandler_Init()
        {
            GetProxyInfo();
            List<(string, int)> proxyinfo_init = new List<(string, int)>  //用户添加 or 主流代理路径
            {
                ("127.0.0.1",10809),
                ("127.0.0,1",7890)
            };

            foreach (var proxyinfo in proxyinfo_init)
            {
                if (!CoreDataWorker.ProxyInfo.ContainsKey(proxyinfo))
                {
                    CoreDataWorker.ProxyInfo.TryAdd(proxyinfo, 0);
                }
            }

        }

        public static void GetProxyInfo()
        {
            try
            {
                IWebProxy defaultProxy = WebRequest.DefaultWebProxy!;
                var ProxyIPAddr = defaultProxy?.GetProxy(new Uri("https://www.google.com"));
                if (ProxyIPAddr != null)
                {
                    // 获取代理地址
                    LoggerWorker.Instance._logger.Debug("defaultProxy Address: " + ProxyIPAddr);
                    var match = Regex.Match(ProxyIPAddr.ToString(), @"http://([\d\.]+):(\d+)/");

                    if (match.Success)
                    {
                        string ipAddress = match.Groups[1].Value;
                        int port = Convert.ToInt32(match.Groups[2].Value);

                        if (!CoreDataWorker.ProxyInfo.ContainsKey((ipAddress, port)))
                        {
                            CoreDataWorker.ProxyInfo.TryAdd((ipAddress, port), 0);
                        }
                        LoggerWorker.Instance._logger.Debug($"IP Address: {ipAddress}");
                        LoggerWorker.Instance._logger.Debug($"Port: {port}");
                    }
                    else
                    {
                        LoggerWorker.Instance._logger.Warning("Can't get proxyinfo!");
                    }
                }


            }
            catch (Exception ex)
            {
                LoggerWorker.Instance._logger.Error("An error occurred: " + ex.Message);
            }
        }
    }
}
