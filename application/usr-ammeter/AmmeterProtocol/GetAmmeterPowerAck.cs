using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.ReadAck, FunctionCode = AmmeterFunctionCode.ApparentPower)]
    public class GetAmmeterPowerAck : IAmmeterCommand, IToRedisCommand
    {
        public double Power { get; set; }
        public void DecodeInnerData(BinaryReader br)
        {
            byte[] temp = br.ReadBytes(3);
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] -= AmmeterCommand.NumberBase;
            }
            Power = int.Parse(temp.ReverseArray().ConvertToString()) / 10000.0;
        }

        public byte[] EncodeInnerData()
        {
            using MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] PowerValue = (((int)(Power * 10000)).ToString().PadLeft(6, '0').ConvertToByteArray()).ReverseArray();
            for (int i = 0; i < PowerValue.Length; i++)
            {
                PowerValue[i] += AmmeterCommand.NumberBase;
            }
            bw.Write(PowerValue);

            return ms.ToArray();
        }

        public void ToRedisCommand(RedisCommand redisCommand)
        {
            redisCommand.Response.Add(new KeyValuePair<string, string>("Power", Power.ToString("f2")));
        }
    }
}
