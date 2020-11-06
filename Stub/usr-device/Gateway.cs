using GatewayService.SocketAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using FrameworkCore.Instrument;

namespace usr_device
{
    public class EndDevice
    {
        public ILogger Logger { get; set; }
        public String DeviceID { get; set; }
        public String MacAddress { get; set; } //12字节，0~9 20 16 12 00 00 31
        public int Energy { get; set; }// 电量（单位0.01KWH），数据库没有，返回0
    }

    class Gateway
    {
        public ILogger Logger { get; set; }
        public string GatewayId { get; set; }
        public string MacAddress { get; set; }// 12字节 0~9A ~H
        public EndDevice[] EndDevices { get; set; }

        public Socket Socket { get; set; }
        public bool Running { get; private set; } = false;
        public bool Connected { get; private set; } = false;
        public bool Registered { get; private set; } = false;
        public DateTime LastSendTime { get; set; } = DateTime.Now;
        public DateTime LastRecvTime { get; set; } = DateTime.Now;
        public SocketAsyncEventArgsPool m_socketAsyncEventArgsPool;
        public int GatewayDelayMilliseconds;

        #region framework
        void SetStatus(bool running, bool connected, bool registered)
        {
            Running = running;
            Connected = connected;
            Registered = registered;
        }

        internal void Connect()
        {
            SetStatus(true, false, false);
            SocketAsyncEventArgs e = m_socketAsyncEventArgsPool.Pop(this);
            e.SetBuffer(e.Offset, 0);
            Socket = new Socket(e.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
            Logger.LogInformation($"{MacAddress} connect {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");
            m_socketAsyncEventArgsPool.Push(e);

            SetStatus(true, true, false);
            SendRegisterPackage();                //发注册包
            ReceiveData();
        }

        internal void Disconnect()
        {
            if (!Connected) return;

            SocketAsyncEventArgs e = m_socketAsyncEventArgsPool.Pop(this);
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
            m_socketAsyncEventArgsPool.Push(e);
            Logger.LogInformation($"{MacAddress} disconnect {Socket.RemoteEndPoint} on {Socket.LocalEndPoint}");

            CloseSocket();
        }

        private void SendData(byte[] data)
        {
            if (!Running) return;

            SocketAsyncEventArgs e = m_socketAsyncEventArgsPool.Pop(this);
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
            Logger.LogInformation($"{MacAddress} sended data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");

            m_socketAsyncEventArgsPool.Push(e);
        }

        internal void ReceiveData()
        {
            if (!Running) return;

            SocketAsyncEventArgs e = m_socketAsyncEventArgsPool.Pop(this);
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
                if (Running)
                {
                    //对方主动断开连接，异常处理
                    OnSocketError(e, true);
                }
                else
                {
                    m_socketAsyncEventArgsPool.Push(e);
                    //由Disconnect导致收到0长度字节，不用处理
                }
            }
            else
            {
                try
                {
                    Logger.LogInformation($"{MacAddress} receive data {e.Buffer.ConvertToString(e.Offset, e.BytesTransferred, ' ')}");
                    ProcessReceivedData(e);
                }
                catch { }

                m_socketAsyncEventArgsPool.Push(e);

                ReceiveData();
            }
        }

        private void OnException(Exception ex, SocketAsyncEventArgs e)
        {
            Logger.LogInformation($"{MacAddress} exception {ex}");

            if (ex is SocketException)
            {
                OnSocketError(e, false);
            }
            else
            {
            }
        }

        internal void OnSocketError(SocketAsyncEventArgs e, bool isReceiveZeroBytes)
        {
            SetStatus(false, Connected, Registered);

            if (e != null) m_socketAsyncEventArgsPool.Push(e);

            CloseSocket();
        }

        private void CloseSocket()
        {
            if (!Connected) return;

            lock (this)
            {
                if (!Connected) return;

                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                    Socket.Close();
                }
                catch { }
                SetStatus(false, false, false);
                Socket = null;

                //防止发送无效的心跳包
                LastSendTime = DateTime.Now;
                LastRecvTime = DateTime.Now;
            }
        }

