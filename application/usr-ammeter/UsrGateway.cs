using Ammeter;
using FrameworkCore.Instrument;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Redis;
using FrameworkCore.Service;
using FrameworkCore.SocketAdapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace usr_ammeter.GatewayAdapter
{
    /// <summary>
    /// 单个网关
    /// 网关由上层的Connector或Listener管理
    /// </summary>
    public class UsrGateway : TcpSocketAdapter
    {
        readonly MyWaiter<bool> init = new MyWaiter<bool>();
        readonly UsrGatewayProtocolConvert _protocolConvert;

        public UsrGateway()
        {
            IsWorkOnServer = true;
            _protocolConvert = new UsrGatewayProtocolConvert();

            OnReceivedData += async (e) => { await ProcessReceivedDataAsync(e); };
            OnPulse += async () => { await ProcessReceivedAmmeterCommandAsync(null); };
        }

        protected override async Task<bool> InitAdapter()
        {
            //等待注册包
            var ret = await init;
            if (ret) await _protocolConvert.InitAsync();
            return ret;
        }

        protected override async Task ProcessRedisCommandAsync(RedisCommand command)
        {
            _protocolConvert.AddRedisCommand(command);
            await ProcessReceivedAmmeterCommandAsync(null);
        }

        private async Task ProcessReceivedAmmeterCommandAsync(AmmeterCommand command)
        {
            if (command != null
                || ((DateTime.Now - LastRecvTime) > TimeSpan.FromMilliseconds(int.Parse(AdapterParameters["DeviceTimeoutMilliseconds"]))
                    && (DateTime.Now - LastSendTime) > TimeSpan.FromMilliseconds(int.Parse(AdapterParameters["DeviceTimeoutMilliseconds"]))))
            {
                RedisCommand redisCommand = _protocolConvert.MatchCommand(command);
                if (redisCommand != null)
                {
                    await RedisService.GatewayServerPublishCommandAsync(redisCommand);
                }

                AmmeterCommand nextCommand = _protocolConvert.GetNextAmmeterCommand();
                if (command != null)
                {
                    byte[] data = nextCommand.Encode();
                    SendData(data);
                }
            }
        }

        protected async Task ProcessReceivedDataAsync(SocketAsyncEventArgs e)
        {
            string receive = BitConverter.ToString(e.Buffer, e.Offset, e.BytesTransferred).Replace("-", "");
            if (receive.StartsWith("72656769737465723A"))
            {
                //收到注册包,回应注册成功
                string register = Encoding.Default.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                MacAddress = register.Split(':')[1].Substring(0, AmmeterCommand._meterAddressLength * 2);

                byte[] response = Encoding.Default.GetBytes("register:ok");
                SendData(response);

                //确定网关Id，若是新的网关，保存数据库
                await RegisterAndGetGatewayInfoAsync();

                init.SetResult(true);
            }
            else if (receive.StartsWith("FEFEFEFE68"))
            {
                //心跳包，原封不动返回来，不用处理
                byte[] response = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, response, 0, e.BytesTransferred);
                SendData(response);
            }
            else
            {
                MemoryStream ms = new MemoryStream(e.Buffer, e.Offset, e.BytesTransferred);
                using BinaryReader br = new BinaryReader(ms);
                AmmeterCommand command = new AmmeterCommand();
                command.Decode(br);

                await ProcessReceivedAmmeterCommandAsync(command);
            }
        }

        private async Task RegisterAndGetGatewayInfoAsync()
        {
            var currentDevice = await DeviceService.SearchAsync(d => d.MacAddress == MacAddress);
            if (currentDevice.Count() > 0)
            {
                //Redis中已经注册过，设备Id
                var gateway = currentDevice.ElementAt(0);
                AdapterId = gateway.GatewayId.ToString();
                //var ammeters = await DeviceService.SearchAsync(a => a.GatewayId == gateway.GatewayId && a.DeviceId != gateway.DeviceId);
                //foreach (var ammeter in ammeters)
                //{
                //    Ammeters.Add(new KeyValuePair<string, string>(ammeter.DeviceId.ToString(), ammeter.MacAddress));
                //}
            }
            else
            {
                //没注册过，保存数据库
                Device usrDevice = await DeviceService.GetDeviceTemplate(UsrHelper.UsrDevcieProductId);
                usrDevice.DeviceId = Guid.NewGuid();
                usrDevice.GatewayId = usrDevice.DeviceId; //网关就是自己
                usrDevice.IsGateway = true;
                usrDevice.IsIndependentOnline = true;
                usrDevice.IsOnLine = true;
                usrDevice.MacAddress = MacAddress;

                if (await DeviceService.AddDeviceAsync(usrDevice))
                {
                    AdapterId = usrDevice.GatewayId.ToString();
                }
            }
        }
    }
}