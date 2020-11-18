using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ammeter
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.Read, FunctionCode = AmmeterFunctionCode.ApparentPower)]
    sealed public class GetAmmeterPower : IAmmeterCommand
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