        internal void CheckStatus(int errorSeconds, int _heartSeconds)
        {
            TimeSpan errorTimeSpan = TimeSpan.FromSeconds(errorSeconds);
            if (!Running && (DateTime.Now - LastRecvTime > errorTimeSpan || DateTime.Now - LastSendTime > errorTimeSpan))
            {
                Connect();
                return;
            }
            else if (Running && Connected && !Registered && (DateTime.Now - LastRecvTime > errorTimeSpan || DateTime.Now - LastSendTime > errorTimeSpan))
            {
                Disconnect();
                return;
            }
            else if (DateTime.Now - LastRecvTime > TimeSpan.FromSeconds(_heartSeconds) || DateTime.Now - LastSendTime > TimeSpan.FromSeconds(_heartSeconds))
            {
                SendHeartPackage();
                return;
            }
            else
            {
                //其它状态
            }
        }
        #endregion framework

        #region business
        internal void SendRegisterPackage()
        {
            string registerPackage = "register:" + MacAddress;
            byte[] data = Encoding.UTF8.GetBytes(registerPackage);

            SendData(data);
        }

        internal void SendHeartPackage()
        {
            string heartPackage = "FEFEFEFE68" + MacAddress + "68FF000016";
            byte[] data = ToBytes(heartPackage);

            SendData(data);
        }

        byte Crc(byte[] data, int offset, int count)
        {
            byte ret = 0;
            for (int index = 0; index < count; index++)
            {
                ret += data[index + offset];
            }

            return ret;
        }
        byte[] ToBytes(string data)
        {
            string tmp = data.Replace(" ", "").Replace("-", "");
            byte[] ret = new byte[tmp.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(tmp.Substring(i * 2, 2), 0x10);
            }

            return ret;
        }
        public static string GetAmmeter(byte[] buffer, int offset)
        {
            byte[] tmp = new byte[6];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 6);
            return BitConverter.ToString(tmp.Reverse().ToArray()).Replace("-", "");
        }

        private void ProcessReceivedData(SocketAsyncEventArgs e)
        {
            string receive = BitConverter.ToString(e.Buffer, e.Offset, e.BytesTransferred).Replace("-", "");

            if (receive.Contains("68110433333333"))
            {
                //抄读："0xFE 0xFE 0xFE 0xFE 0x68 0x31 0x00 0x00 0x12 0x16 0x20 0x68 0x11 0x04 0x33 0x33 0x33 0x33 0x2A 0x16 "
                byte[] deviceId = new byte[6];
                Buffer.BlockCopy(e.Buffer, e.Offset + 5, deviceId, 0, 6);
                string device = BitConverter.ToString(deviceId.Reverse().ToArray()).Replace("-", "");
                //string device = Simulator.getAmmeter(e.Buffer, e.Offset + 5);

                EndDevice endDevice = EndDevices.Single<EndDevice>(d => d.MacAddress == device);
                endDevice.Energy++;
                byte[] energy = ToBytes(endDevice.Energy.ToString().PadLeft(8, '0')).Reverse().ToArray();
                for (int i = 0; i < energy.Length; i++)
                {
                    energy[i] += 0x33;
                }
                //回应："0xFE,0xFE,0xFE,0xFE,0x68,0x31,0x00,0x00,0x12,0x16,0x20,0x68,0x91,0x08,0x33,0x33,0x33,0x33,0x73,0x35,0x34,0x33,0x01,0x16 " 102.4
                string response = "FEFEFEFE68"
                    + BitConverter.ToString(deviceId).Replace("-", "")
                    + "68910833333333"
                    // +"73353433"
                    + BitConverter.ToString(energy).Replace("-", "")
                    + "0116";

                byte[] data = ToBytes(response);
                data[^2] = Crc(data, 4, data.Length - 6);

                if (GatewayDelayMilliseconds > 0) Thread.Sleep(GatewayDelayMilliseconds);

                SendData(data);
            }
            else if (receive.Contains("68FF00"))
            {
                //心跳包: FEFEFEFE68D8B04CE9291668FF000016
                //不用回复
            }
            else if (receive.Contains("72656769737465723A"))
            {
                if (receive.Contains("72656769737465723A6F6B"))
                {
                    SetStatus(true, true, true);
                }
                else
                {
                    CloseSocket();
                }
            }
            else
            {
                //其它情况
            }
        }
        #endregion business
        public override string ToString()
        {
            return GatewayId;
        }
    }
}
