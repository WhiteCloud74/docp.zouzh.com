using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ammeter
{
    public class GetAmmeterEnergyAck : IAmmeterCommand
    {
        public double Energy { get; set; }

        public void DecodeInnerData(BinaryReader br)
        {
            byte[] value = br.ReadBytes(4);
            for (int i = 0; i < value.Length; i++)
            {
                value[i] -= AmmeterCommand.NumberBase;
            }
            Energy = int.Parse(value.ReverseArray().ConvertToString()) / 100.0;
        }

        public byte[] EncodeInnerData()
        {
            using MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] energyValue = (((int)(Energy * 100)).ToString().PadLeft(8, '0').ConvertToByteArray()).ReverseArray();
            for (int i = 0; i < energyValue.Length; i++)
            {
                energyValue[i] += AmmeterCommand.NumberBase;
            }
            bw.Write(energyValue);

            return ms.ToArray();
        }
    }
}
