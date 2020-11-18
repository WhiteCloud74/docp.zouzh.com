using FrameworkCore.SocketAdapter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewaySimulator
{
    class GatewaySimulatorStart : BackgroundService
    {
        public readonly ILogger<GatewaySimulatorStart> _logger;
        public GatewaySimulatorStart(ILogger<GatewaySimulatorStart> logger)
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
                foreach (var item in Configuration.GetSection("Listener").GetChildren())
                {
                    Dictionary<string, string> adapterParameters = new Dictionary<string, string>();
                    foreach (var adapterParameter in item.GetSection("AdapterParameters").GetChildren())
                    {
                        adapterParameters.Add(adapterParameter.Key, adapterParameter.Value);
                    }

                    TcpSocketAdapterListener listener = new TcpSocketAdapterListener(
                        _logger,
                        item["ListenerId"],
                        item["LocalEndPoint"],
                        item["AdapterType"],
                        adapterParameters,
                        int.Parse(item["CheckSeconds"]),
                        int.Parse(item["PulseMilliseconds"]),
                        int.Parse(item["AdapterMaxCount"]),
                        int.Parse(item["SinglePackageMaxSize"]));

                    Task.Factory.StartNew(() => { listener.StartListen(); });

                    _logger.LogInformation($"{listener.ListenerId} Worker running at: {listener.LocalEndPoint}");
                }

                foreach (var item in Configuration.GetSection("Connector").GetChildren())
                {
                    Dictionary<string, string> adapterParameters = new Dictionary<string, string>();
                    foreach (var adapterParameter in item.GetSection("AdapterParameters").GetChildren())
                    {
                        adapterParameters.Add(adapterParameter.Key, adapterParameter.Value);
                    }

                    TcpSocketAdapterConnector connector = new TcpSocketAdapterConnector(
                        _logger,
                        item["ConnectId"],
                        item["AdapterType"],
                        adapterParameters,
                        int.Parse(item["CheckSeconds"]),
                        int.Parse(item["PulseMilliseconds"]),
                        int.Parse(item["AdapterMaxCount"]),
                        int.Parse(item["SinglePackageMaxSize"]));

                    List<KeyValuePair<string, string>> remoteEndPoints = new List<KeyValuePair<string, string>>();
                    foreach (var remoteEndPoint in item.GetSection("RemoteEndPoint").GetChildren())
                    {
                        remoteEndPoints.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                    }

                    connector.AddAdapters(remoteEndPoints);

                    Task.Run(() => { connector.Start(); });
                }
            });
        }
    }

}
