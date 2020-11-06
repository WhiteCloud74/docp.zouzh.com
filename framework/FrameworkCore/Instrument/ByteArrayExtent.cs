using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.Instrument
{
    public static class ByteArrayExtent
    {
        public static string ConvertToString(this byte[] data, char splitChart)
        {
            return ConvertToString(data, 0, data.Length, splitChart);
        }

        public static string ConvertToString(this byte[] data)
        {
            return ConvertToString(data, 0, data.Length, ' ');
        }

        public static string ConvertToString(this byte[] data, int startIndex, int length, char splitChart)
        {
            return BitConverter.ToString(data, startIndex, length).Replace('-', splitChart);
        }

        public static string ByteArrayToString(this byte[] data)
        {
            if (data == null) return "";
            char[] charArray = new char[data.Length];
            for (int i = 0; i < charArray.Length; i++)
            {
                charArray[i] = (char)data[i];
            }
            return (new string(charArray));

        }
        public static byte[] ReverseArray(this byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            int count = data.Length;
            for (int i = 0; i < count / 2; i++)
            {
                byte temp = data[count - 1 - i];
                data[count - 1 - i] = data[i];
                data[i] = temp;
            }
            return data;
        }
        public static string ReverseString(this string str)
        {
            char[] data = new char[str.Length];
            data = str.ToCharArray();
            if (data == null)
            {
                return null;
            }
            int count = data.Length;
            for (int i = 0; i < count / 2; i++)
            {
                char temp = data[count - 1 - i];
                data[count - 1 - i] = data[i];
                data[i] = temp;
            }
            return new string(data);
        }
        private const ushort _poly = 0x8005;
        private const string _StartOffsetOutOfRange = "开始位置不能小于0,也不能大于字节串长度";
        private const string _TotalLengthOutOfRange = "开始位置与长度之和,不能大于字节串长度";
        private const string _ArrayLengthIsTooShort = "数组长度太短或开始位置过大";
        private const string _LengthParameterOutOfRange = "长度不能小于1,也不能大于8";
        /// <summary>
        /// 计算byte串的CRC
        /// </summary>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort Crc(this byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            return Crc(data, 0, data.Length);
        }
        public static ushort CrcBaoBo(this byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            return CrcBaoBo(data, 0, data.Length, 0xA001);
        }

        /// <summary>
        /// 计算byte串的CRC
        /// </summary>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="data"></param>
        /// <param name="startOffset">开始byte偏移</param>
        /// <param name="length">byte串长度</param>
        /// <returns></returns>
        public static ushort Crc(this byte[] data, int startOffset, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            else if (startOffset < 0 || startOffset >= data.Length)
            {
                throw new ArgumentException(_StartOffsetOutOfRange);
            }
            else if (length + startOffset > data.Length)
            {
                throw new ArgumentException(_TotalLengthOutOfRange);
            }

            ushort crc = 0;
            while (length-- > 0)
            {
                byte bt = data[startOffset++];
                for (int i = 0; i < 8; i++)
                {
                    bool b1 = (crc & 0x8000U) != 0;
                    bool b2 = (bt & 0x80U) != 0;
                    if (b1 != b2) crc = (ushort)((crc << 1) ^ _poly);
                    else crc <<= 1;
                    bt <<= 1;
                }
            }
            return crc;
        }
        public static ushort CrcBaoBo(this byte[] data, int startOffset, int length, ushort poly = 0xA001)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            else if (startOffset < 0 || startOffset >= data.Length)
            {
                throw new ArgumentException(_StartOffsetOutOfRange);
            }
            else if (length + startOffset > data.Length)
            {
                throw new ArgumentException(_TotalLengthOutOfRange);
            }

            ushort crc = 0xFFFF;
            while (length-- > 0)
            {
                byte bt = data[startOffset++];
                crc = (ushort)(crc ^ (ushort)(bt));
                for (int i = 0; i < 8; i++)
                {
                    bool flag = (crc & 0x0001) == 0x1U;
                    crc = (ushort)(crc >> 1);
                    if (flag)
                    {
                        crc ^= poly;
                    }
                }
            }
            return crc;
        }

        public static byte AccumulativeTotal(this byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            return AccumulativeTotal(data, 0, data.Length);
        }

        public static byte AccumulativeTotal(this byte[] data, int startOffset, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            else if (startOffset < 0 || startOffset >= data.Length)
            {
                throw new ArgumentException(_StartOffsetOutOfRange);
            }
            else if (length + startOffset > data.Length)
            {
                throw new ArgumentException(_TotalLengthOutOfRange);
            }

            byte accumulativeTotal = 0;
            for (int i = 0; i < length; i++)
            {
                accumulativeTotal += data[i + startOffset];
            }
            return accumulativeTotal;
        }

        /// <summary>
        /// 字节数组是否是另一个字节数组的开头部分
        /// </summary>
        /// <exception cref="System.ArgumentException"/>
        /// <param name="data"></param>
        /// <param name="destination"></param>
        /// <param name="beginOffset"></param>
        /// <returns></returns>
        public static bool AreStartOf(this byte[] data, byte[] destination, int beginOffset)
        {
            if (destination.Length < data.Length)
            {
                return false;
            }

            if (beginOffset < 0 || beginOffset > destination.Length - data.Length)
            {
                throw new ArgumentException(_StartOffsetOutOfRange);
            }

            for (int index = 0; index < data.Length; index++)
            {
                if (data[index] != destination[index + beginOffset])
                {
                    return false;
                }
            }
            return true;
        }

        public static UInt64 GetData(this byte[] data, int beginOffset, int length)
        {
            if (length < 1 || length > sizeof(UInt64))
            {
                throw new ArgumentException(_LengthParameterOutOfRange);
            }

            if (beginOffset < 0 || beginOffset > data.Length - length)
            {
                throw new ArgumentException(_StartOffsetOutOfRange);
            }

            UInt64 ret = data[beginOffset];
            for (int index = 1; index < length; index++)
            {
                ret += (ulong)data[beginOffset + index] << (8 * index);
            }

            return ret;
        }

        public static byte[] Security(this byte[] data)
        {
            throw new NotImplementedException();
        }

    }

}
