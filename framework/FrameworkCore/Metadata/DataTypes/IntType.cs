using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.Metadata.DataTypes
{
    public class IntType : DataType
    {
        public int Max { get; set; }
        public int Min { get; set; }

        public override bool IsValid(string dataValue)
        {
            return int.Parse(dataValue) >= Min && int.Parse(dataValue) <= Max;
        }
    }
}