using FrameworkCore.SocketAdapter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace usr_device
{
    class Simulator : BackgroundService
    {
        private readonly ILogger<Simulator> _logger;
        public Simulator(ILogger<Simulator> logger)
        {
            _logger = logger;
        }

        SocketAsyncEventArgsPool _socketAsyncEventArgsPool;
        public List<Gateway> _netgates = new List<Gateway>();
        Timer _statusCheckTimer;
        readonly int _errorSeconds = 5;
        int _heartSeconds;
        bool _isRunning;
        int _gatewayDelayMilliseconds;
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build()
                .GetSection("Gateway");

            _heartSeconds = int.Parse(config["heartSeconds"]);
            string hostIp = config["serverIpAddress"].Split(':')[0];
            string hostPort = config["serverIpAddress"].Split(':')[1];
            IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Parse(hostIp), int.Parse(hostPort));
            int singleBufferSize = int.Parse(config["singleBufferSize"]);
            int gatewayCount = int.Parse(config["gatewayCount"]);
            _gatewayDelayMilliseconds = int.Parse(config["gatewayDelayMilliseconds"]);

            _netgates = CreateGateway(gatewayCount);
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(gatewayCount, singleBufferSize, OnIoComplete, hostEndPoint);
            foreach (var item in _netgates)
            {
                item.m_socketAsyncEventArgsPool = _socketAsyncEventArgsPool;
            }
            _statusCheckTimer = new Timer(OnStatusCheckTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        private List<Gateway> CreateGateway(int gatewayCount)
        {
            Random random = new Random();
            List<Gateway> ret = new List<Gateway>(gatewayCount);
            for (int i = 1; i <= gatewayCount; i++)
            {
                Gateway gateway = new Gateway()
                {
                    Logger = _logger,
                    GatewayId = $"CED4{i:D4}0000",
                    MacAddress = $"CED4{i:D4}0000",
                    GatewayDelayMilliseconds = _gatewayDelayMilliseconds,
                    EndDevices = new EndDevice[random.Next(4, 8)]
                };

                for (int j = 1; j <= gateway.EndDevices.Length; j++)
                {
                    gateway.EndDevices[j] = new EndDevice()
                    {
                        Logger = _logger,
                        DeviceID = $"CED4{i:D4}{j:D4}",
                        MacAddress = $"CED4{i:D4}{j:D4}",
                        Energy = 0,
                    };
                }

                ret.Add(gateway);
            }
            return ret;
        }

        public void Start()
        {
            _isRunning = true;
            _logger.LogInformation("Start");
            Task.Factory.StartNew(() =>
            {
                foreach (var item in _netgates)
                {
                    item.Connect();
                }
                _statusCheckTimer.Change(10000, 5000);
            });
        }

        internal void Stop()
        {
            _isRunning = false;
            _logger.LogInformation("Stop");
            Task.Factory.StartNew(() =>
            {
                _statusCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                foreach (var item in _netgates)
                {
                    item.Disconnect();
                }
            });
        }

        private void OnStatusCheckTimer(object state)
        {
            if (!_isRunning) return;

            foreach (var item in _netgates)
            {
                item.CheckStatus(_errorSeconds, _heartSeconds);
            }
        }

        void OnIoComplete(object sender, SocketAsyncEventArgs e)
        {
            Gateway netgate = e.UserToken as Gateway;
            if (e.SocketError == SocketError.Success)
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        netgate.OnConnected(e);
                        break;
                    case SocketAsyncOperation.Disconnect:
                        netgate.OnDisconnected(e);
                        break;
                    case SocketAsyncOperation.Send:
                        netgate.LastSendTime = DateTime.Now;
                        netgate.OnSended(e);
                        break;
                    case SocketAsyncOperation.Receive:
                        netgate.LastRecvTime = DateTime.Now;
                        netgate.OnReceived(e);
                        break;
                    default:
                        throw new ArgumentException("未处理事件");
                }
            }
            else
            {
                netgate.OnSocketError(e, false);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                Init();
                Start();
            });
        }
    }
}
