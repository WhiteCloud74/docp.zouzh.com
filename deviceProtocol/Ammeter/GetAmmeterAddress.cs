using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ammeter
{
    [AmmeterCommand(ControlCode = AmmeterControlCode.Read, FunctionCode = AmmeterFunctionCode.Address)]
    sealed public class GetAmmeterAddress : IAmmeterCommand
    {
        void IAmmeterCommand.DecodeInnerData(BinaryReader br)
        {
            //have nothing to do
        }
        byte[] IAmmeterCommand.EncodeInnerData()
        {
            return new byte[0];
        }
    }
}
