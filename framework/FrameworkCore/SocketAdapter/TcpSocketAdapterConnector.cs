using FrameworkCore.Instrument;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkCore.SocketAdapter
{
    public class TcpSocketAdapterConnector
    {
        readonly Type _adapterType;
        readonly Dictionary<string, string> _adapterParameters;
        public string ConnectorId { get; private set; }
        public ILogger Logger { get; private set; }

        readonly Timer _pulseTimer;                                               //脉冲定时器，0.5秒触发一次
        readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;              //事件池
        readonly List<TcpSocketAdapter> _adapters;                                //这个连接器管理的网关
        readonly int _checkSeconds;                                               //检查网关状态的时间间隔
        readonly int _pulseMilliseconds;                                          //脉冲时间间隔
        DateTime _lastCheckTime = DateTime.Now;

        public TcpSocketAdapterConnector(ILogger logger, string connectorId, string adapterType, Dictionary<string, string> adapterParameters,
            int checkSeconds = 5, int pulseMilliseconds = 500, int adapterMaxCount = 20000, int singlePackageMaxSize = 128)
        {
            Logger = logger;
            ConnectorId = connectorId;
            _adapterType = CommonFunction.GetTypeByBaseTypeAndTypeName(typeof(TcpSocketAdapter), adapterType);
            _adapterParameters = adapterParameters;
            _checkSeconds = checkSeconds;
            _pulseMilliseconds = pulseMilliseconds;

            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool();
            _socketAsyncEventArgsPool.Init(adapterMaxCount, singlePackageMaxSize, TcpSocketAdapter.OnIoCompleted);
            _pulseTimer = new Timer(OnPulseTimer, null, Timeout.Infinite, Timeout.Infinite);
            _adapters = new List<TcpSocketAdapter>();
        }

        void OnPulseTimer(object state)
        {
            _pulseTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if ((DateTime.Now - _lastCheckTime) >= TimeSpan.FromSeconds(_checkSeconds))
            {
                _lastCheckTime = DateTime.Now;
                foreach (var item in _adapters)
                {
                    if (item.Socket == null)
                    {
                        item.Connect();
                    }
                    else if (item.IsAbnormal())
                    {
                        item.CoreError("状态检查异常", null, null);
                    }
                }
            }

            //给所有的网关发脉冲
            Parallel.ForEach(_adapters, v => v.Pulse());

            _pulseTimer.Change(TimeSpan.FromMilliseconds(_pulseMilliseconds), TimeSpan.FromMilliseconds(_pulseMilliseconds));
        }

        public void AddAdapter(KeyValuePair<string, string> parameter)
        {
            TcpSocketAdapter adapter = Activator.CreateInstance(_adapterType) as TcpSocketAdapter;
            adapter.Logger = Logger;
            adapter.SocketAsyncEventArgsPool = _socketAsyncEventArgsPool;
            adapter.MacAddress = parameter.Key;
            adapter.RemoteEndPoint = parameter.Value;
            adapter.OnConnected += e => adapter.Init();
            foreach (var item in _adapterParameters)
            {
                adapter.AdapterParameters.Add(item.Key, item.Value);
            }

            _adapters.Add(adapter);
        }

        public void AddAdapters(IEnumerable<KeyValuePair<string, string>> adapters)
        {
            foreach (var adapter in adapters)
            {
                AddAdapter(adapter);
            }
        }

        public void Start()
        {
            foreach (var item in _adapters)
            {
                item.Connect();
            }
            _pulseTimer.Change(TimeSpan.FromSeconds(_checkSeconds), TimeSpan.FromMilliseconds(_pulseMilliseconds));
        }
    }
}
