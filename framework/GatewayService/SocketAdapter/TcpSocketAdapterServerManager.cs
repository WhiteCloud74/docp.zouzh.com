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

namespace GatewayService.SocketAdapter
{
    public class TcpSocketAdapterServerManager
    {
        internal ILogger Logger;

        const int backlog = 100;
        const int _checkSeconds = 10;                                                   //检查定时器触发时间
        readonly Timer _checkTimer;                                                     //状态检查定时器
        readonly int _heartSeconds;                                                     //心跳时间，若服务器与网关没有通讯超过该时间，则断开连接
        readonly IPEndPoint _localEndPoint;                                             //监听地址和端口

        readonly ConcurrentDictionary<Guid, TcpSocketAdapterServer> _gatewayAdapters;   //连接上来的网关
        readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;                    //事件池
        Type _gatewayAdapterType;
        string _gatewayrType;

        bool _isListening;                                                              //是否在监听
        Socket _listenSocket;
        void OnCheckTimer(object state)
        {
            if (!_isListening) { Start(); }

            List<KeyValuePair<Guid, TcpSocketAdapterServer>> disconnectedGatewayAdapters
                = _gatewayAdapters.Where(
                    t => ((DateTime.Now - t.Value.LastRecvTime) > TimeSpan.FromSeconds(_heartSeconds))
                    || ((DateTime.Now - t.Value.LastSendTime) > TimeSpan.FromSeconds(_heartSeconds)))
                .ToList();

            foreach (var gateway in disconnectedGatewayAdapters)
            {
                ProcessGatewayError(gateway.Value, null);
            }
        }

        public TcpSocketAdapterServerManager(string ipAddress, int listenPort, int heartSeconds,
            int gatewayMaxCount, int singlePackageMaxSize, string gatewayAdapterType)
        {
            _gatewayrType = gatewayAdapterType;

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

            _heartSeconds = heartSeconds;
            _checkTimer = new Timer(OnCheckTimer, null, Timeout.Infinite, Timeout.Infinite);
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(gatewayMaxCount, singlePackageMaxSize, OnIoCompleted);
            _gatewayAdapters = new ConcurrentDictionary<Guid, TcpSocketAdapterServer>();
        }

        public void Start()
        {
            _gatewayAdapterType = CommonFunction.GetTypeByBaseTypeAndTypeName(typeof(TcpSocketAdapter), _gatewayrType);
            _checkTimer.Change(TimeSpan.FromSeconds(_checkSeconds), TimeSpan.FromSeconds(_checkSeconds));

            _listenSocket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_localEndPoint);
            _listenSocket.Listen(backlog);
            ThreadPool.SetMinThreads(backlog, backlog);

            _isListening = true;
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += ProcessAccept;
            Logger.LogInformation("listen on {addres}", _listenSocket.LocalEndPoint.ToString());
            StartAccept(acceptEventArg);
        }

        void OnIoCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                ProcessGatewayError(e.UserToken as TcpSocketAdapterServer, e);
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (e.BytesTransferred == 0)
                    {
                        ProcessGatewayError(e.UserToken as TcpSocketAdapterServer, e);
                    }
                    else
                    {
                        ((TcpSocketAdapterServer)e.UserToken).OnReceived(e);
                    }
                    break;
                case SocketAsyncOperation.Send:
                    ((TcpSocketAdapterServer)e.UserToken).OnSended(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not an accept or receive or send");
            }
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
            catch (SocketException e)
            {
                ProcessListenerError(e, acceptEventArg);
            }
        }

        void ProcessAccept(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                ProcessListenerError(null, e);
                return;
            }

            Logger.LogInformation($"accept {e.AcceptSocket.RemoteEndPoint}");

            TcpSocketAdapterServer gatewayAdapter = Activator.CreateInstance(_gatewayAdapterType) as TcpSocketAdapterServer;
            gatewayAdapter.Socket = e.AcceptSocket;
            gatewayAdapter.SocketAsyncEventArgsPool = _socketAsyncEventArgsPool;
            gatewayAdapter.Logger = Logger;
            gatewayAdapter.ReceiveData();

            ProcessGatewayConnect(gatewayAdapter);

            StartAccept(e);
        }

        private void ProcessListenerError(SocketException ex, SocketAsyncEventArgs e)
        {
            try
            {
                _listenSocket.Shutdown(SocketShutdown.Both);
                _listenSocket.Close();
                _listenSocket = null;
            }
            catch { }
            finally
            {
                _isListening = false;
            }
        }

        void ProcessGatewayConnect(TcpSocketAdapterServer gatewayAdapter)
        {
            if (gatewayAdapter.InitAsync().Result)
            {
                //终止订阅发到该网关的指令,以防网关是重连或者以前连接在别的服务器
                RedisService.UnsubscribeAsync(gatewayAdapter.GatewayId).Wait();

                //添加或更新网关
                _gatewayAdapters.AddOrUpdate(gatewayAdapter.GatewayId, gatewayAdapter, (g, a) => a);

                //订阅该网关的指令
                RedisService.GatewayServerSubscribeCommandAsync(gatewayAdapter.GatewayId,
                   gatewayAdapter.ProcessRedisCommand).Wait();
            }
            else
            {
                gatewayAdapter.OnException(null, null);
            }
        }

        void ProcessGatewayError(TcpSocketAdapterServer gatewayAdapter, SocketAsyncEventArgs e)
        {
            gatewayAdapter.ProcessError(e);

            _gatewayAdapters.TryRemove(gatewayAdapter.GatewayId, out _);

            //终止订阅发到该网关的指令
            RedisService.UnsubscribeAsync(gatewayAdapter.GatewayId).Wait();

            //网关处理Socket错误
            gatewayAdapter.OnException(null, null);
        }
    }
}
