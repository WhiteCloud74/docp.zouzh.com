using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.SocketAdapter
{
    class TcpSocketAdapterStart : BackgroundService
    {
        public readonly ILogger<TcpSocketAdapterStart> _logger;
        public TcpSocketAdapterStart(ILogger<TcpSocketAdapterStart> logger)
        {
            _logger = logger;
            _logger.BeginScope($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  ", null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var Configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .Build();

            return Task.Factory.StartNew(() =>
            {
                foreach (var item in Configuration.GetSection("TcpServer").GetChildren())
                {
                    TcpSocketAdapterServerManager tcpSocketAdapterServerManager = new TcpSocketAdapterServerManager(
                        item["IpAddress"],
                        int.Parse(item["ListenPort"]),
                        int.Parse(item["HeartSeconds"]),
                        int.Parse(item["GatewayMaxCount"]),
                        int.Parse(item["SinglePackageMaxSize"]),
                        item["GatewayAdapterType"])
                    {
                        Logger = _logger
                    };
                    _logger.LogInformation("{gateway} Worker running at: {ipaddress}:{port}", item["GatewayAdapterType"], item["IpAddress"], item["ListenPort"]);

                    Task.Factory.StartNew(() => { tcpSocketAdapterServerManager.Start(); });
                }
            });
        }
    }
}