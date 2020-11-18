using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.SocketAdapter
{
    /// <summary>
    /// 系统中服务器端
    /// 可以是TcpClient也可以是TcpServer
    /// </summary>
    public class TcpSocketAdapterServer : TcpSocketAdapter
    {
        internal protected override bool IsAbnormal()
        {
            throw new NotImplementedException();
        }
    }
}
