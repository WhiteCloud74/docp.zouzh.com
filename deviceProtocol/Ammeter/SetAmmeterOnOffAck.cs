
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ammeter
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.SetOnOffAck, FunctionCode = AmmeterFunctionCode.writeNotNeed)]
    public class SetAmmeterOnOffAck : IAmmeterCommand
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
    }
}
