using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.SetOnOff, FunctionCode = AmmeterFunctionCode.writeNotNeed)]
    public class SetAmmeterOnOff : IAmmeterCommand, IFromRedisCommand
    {
        /// <summary>
        /// 密码权限
        /// </summary>
        public byte PwAuth { get; set; }
        public string P0P1P2 { get; set; }
        public string C0C1C2 { get; set; }
        /// <summary>
        /// 0x1a断开，0x1b合闸
        /// </summary>
        public byte N1 { get; set; }
        public string N3_N8 { get; set; } = "335115180890";
        public void DecodeInnerData(BinaryReader br)
        {
            PwAuth = br.ReadByte();
            P0P1P2 = br.ReadBytes(3).ConvertToString();
            C0C1C2 = br.ReadBytes(3).ConvertToString();
            N1 = br.ReadByte();
            N3_N8 = br.ReadBytes(6).ConvertToString();
        }

        public byte[] EncodeInnerData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(PwAuth);
                bw.Write(P0P1P2.ConvertToByteArray());
                bw.Write(C0C1C2.ConvertToByteArray());
                bw.Write(N1);
                bw.Write(N3_N8.ConvertToByteArray());
                byte[] ret = ms.ToArray();
                return ret;
            }
        }

        public void FromRedisCommand(RedisCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
