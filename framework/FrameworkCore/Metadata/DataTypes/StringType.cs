using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FrameworkCore.Metadata.DataTypes
{
    public class StringType : MyDataType
    {
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public string RegexString { get; set; }

        public override bool IsValid(string dataValue)
        {
            return dataValue.Length >= MinLength && dataValue.Length <= MaxLength
                && (string.IsNullOrEmpty(RegexString) || Regex.IsMatch(dataValue, RegexString));
        }
    }
}
