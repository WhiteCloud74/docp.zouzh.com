using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.SetOnOffAck, FunctionCode = AmmeterFunctionCode.writeNotNeed)]
    public class SetAmmeterOnOffAck : IAmmeterCommand, IToRedisCommand
    {
        public bool success = true;
        public void DecodeInnerData(BinaryReader br)
        {
            if (br.BaseStream.Position != br.BaseStream.Length - 2)
            {
                success = false;
            }
        }

        public byte[] EncodeInnerData()
        {
            return new byte[0];
        }

        public void ToRedisCommand(RedisCommand redisCommand)
        {
            redisCommand.Response.Add(new KeyValuePair<string, string>("Result", success ? "True" : "False"));
        }
    }
}
