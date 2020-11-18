using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.SocketAdapter
{
    public delegate void ProcessSocketEvent(SocketAsyncEventArgs e);
    public delegate void OnInitComplete(bool success);

    public class TcpSocketAdapter
    {
        internal event ProcessSocketEvent OnAccepted;
        internal event ProcessSocketEvent OnConnected;
        internal event ProcessSocketEvent OnDisonnected;
        public event ProcessSocketEvent OnSendedData;
        public event ProcessSocketEvent OnReceivedData;
        public event ProcessSocketEvent OnAbnormalClosed;
        public event Action OnPulse;
        public event OnInitComplete OnInitComplete;

        internal protected ILogger Logger { get; set; }
        internal protected Dictionary<string, string> AdapterParameters { get; set; }
        internal Socket Socket { get; set; }
        internal SocketAsyncEventArgsPool SocketAsyncEventArgsPool { get; set; }

        internal protected DateTime LastSendTime { get; private set; } = DateTime.Now;
        internal protected DateTime LastRecvTime { get; private set; } = DateTime.Now;
        private readonly object _lock = new object();
        private const int backlog = 100;

        /// <summary>
        /// 是服务器端还是设备端
        /// 服务器端需订阅发布消息
        /// 设备端不需要
        /// </summary>
        public bool IsWorkOnServer { get; set; }
        public string AdapterId { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string LocalEndPoint { get; set; } = "";
        public string RemoteEndPoint { get; set; } = "";

        public TcpSocketAdapter()
        {
            if (IsWorkOnServer)
            {
                OnInitComplete += async b =>
                {
                    if (b)
                    {
                        //终止订阅发到该网关的指令,以防网关是重连或者以前连接在别的服务器
                        await RedisService.UnsubscribeAsync(AdapterId);

                        //订阅该网关的指令
                        await RedisService.GatewayServerSubscribeCommandAsync(AdapterId, ProcessRedisCommandAsync);
                    }
                };
                OnAbnormalClosed += async e =>
                {
                    await RedisService.UnsubscribeAsync(AdapterId);
                };
            }
        }

        public TcpSocketAdapter(ILogger logger, Dictionary<string, string> otherParameters) : this()
        {
            Logger = logger;
            AdapterParameters = otherParameters;
        }

        internal static void OnIoCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                ((TcpSocketAdapter)e.UserToken).CoreError(e.LastOperation.ToString(), null, e);
                return;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ((TcpSocketAdapter)e.UserToken).ProcessAccepted(e);
                    break;
                case SocketAsyncOperation.Connect:
                    ((TcpSocketAdapter)e.UserToken).ProcessConnected(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ((TcpSocketAdapter)e.UserToken).ProcessDisconnected(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ((TcpSocketAdapter)e.UserToken).ProcessReceived(e);
                    break;
                case SocketAsyncOperation.Send:
                    ((TcpSocketAdapter)e.UserToken).ProcessSended(e);
                    break;
                default:
                    throw new ArgumentException($"{e.LastOperation}: The last operation completed on the socket is not define");
            }
        }

        internal void Pulse()
        {
            OnPulse?.Invoke();
        }

        internal void CoreError(string socketAction, Exception ex, SocketAsyncEventArgs e)
        {
            if (e != null) SocketAsyncEventArgsPool.Push(e);

            if (Socket == null) return;

            lock (_lock)
            {
                if (Socket == null) return;

                Logger.LogError($"{MacAddress} {socketAction} error: {ex}");
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                }
                catch { }
                finally
                {
                    Socket = null;
                }
            }

            try
            {
                OnAbnormalClosed?.Invoke(null);
            }
            catch
            {
            }
        }

        void UserError(string userAction, Exception ex)
        {
            Logger.LogWarning($"{MacAddress} {userAction} error: {ex}");
        }

        internal void Listen(SocketAsyncEventArgs e)
        {
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(LocalEndPoint.Split(':')[0]),
                    int.Parse(LocalEndPoint.Split(':')[1]));
                Socket.Bind(endPoint);
                Socket.Listen(backlog);
            }
            catch (Exception ex)
            {
                CoreError("Listen", ex, null);
            }

            Logger.LogInformation($"{MacAddress} start listen on {LocalEndPoint}");
            //SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            //e.SetBuffer(e.Offset, 0);

            ContinualAccept(e);
        }

        void ContinualAccept(SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket = null;
                if (!Socket.AcceptAsync(e))
                {
                    ProcessAccepted(e);
                }
            }
            catch (Exception ex)
            {
                CoreError("AcceptAsync", ex, e);
            }
        }

        void ProcessAccepted(SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} accept {e.AcceptSocket.RemoteEndPoint}");

            try
            {
                OnAccepted?.Invoke(e);
            }
            catch (Exception ex)
            {
                UserError("OnAccepted", ex);
            }
            finally
            {
                ContinualAccept(e);
            }
        }

        internal void Connect()
        {
            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(LocalEndPoint.Split(':')[0]),
                int.Parse(LocalEndPoint.Split(':')[1]));
            e.RemoteEndPoint = endPoint;
            e.SetBuffer(e.Offset, 0);
            e.DisconnectReuseSocket = false;

            Logger.LogInformation($"{MacAddress} try to connect {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (!Socket.ConnectAsync(e)) ProcessConnected(e);
            }
            catch (Exception ex)
            {
                CoreError("ConnectAsync", ex, e);
            }
        }

        void ProcessConnected(SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} has connected {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");

            try
            {
                OnConnected?.Invoke(e);
            }
            catch (Exception ex)
            {
                UserError("OnConnected", ex);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
            }
        }

        internal void Disconnect()
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            try
            {
                if (!Socket.DisconnectAsync(e)) ProcessDisconnected(e);
            }
            catch (Exception ex)
            {
                CoreError("DisconnectAsync", ex, e);
            }
        }

        void ProcessDisconnected(SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} has disconnected {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");

            try
            {
                OnDisonnected?.Invoke(null);
            }
            catch (Exception ex)
            {
                UserError("OnDisonnected", ex);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
            }
        }

        protected void SendData(byte[] data)
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            Buffer.BlockCopy(data, 0, e.Buffer, e.Offset, data.Length);
            e.SetBuffer(e.Offset, data.Length);

            try
            {
                if (!Socket.SendAsync(e)) ProcessSended(e);
            }
            catch (Exception ex)
            {
                CoreError("SendAsync", ex, e);
            }
        }

        void ProcessSended(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                ((TcpSocketAdapter)e.UserToken).CoreError("send 0 bytes", null, e);
                return;
            }

            Logger.LogInformation($"{MacAddress} has send data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
            LastSendTime = DateTime.Now;
            try
            {
                OnSendedData?.Invoke(e);
            }
            catch (Exception ex)
            {
                UserError("OnSendedData", ex);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
            }
        }

        void ReceiveData()
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            try
            {
                if (!Socket.ReceiveAsync(e)) ProcessReceived(e);
            }
            catch (Exception ex)
            {
                CoreError("ReceiveAsync", ex, e);
            }
        }

        void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                ((TcpSocketAdapter)e.UserToken).CoreError("receive 0 bytes", null, e);
                return;
            }

            Logger.LogInformation($"{MacAddress} has received data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
            LastRecvTime = DateTime.Now;
            try
            {
                OnReceivedData?.Invoke(e);
            }
            catch (Exception ex)
            {
                UserError("OnReceivedData", ex);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
                ReceiveData();
            }
        }

        internal void Init()
        {
            Task.Run(async () =>
           {
               var timeout = Task.Delay(int.Parse(AdapterParameters["AdapterInitSeconds"]) * 1000);
               ReceiveData();
               var init = InitAdapter();

               var taskList = new List<Task> { timeout, init };
               if (await Task.WhenAny(taskList) == init)
               {
                   OnInitComplete?.Invoke(init.Result);
               }
               else
               {
                   CoreError("Init Timeout", null, null);
               }
           });
        }

        /// <summary>
        /// 进行初始化工作，期间可以与对方通讯
        /// 必须填写必要的参数GatewayId，MacAddress
        /// </summary>
        /// <returns></returns>
        internal protected virtual async Task<bool> InitAdapter()
        {
            return await Task.Run(() => { return false; });
        }

        /// <summary>
        /// 网关是否状态异常，需要断开
        /// </summary>
        /// <returns></returns>
        internal protected virtual bool IsAbnormal()
        {
            return (DateTime.Now - LastRecvTime) > TimeSpan.FromSeconds(3 * int.Parse(AdapterParameters["HeartSeconds"]))
                || (DateTime.Now - LastSendTime) > TimeSpan.FromSeconds(3 * int.Parse(AdapterParameters["HeartSeconds"]));
        }

        /// <summary>
        /// 服务器端需要重载，处理应用端发来的请求
        /// </summary>
        /// <param name="redisCommand"></param>
        /// <returns></returns>
        internal protected virtual async Task ProcessRedisCommandAsync(RedisCommand redisCommand)
        {
            await Task.Run(() => { });
        }
    }
}
