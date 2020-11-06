using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FrameworkCore.Instrument
{
    public static class StringExtent
    {
        private const string _ValidCharCountIsNotEven = "字符串中有效字符数量不是偶数";
        private const string _IncludeInvalidChar = "字符串中包含无效字符";
        private const string _IsNullOrWhiteSpace = "字符串为空或空格";

        /// <summary>
        /// 转换十六进制数值字符串为相应值的Byte串，字符串中无分隔符
        /// </summary>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ConvertToByteArray(this string str)
        {
            if (str != null)
            {

                if (string.IsNullOrWhiteSpace(str))
                {
                    throw new ArgumentException(_IsNullOrWhiteSpace);
                }
                else if (!Regex.IsMatch(str, "^[0-9A-Fa-f]+$"))
                {
                    throw new ArgumentException(_IncludeInvalidChar);
                }
                else if (str.Length % 2 != 0)
                {
                    throw new ArgumentException(_ValidCharCountIsNotEven);
                }

                byte[] ret = new byte[str.Length / 2];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = Convert.ToByte(str.Substring(i * 2, 2), 0x10);
                }
                return ret;
            }
            return null;

        }

        /// <summary>
        /// 转换十六进制数值字符串为相应值的Byte串，字符串以splitChars中任意字符为分隔符
        /// </summary>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="str"></param>
        /// <param name="splitChars"></param>
        /// <returns></returns>
        public static byte[] ConvertToByteArray(this string str, char[] splitChars)
        {
            StringBuilder sb = new StringBuilder();
            CharEnumerator enumerator = str.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!splitChars.Contains(enumerator.Current))
                {
                    sb.Append(enumerator.Current);
                }
            }
            return ConvertToByteArray(sb.ToString());
        }

        /// <summary>
        /// 转换十六进制数值字符串为相应值的Byte串，字符串以splitChar为分隔符
        /// </summary>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="str"></param>
        /// <param name="splitChar">分隔符</param>
        /// <returns></returns>
        public static byte[] ConvertToByteArray(this string str, char splitChar)
        {
            return ConvertToByteArray(str, new char[] { splitChar });
        }

        public static byte[] ConvertToByteArrayOneByOne(this string str)
        {
            byte[] ret = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                ret[i] = (byte)str[i];
            }
            return ret;
        }
        public static byte[] StringArrayConvertToByteArray(this string[] str)
        {
            if (null != str)
            {
                byte[] array = new byte[str.Length];
                int i = 0;
                foreach (string item in str)
                {
                    array[i] = Convert.ToByte(item, 10);
                    ++i;
                }
                return array;
            }
            return null;
        }
    }
}
