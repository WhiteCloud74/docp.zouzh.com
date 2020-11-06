using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.Read, FunctionCode = AmmeterFunctionCode.CombineEnergy)]
    sealed public class GetAmmeterEnergy : IAmmeterCommand
    {
        public void DecodeInnerData(BinaryReader br)
        {
           //have nothing to do
        }

        public byte[] EncodeInnerData()
        {
            return new byte[0];
        }
    }
}
