using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.SocketAdapter
{
    public delegate void ProcessSocketEvent(SocketAsyncEventArgs e);
    public delegate void ProcessException(Exception ex, SocketAsyncEventArgs e);

    /// <summary>
    /// 负责发送接收数据
    /// </summary>
    public abstract class TcpSocketAdapter
    {
        internal ILogger Logger;

        public event ProcessSocketEvent OnSendedData;
        public event ProcessException OnProcessException;

        public Guid GatewayId { get; set; }
        public string MacAddress;

        public Socket Socket { get; set; }
        public SocketAsyncEventArgsPool SocketAsyncEventArgsPool { get; set; }
        public DateTime LastSendTime { get; private set; } = DateTime.Now;
        public DateTime LastRecvTime { get; private set; } = DateTime.Now;

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
                ProcessError(e);
                return;
            }

            try
            {
                Logger.LogInformation($"{MacAddress} send data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
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
                ProcessError(e);
                return;
            }

            try
            {
                LastRecvTime = DateTime.Now;
                Logger.LogInformation($"{MacAddress} recv data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
                ProcessSocketDataAsync(e);
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
                // ex is SocketException，Adapter抓到异常
                ProcessError(e);
            }

            // ex 是其它异常，由用户去处理
            OnProcessException?.Invoke(ex, e);
        }

        internal void ProcessError(SocketAsyncEventArgs e)
        {
            if (e != null) SocketAsyncEventArgsPool.Push(e);
            Logger.LogInformation($"{MacAddress} socket error");
            if (Socket != null)
            {
                lock (this)
                {
                    if (Socket != null)
                    {
                        try
                        {
                            Socket.Shutdown(SocketShutdown.Both);
                            Socket.Close();
                        }
                        catch { }
                        Socket = null;

                    }
                }
            }
        }

        /// <summary>
        /// 网关连接后，初始化网关。初始化过程中可以与设备通讯
        /// 初始化必须给GatewayID赋值，网关守护程序依靠GatewayID订阅应用服务发给网关的指令
        /// </summary>
        /// <returns>初始化是否成功</returns>
        public abstract Task<bool> InitAsync();

        /// <summary>
        /// 处理收到的设备数据，处理过程可以给设备发数据
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract Task ProcessSocketDataAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// 处理应用服务发来的指令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public abstract void ProcessRedisCommand(RedisCommand command);
    }
}