using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.ReadAck, FunctionCode = AmmeterFunctionCode.Address)]
    public class GetAmmeterAddressAck : IAmmeterCommand, IToRedisCommand
    {
        public string meterAddress { get; set; }
        public void DecodeInnerData(BinaryReader br)
        {
            byte[] temp = br.ReadBytes(6).ReverseArray();
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] -= 0x33;
            }
            meterAddress = temp.ConvertToString();
        }

        public byte[] EncodeInnerData()
        {
            using MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] temp = meterAddress.PadLeft(12, '0').ConvertToByteArray().ReverseArray();
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] += 0x33;
            }
            bw.Write(temp);

            return ms.ToArray();
        }

        public void ToRedisCommand(RedisCommand redisCommand)
        {
            redisCommand.Response.Add(new KeyValuePair<string, string>("MeterAddress", meterAddress));
        }
    }
}
