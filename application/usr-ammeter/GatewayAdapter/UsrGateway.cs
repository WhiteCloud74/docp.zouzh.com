using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using GatewayService.SocketAdapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using usr_ammeter.AmmeterProtocol;

namespace usr_ammeter.GatewayAdapter
{
    public class UsrGateway : TcpSocketAdapterServer
    {
        readonly MyWaiter<bool> init = new MyWaiter<bool>();
        List<SendedCommand> temps = new List<SendedCommand>();

        public override async Task<bool> InitAsync()
        {
            return await init;
        }

        public override void ProcessRedisCommand(RedisCommand command)
        {
            AmmeterCommand ammeterCommand = new AmmeterCommand();
            ammeterCommand.FromRedisCommand(command);

            byte[] data = ammeterCommand.Encode();
            SendData(data);
            temps.Add(new SendedCommand() { AmmeterCommand = ammeterCommand, RedisCommand = command });
        }

        public override async Task ProcessSocketDataAsync(SocketAsyncEventArgs e)
        {
            string receive = BitConverter.ToString(e.Buffer, e.Offset, e.BytesTransferred).Replace("-", "");
            if (receive.StartsWith("72656769737465723A"))
            {
                string register = Encoding.Default.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                MacAddress = register.Split(':')[1];
                init.SetResult(true);

                byte[] response = Encoding.Default.GetBytes("register:ok");
                SendData(response);
            }
            else if (receive.StartsWith("FEFEFEFE68"))
            {
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

                RedisCommand redisCommand = MatchCommand(command);
                await RedisService.GatewayServerPublishCommandAsync(redisCommand);
            }
        }

        private RedisCommand MatchCommand(AmmeterCommand command)
        {
            RedisCommand ret = null;
            if (command.NeedMatchCommand)
            {
                SendedCommand temp = null;
                foreach (var item in temps)
                {
                    if (command.MatchCommand(item.AmmeterCommand))
                    {
                        temp = item;
                        ret = item.RedisCommand;
                    }
                }
                if (temp != null) temps.Remove(temp);
            }

            if (ret == null)
            {
                ret = new RedisCommand()
                {
                    ApplicateServerId = Guid.Empty.ToString(),
                    CommandId = Guid.NewGuid().ToString(),
                    CommandType = CommandType.EventReport,
                    GatewayId = GatewayId.ToString(),
                };
            }

            command.ToRedisCommand(ref ret);
            return ret;
        }
    }
    class SendedCommand
    {
        internal RedisCommand RedisCommand { get; set; }
        internal AmmeterCommand AmmeterCommand { get; set; }
    }
}