using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.SocketAdapter
{
    /// <summary>
    /// 系统中的设备端
    /// 可以是TcpClient也可以是TcpServer
    /// </summary>
    public class TcpSocketAdapterDevice : TcpSocketAdapter
    {
        public TcpSocketAdapterDevice()
        {
            OnAccepted += SocketAdapter_OnAccepted;
            OnConnected += SocketAdapter_OnConnected;
            OnReceivedData += SocketAdapter_OnReceivedData;
        }

        private void SocketAdapter_OnReceivedData(System.Net.Sockets.SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SocketAdapter_OnConnected(System.Net.Sockets.SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SocketAdapter_OnAccepted(System.Net.Sockets.SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
