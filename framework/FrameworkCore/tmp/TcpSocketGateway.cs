using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkCore.SocketAdapter
{
    public delegate void ProcessException(Exception ex, SocketAsyncEventArgs e);

    /// <summary>
    /// 负责发送接收数据
    /// </summary>
    public abstract class TcpSocketGateway
    {
        public event ProcessSocketEvent OnConnect;
        public event ProcessSocketEvent OnDisonnect;
        /// <summary>
        /// 异常情况导致Socket关闭
        /// </summary>
        public event ProcessSocketEvent OnAbnormalClosed;
        public event ProcessSocketEvent OnSendedData;
        public event ProcessSocketEvent OnReceivedData;
        /// <summary>
        /// 用户异常处理
        /// </summary>
        public event ProcessException OnProcessException;

        public string GatewayServerId { get; set; }
        public string GatewayId { get; protected set; }
        public string MacAddress { get; set; }

        /// <summary>
        /// 工作在服务器端还是设备端
        /// 服务器端需要发布订阅Redis消息，设备端不需要
        /// </summary>
        public bool IsServer { get; set; } = true;
        internal protected ILogger Logger { get; set; }

        /// <summary>
        /// 网关适配器作为客户端，连接到设备，则需给出设备的EndPoint
        /// </summary>
        internal EndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 心跳时间，若服务器与网关在这时间内没有通讯，则断开连接
        /// </summary>
        internal int HeartSeconds { get; set; }
        /// <summary>
        /// 设备初始化的时间，若超过这个时间，初始化失败
        /// </summary>
        internal int GatewayInitSeconds;
        internal protected int DeviceTimeoutMilliseconds;
        internal protected int TrySendTimes;
        internal Socket Socket { get; set; }

        internal SocketAsyncEventArgsPool SocketAsyncEventArgsPool { get; set; }
        internal protected DateTime LastSendTime { get; private set; } = DateTime.Now;
        internal protected DateTime LastRecvTime { get; private set; } = DateTime.Now;

        internal void Connect()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            e.RemoteEndPoint = RemoteEndPoint;
            e.SetBuffer(e.Offset, 0);
            e.DisconnectReuseSocket = false;

            Logger.LogInformation($"{MacAddress} try to connect {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");
            try
            {
                if (!Socket.ConnectAsync(e)) OnConnected(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
        }

        internal void OnConnected(SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} has connected {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");

            SocketAsyncEventArgsPool.Push(e);

            GatewayInitAsync().Wait();

            OnConnect?.Invoke(e);
        }

        internal async Task<bool> GatewayInitAsync()
        {
            ReceiveData();

            var timeout = Task.Delay(TimeSpan.FromSeconds(GatewayInitSeconds));
            var init = InitAsync();
            var taskList = new List<Task> { timeout, init };
            if (await Task.WhenAny(taskList) == init && init.Result)
            {
                if (IsServer)
                {
                    //终止订阅发到该网关的指令,以防网关是重连或者以前连接在别的服务器
                    await RedisService.UnsubscribeAsync(GatewayId);

                    //订阅该网关的指令
                    await RedisService.GatewayServerSubscribeCommandAsync(GatewayId, ProcessRedisCommandAsync);
                }

                return true;
            }
            else
            {
                CloseSocket(null, false);
                return false;
            }
        }

        internal void Disconnect()
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            try
            {
                if (!Socket.DisconnectAsync(e)) OnDisconnected(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
        }

        internal void OnDisconnected(SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} has disconnected {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");

            CloseSocket(e, true);

            OnDisonnect?.Invoke(null);
        }

        protected void SendData(byte[] data)
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            Buffer.BlockCopy(data, 0, e.Buffer, e.Offset, data.Length);
            e.SetBuffer(e.Offset, data.Length);

            try
            {
                if (!Socket.SendAsync(e)) OnSended(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
        }

        internal void OnSended(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                CloseSocket(e, false);
                return;
            }

            try
            {
                Logger.LogInformation($"{MacAddress} has send data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
                LastSendTime = DateTime.Now;
                OnSendedData?.Invoke(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
            }
        }

        internal protected void ReceiveData()
        {
            if (Socket == null) return;

            SocketAsyncEventArgs e = SocketAsyncEventArgsPool.Pop(this);
            try
            {
                if (!Socket.ReceiveAsync(e)) OnReceived(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
        }

        internal void OnReceived(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                CloseSocket(e, false);
                return;
            }

            try
            {
                LastRecvTime = DateTime.Now;
                Logger.LogInformation($"{MacAddress} has received data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
                OnReceivedData(e);
            }
            catch (Exception ex)
            {
                OnException(ex, e);
            }
            finally
            {
                SocketAsyncEventArgsPool.Push(e);
            }

            //若异常处理中，用户抛异常，就不会再接收数据
            ReceiveData();
        }

        internal void OnException(Exception ex, SocketAsyncEventArgs e)
        {
            if (ex == null || ex is SocketException)
            {
                //ex == null，由manager调用，socket异步出错或初始化出错
                // ex is SocketException，Adapter本身抓到异常
                CloseSocket(e, false);
            }

            // ex 是其它异常，由用户去处理
            OnProcessException?.Invoke(ex, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isNormal">是否正常关闭</param>
        internal void CloseSocket(SocketAsyncEventArgs e, bool isNormal)
        {
            if (e != null) SocketAsyncEventArgsPool.Push(e);
            if (Socket != null)
            {
                lock (this)
                {
                    if (Socket != null)
                    {
                        try
                        {
                            if (IsServer)
                            {
                                RedisService.UnsubscribeAsync(GatewayId).Wait();
                            }

                            Socket.Shutdown(SocketShutdown.Both);
                            Socket.Close();
                            Logger.LogInformation($"{MacAddress} socket closed");
                        }
                        catch { }
                        Socket = null;
                    }
                }
            }

            if (!isNormal)
            {
                OnAbnormalClosed?.Invoke(null);
            }
        }
        internal async Task PulseAsync()
        {
            if (Socket != null)
            {
                await OnPulseAsync();
            }
        }
        /// <summary>
        /// 网关连接后，初始化网关。初始化过程中可以与设备通讯
        /// 初始化必须给GatewayID赋值，网关守护程序依靠GatewayID订阅应用服务发给网关的指令
        /// </summary>
        /// <returns>初始化是否成功</returns>
        protected abstract Task<bool> InitAsync();

        /// <summary>
        /// 处理收到的设备数据，处理过程可以给设备发数据
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected abstract Task ProcessReceivedDataAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// 处理应用服务发来的指令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected virtual Task ProcessRedisCommandAsync(RedisCommand command) { throw new NotImplementedException(); }

        /// <summary>
        /// 为防止设备通讯过程中出现假死现象，外部给个脉冲
        /// 利用这个脉冲，重新激活通讯流程
        /// </summary>
        protected abstract Task OnPulseAsync();
    }
}