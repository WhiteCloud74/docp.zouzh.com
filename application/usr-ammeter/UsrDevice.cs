using Ammeter;
using FrameworkCore.Instrument;
using FrameworkCore.SocketAdapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace usr_ammeter
{
    /// <summary>
    /// 单个设备
    /// 设备由上层的Connector或Listener管理
    /// </summary>
    public class UsrDevice : TcpSocketAdapter
    {
        readonly MyWaiter<bool> init = new MyWaiter<bool>();
        readonly UsrDeviceProtocolConvert _protocolConvert;

        public UsrDevice()
        {
            IsWorkOnServer = false;

            _protocolConvert = new UsrDeviceProtocolConvert();
            OnReceivedData += UserDevice_OnReceivedData;
            OnPulse += UserDevice_OnPulse;
        }

        private void UserDevice_OnPulse()
        {
            if ((DateTime.Now - LastRecvTime) > TimeSpan.FromSeconds(int.Parse(AdapterParameters["HeartSeconds"])) &&
                (DateTime.Now - LastSendTime) > TimeSpan.FromSeconds(int.Parse(AdapterParameters["HeartSeconds"])))
            {
                string heartPackage = $"FEFEFEFE68{MacAddress}";
                byte[] data = Encoding.Default.GetBytes(heartPackage);
                SendData(data);
            }
        }

        private void UserDevice_OnReceivedData(SocketAsyncEventArgs e)
        {
            string receive = BitConverter.ToString(e.Buffer, e.Offset, e.BytesTransferred).Replace("-", "");
            if (receive.StartsWith("72656769737465723A"))
            {
                string register = Encoding.Default.GetString(e.Buffer, e.Offset, e.BytesTransferred);

                if (register.Contains("register:ok"))
                {
                    //收到注册包的回应消息，注册成功
                    init.SetResult(true);
                }
            }
            else if (receive.StartsWith("FEFEFEFE68"))
            {
                //心跳包的回应，不用处理
            }
            else
            {
                MemoryStream ms = new MemoryStream(e.Buffer, e.Offset, e.BytesTransferred);
                using BinaryReader br = new BinaryReader(ms);
                AmmeterCommand command = new AmmeterCommand();
                command.Decode(br);

                AmmeterCommand response = _protocolConvert.CreateResponse(command);
                SendData(response.Encode());
            }
        }

        protected override async Task<bool> InitAdapter()
        {
            //发送注册包
            string registerPackage = $"register:{MacAddress}";
            byte[] data = Encoding.Default.GetBytes(registerPackage);
            SendData(data);

            //等待回应
            var ret = await init;
            if (ret) await _protocolConvert.InitAsync();
            return ret;
        }
    }
}
