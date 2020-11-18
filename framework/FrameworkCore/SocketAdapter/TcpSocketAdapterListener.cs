using FrameworkCore.Instrument;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkCore.SocketAdapter
{
    public class TcpSocketAdapterListener
    {
        readonly Type _adapterType;
        readonly Dictionary<string, string> _adapterParameters;
        public string ListenerId { get; private set; }
        public ILogger Logger { get; private set; }
        public string LocalEndPoint { get; private set; }

        readonly Timer _pulseTimer;                                               //脉冲定时器，0.5秒触发一次
        readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;              //事件池
        readonly int _checkSeconds;                                               //检查网关状态的时间间隔
        readonly int _pulseMilliseconds;                                          //脉冲时间间隔
        DateTime _lastCheckTime = DateTime.Now;

        readonly ConcurrentDictionary<string, TcpSocketAdapter> _adapters;        //连接上来的网关
        TcpSocketAdapter _listener;

        public TcpSocketAdapterListener(ILogger logger, string listenerId, string localEndPoint, string adapterType, Dictionary<string, string> adapterParameters,
            int checkSeconds = 5, int pulseMilliseconds = 500, int adapterMaxCount = 20000, int singlePackageMaxSize = 128)
        {
            Logger = logger;
            ListenerId = listenerId;
            LocalEndPoint = localEndPoint;
            _adapterType = CommonFunction.GetTypeByBaseTypeAndTypeName(typeof(TcpSocketAdapter), adapterType);
            _adapterParameters = adapterParameters;
            _checkSeconds = checkSeconds;
            _pulseMilliseconds = pulseMilliseconds;

            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(adapterMaxCount, singlePackageMaxSize, TcpSocketAdapter.OnIoCompleted);
            _pulseTimer = new Timer(OnPulseTimer, null, Timeout.Infinite, Timeout.Infinite);
            _adapters = new ConcurrentDictionary<string, TcpSocketAdapter>();
        }

        void OnPulseTimer(object state)
        {
            _pulseTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if ((DateTime.Now - _lastCheckTime) >= TimeSpan.FromSeconds(_checkSeconds))
            {
                _lastCheckTime = DateTime.Now;

                if (_listener == null) { StartListen(); }

                //将已经停止的网关清除
                List<KeyValuePair<string, TcpSocketAdapter>> disconnectedAdapters = _adapters.Where(
                    t => (DateTime.Now - t.Value.LastRecvTime) > TimeSpan.FromSeconds(int.Parse(t.Value.AdapterParameters["HeartSeconds"]))
                    || (DateTime.Now - t.Value.LastSendTime) > TimeSpan.FromSeconds(int.Parse(t.Value.AdapterParameters["HeartSeconds"])))
                    .ToList();
                Parallel.ForEach(disconnectedAdapters, a =>
                {
                    a.Value.CoreError("status invalid", null, null);
                    _adapters.TryRemove(a.Key, out _);
                });
            }

            //给所有的网关发脉冲
            Parallel.ForEach(_adapters, v => v.Value.Pulse());

            _pulseTimer.Change(TimeSpan.FromMilliseconds(_pulseMilliseconds), TimeSpan.FromMilliseconds(_pulseMilliseconds));
        }

        public void StartListen()
        {
            _listener = new TcpSocketAdapter
            {
                Logger = Logger,
                SocketAsyncEventArgsPool = _socketAsyncEventArgsPool,
                AdapterParameters = new Dictionary<string, string>()
            };
            _listener.LocalEndPoint = LocalEndPoint;
            _listener.AdapterId = ListenerId;
            _listener.OnAccepted += Listener_OnAccepted;
            _listener.OnAbnormalClosed += (e) => { _listener = null; };

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += TcpSocketAdapter.OnIoCompleted;
            e.UserToken = _listener;

            _listener.Listen(e);

            _pulseTimer.Change(TimeSpan.FromSeconds(_checkSeconds), TimeSpan.FromMilliseconds(_pulseMilliseconds));
        }

        private void Listener_OnAccepted(SocketAsyncEventArgs e)
        {
            TcpSocketAdapter adapter = Activator.CreateInstance(_adapterType) as TcpSocketAdapter;
            adapter.Logger = Logger;
            adapter.Socket = e.AcceptSocket;
            adapter.SocketAsyncEventArgsPool = _socketAsyncEventArgsPool;
            adapter.OnInitComplete += b => { if (b) _adapters.AddOrUpdate(adapter.AdapterId, adapter, (g, a) => a); };
            adapter.AdapterParameters = new Dictionary<string, string>();
            foreach (var item in _adapterParameters)
            {
                adapter.AdapterParameters.Add(item.Key, item.Value);
            }

            adapter.Init();
        }
    }
}
