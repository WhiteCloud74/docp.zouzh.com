using FrameworkCore.Instrument;
using FrameworkCore.Redis;
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
    public class TcpSocketGatewayListener
    {
        readonly ILogger _logger;
        readonly int _checkSeconds;                                               //检查定时器触发时间
        readonly Timer _pulseTimer;                                               //状态检查定时器

        readonly int _gatewayInitSeconds;
        readonly ConcurrentDictionary<string, TcpSocketGateway> _gateways;        //连接上来的网关
        readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;              //事件池

        readonly IPEndPoint _localEndPoint;                                       //监听地址和端口
        readonly int _heartSeconds;
        readonly int _deviceTimeoutMilliseconds;
        readonly int _trySendTimes;
        readonly Type _gatewayType;
        readonly string _gatewayServerId;
        bool _isListening;                                                        //是否在监听
        Socket _listenSocket;
        const int backlog = 100;
        const int pulseMilliseconds = 500;
        DateTime _lastPulseTime = DateTime.Now;

        public TcpSocketGatewayListener(ILogger logger, string ipAddress, int listenPort, string gatewayType, string gatewayServerId,
            int heartSeconds = 10, int gatewayInitSeconds = 3, int checkSeconds = 10,
            int gatewayMaxCount = 20000, int singlePackageMaxSize = 128, int trySendTimes = 3, int deviceTimeoutMilliseconds = 3000)
        {
            _logger = logger;
            if (ipAddress == "." || ipAddress.ToLower() == "localhost")
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                _localEndPoint = new IPEndPoint(endPoint.Address, listenPort);
            }
            else
            {
                _localEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), listenPort);
            }

            _gatewayServerId = gatewayServerId;
            _heartSeconds = heartSeconds;
            _checkSeconds = checkSeconds;
            _deviceTimeoutMilliseconds = deviceTimeoutMilliseconds;
            _trySendTimes = trySendTimes;
            _gatewayInitSeconds = gatewayInitSeconds;
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(gatewayMaxCount, singlePackageMaxSize, OnIoCompleted);
            _gatewayType = CommonFunction.GetTypeByBaseTypeAndTypeName(typeof(TcpSocketGateway), gatewayType);
            _gateways = new ConcurrentDictionary<string, TcpSocketGateway>();
            _pulseTimer = new Timer(OnPulseTimer, null, Timeout.Infinite, Timeout.Infinite);
            ThreadPool.SetMinThreads(backlog, backlog);

            void OnPulseTimer(object state)
            {
                _pulseTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if ((DateTime.Now - _lastPulseTime) >= TimeSpan.FromSeconds(_checkSeconds))
                {
                    _lastPulseTime = DateTime.Now;

                    if (!_isListening) { Start(); } //检查监听情况，若已经停止，则再次监听

                    //将已经停止的网关清除
                    List<KeyValuePair<string, TcpSocketGateway>> disconnectedGatewayAdapters = _gateways.Where(
                        t => (DateTime.Now - t.Value.LastRecvTime) > TimeSpan.FromSeconds(t.Value.HeartSeconds)
                        || (DateTime.Now - t.Value.LastSendTime) > TimeSpan.FromSeconds(t.Value.HeartSeconds))
                        .ToList();
                    Parallel.ForEach(disconnectedGatewayAdapters, a => ProcessGatewayError(a.Value, null));
                    //foreach (var gateway in disconnectedGatewayAdapters)
                    //{
                    //    ProcessGatewayError(gateway.Value, null);
                    //}

                }

                //给所有的网关发脉冲
                Parallel.ForEach(_gateways.Values, async v => await v.PulseAsync());
                //foreach (var item in _gatewayAdapters.Values)
                //{
                //    item.OnPulse();
                //}

                _pulseTimer.Change(TimeSpan.FromMilliseconds(pulseMilliseconds), TimeSpan.FromMilliseconds(pulseMilliseconds));
            }

            void OnIoCompleted(object sender, SocketAsyncEventArgs e)
            {
                if (e.SocketError != SocketError.Success)
                {
                    ProcessGatewayError(e.UserToken as TcpSocketGateway, e);
                    return;
                }

                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        if (e.BytesTransferred == 0)
                        {
                            ProcessGatewayError(e.UserToken as TcpSocketGateway, e);
                        }
                        else
                        {
                            ((TcpSocketGateway)e.UserToken).OnReceived(e);
                        }
                        break;
                    case SocketAsyncOperation.Send:
                        ((TcpSocketGateway)e.UserToken).OnSended(e);
                        break;
                    default:
                        throw new ArgumentException("The last operation completed on the socket was not an accept or receive or send");
                }
            }
        }

        public void Start()
        {
            _listenSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_localEndPoint);
            _listenSocket.Listen(backlog);

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += ProcessAccept;
            StartAccept(acceptEventArg);

            _logger.LogInformation("listen on {address}", _listenSocket.LocalEndPoint.ToString());
            _pulseTimer.Change(TimeSpan.FromSeconds(_checkSeconds), TimeSpan.FromMilliseconds(pulseMilliseconds));
            _isListening = true;
        }

        void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            try
            {
                acceptEventArg.AcceptSocket = null;
                if (!_listenSocket.AcceptAsync(acceptEventArg))
                {
                    ProcessAccept(null, acceptEventArg);
                }
            }
            catch (SocketException)
            {
                ProcessListenerError();
            }
        }

        void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                ProcessListenerError();
                return;
            }

            _logger.LogInformation($"accept {e.AcceptSocket.RemoteEndPoint}");

            ProcessGatewayConnect(e);

            StartAccept(e);

            void ProcessGatewayConnect(SocketAsyncEventArgs e)
            {
                TcpSocketGateway gateway = Activator.CreateInstance(_gatewayType) as TcpSocketGateway;
                gateway.Logger = _logger;
                gateway.HeartSeconds = _heartSeconds;
                gateway.GatewayInitSeconds = _gatewayInitSeconds;
                gateway.TrySendTimes = _trySendTimes;
                gateway.DeviceTimeoutMilliseconds = _deviceTimeoutMilliseconds;
                gateway.Socket = e.AcceptSocket;
                gateway.SocketAsyncEventArgsPool = _socketAsyncEventArgsPool;
                gateway.GatewayServerId = _gatewayServerId;

                if (gateway.GatewayInitAsync().Result)
                {
                    //添加或更新网关
                    _gateways.AddOrUpdate(gateway.GatewayId, gateway, (g, a) => a);
                }
            }
        }

        private void ProcessListenerError()
        {
            try
            {
                _listenSocket.Shutdown(SocketShutdown.Both);
                _listenSocket.Close();
            }
            catch { }
            finally
            {
                _listenSocket = null;
                _isListening = false;
            }
        }

        void ProcessGatewayError(TcpSocketGateway gatewayAdapter, SocketAsyncEventArgs e)
        {
            _gateways.TryRemove(gatewayAdapter.GatewayId, out _);

            //网关处理Socket错误
            gatewayAdapter.CloseSocket(e, false);
        }
    }
}
