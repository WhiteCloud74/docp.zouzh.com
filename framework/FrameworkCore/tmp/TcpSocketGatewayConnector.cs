using FrameworkCore.Instrument;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkCore.SocketAdapter
{
    public class TcpSocketGatewayConnector
    {
        readonly ILogger _logger;
        readonly Timer _pulseTimer;                                               //脉冲定时器，0.5秒触发一次
        readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;              //事件池
        readonly List<TcpSocketGateway> _gateways;                                //这个连接器管理的网关
        readonly int _checkSeconds;                                               //检查网关状态的时间间隔

        readonly int _gatewayInitSeconds;

        readonly int _heartSeconds;
        readonly int _deviceTimeoutMilliseconds;
        readonly int _trySendTimes;
        readonly Type _gatewayType;
        readonly string _gatewayServerId;
        const int backlog = 100;
        const int pulseMilliseconds = 500;
        DateTime _lastPulseTime = DateTime.Now;

        public TcpSocketGatewayConnector(ILogger logger, string gatewayType, string gatewayServerId,
            int heartSeconds = 10, int gatewayInitSeconds = 3, int checkSeconds = 10,
            int gatewayMaxCount = 20000, int singlePackageMaxSize = 128, int trySendTimes = 3, int deviceTimeoutMilliseconds = 3000)
        {
            _logger = logger;
            _gatewayServerId = gatewayServerId;
            _heartSeconds = heartSeconds;
            _checkSeconds = checkSeconds;
            _deviceTimeoutMilliseconds = deviceTimeoutMilliseconds;
            _trySendTimes = trySendTimes;
            _gatewayInitSeconds = gatewayInitSeconds;
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(gatewayMaxCount, singlePackageMaxSize, TcpSocketAdapter.OnIoCompleted);
            _gatewayType = CommonFunction.GetTypeByBaseTypeAndTypeName(typeof(TcpSocketGateway), gatewayType);
            _gateways = new List<TcpSocketGateway>();
            _pulseTimer = new Timer(OnPulseTimer, null, Timeout.Infinite, Timeout.Infinite);
            ThreadPool.SetMinThreads(backlog, backlog);

        }

        void OnPulseTimer(object state)
        {
            _pulseTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if ((DateTime.Now - _lastPulseTime) >= TimeSpan.FromSeconds(_checkSeconds))
            {
                _lastPulseTime = DateTime.Now;

                List<TcpSocketGateway> disconnectedGatewayAdapters = _gateways.Where(
                t => t.Socket == null
                || (DateTime.Now - t.LastRecvTime) > TimeSpan.FromSeconds(t.HeartSeconds)
                || (DateTime.Now - t.LastSendTime) > TimeSpan.FromSeconds(t.HeartSeconds))
                .ToList();

                foreach (var gateway in disconnectedGatewayAdapters)
                {
                    ProcessGatewayError(gateway, null);
                }
            }

            //给所有的网关发脉冲
            Parallel.ForEach(_gateways, async v => await v.PulseAsync());

            _pulseTimer.Change(TimeSpan.FromMilliseconds(pulseMilliseconds), TimeSpan.FromMilliseconds(pulseMilliseconds));
        }
        public void Start()
        {
            _logger.LogInformation("connector start work");
            foreach (var gatewayAdapter in _gateways)
            {
                gatewayAdapter.Connect();
            }
            _pulseTimer.Change(TimeSpan.FromSeconds(_checkSeconds), TimeSpan.FromSeconds(_checkSeconds));
        }

        public void AddRemoteEndPoints(List<KeyValuePair<string, string>> remoteEndPoints)
        {
            foreach (var remoteEndPoint in remoteEndPoints)
            {
                TcpSocketGateway gateway = Activator.CreateInstance(_gatewayType) as TcpSocketGateway;
                gateway.Logger = _logger;
                gateway.HeartSeconds = _heartSeconds;
                gateway.GatewayInitSeconds = _gatewayInitSeconds;
                gateway.TrySendTimes = _trySendTimes;
                gateway.DeviceTimeoutMilliseconds = _deviceTimeoutMilliseconds;
                gateway.SocketAsyncEventArgsPool = _socketAsyncEventArgsPool;
                gateway.GatewayServerId = _gatewayServerId;
                gateway.MacAddress = remoteEndPoint.Key;
                gateway.RemoteEndPoint = new IPEndPoint(
                    IPAddress.Parse(remoteEndPoint.Value.Split(':')[0]),
                    int.Parse(remoteEndPoint.Value.Split(':')[1]));

                _gateways.Add(gateway);
            }
        }

        void ProcessGatewayError(TcpSocketGateway gatewayAdapter, SocketAsyncEventArgs e)
        {
            //网关处理Socket错误
            gatewayAdapter.CloseSocket(e, false);
        }
    }
}