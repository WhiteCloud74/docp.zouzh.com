using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace usr_ammeter.AmmeterProtocol
{
    public enum AmmeterControlCode : byte
    {
        Read = 0x11,
        ReadAck = 0x91,
        ReadError = 0xD1,
        ReadSubsequence = 0x12,
        ReadSubsequenceAck = 0x92,
        ReadSubsequenceError = 0xD2,
        Write = 0x14,
        WriteAck = 0x94,
        WriteError = 0xD4,
        ReadAddress = 0x13,
        ReadAddressAck = 0x93,
        WriteAddress = 0x15,
        WriteAddressAck = 0x95,
        BroadCaseCheckTime = 0x08,
        SetOnOff = 0x1c,
        SetOnOffAck = 0x9c,
    }
    public enum AmmeterFunctionCode : UInt32
    {
        Address = 0X04000402,                       //电表号
        CombineEnergy = 0X00000000,             //组合有功电能
        ForwardEnergy = 0X00010000,             //正向有功电能
        ReverseEnergy = 0X00020000,             //反向有功电能
        Voltage = 0X0201FF00,                       //电压
        Current = 0X0202FF00,                       //电流
        ActivePower = 0X0203FF00,               //有功功率
        ReactivePower = 0X0204FF00,             //无功功率
        ApparentPower = 0X0205FF00,             //视在功率
        PowerFactor = 0X0206FF00,               //功率因数
        writeNotNeed = 0XFFFFFFFF,              //设置，没有功能码，作为占位符
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class AmmeterCommandAttribute : Attribute
    {
        public AmmeterControlCode ControlCode { get; set; }
        public AmmeterFunctionCode FunctionCode { get; set; }
    }
    public static class AmmeterCommandAttributeExtend
    {
        public static AmmeterFunctionCode FunctionCode(this IAmmeterCommand deviceCommand)
        {
            return ((AmmeterCommandAttribute)deviceCommand.GetType().GetCustomAttributes(typeof(AmmeterCommandAttribute), false).Single()).FunctionCode;
        }
        public static AmmeterControlCode ControlCode(this IAmmeterCommand deviceCommand)
        {
            return ((AmmeterCommandAttribute)deviceCommand.GetType().GetCustomAttributes(typeof(AmmeterCommandAttribute), false).Single()).ControlCode;
        }
    }

    public interface IAmmeterCommand
    {
        void DecodeInnerData(BinaryReader br);
        byte[] EncodeInnerData();
    }
    public interface IFromRedisCommand
    {
        void FromRedisCommand(RedisCommand command);
    }
    public interface IToRedisCommand
    {
        void ToRedisCommand(RedisCommand redisCommand);
    }

    public class AmmeterCommand
    {
        public const byte NumberBase = 0x33;
        public const string CommonAMmeterAddress = "AAAAAAAAAAAA";
        const byte _beginFlag = 0x68;
        const byte _endFlag = 0x16;
        const byte _headFlag = 0xFE;
        public const int _meterAddressLength = 6;
        const int _functionCodeLength = 4;
        public string MeterAddress { get; set; }
        public IAmmeterCommand _command;

        public void Decode(BinaryReader br)
        {
            byte beginFlag = br.ReadByte();
            while (beginFlag == _headFlag)
            {
                beginFlag = br.ReadByte();
            }
            Debug.Assert(beginFlag == _beginFlag);

            MeterAddress = br.ReadBytes(_meterAddressLength).ReverseArray().ConvertToString();

            byte middleFlag = br.ReadByte();
            Debug.Assert(middleFlag == _beginFlag);

            AmmeterControlCode controlCode = (AmmeterControlCode)br.ReadByte();
            byte dataLength = br.ReadByte();

            CreateCommand(controlCode, br);

            byte check = br.ReadByte();
            byte endFlag = br.ReadByte();
            Debug.Assert(endFlag == _endFlag);

            void CreateCommand(AmmeterControlCode controlCode, BinaryReader br)
            {
                AmmeterFunctionCode functionCode = AmmeterFunctionCode.writeNotNeed;
                if (NeedFunctionCode(controlCode))
                {
                    functionCode = (AmmeterFunctionCode)(br.ReadUInt32() - 0X33333333);
                }

                if (_commandTypes.Keys.Contains($"{controlCode}:{ functionCode}"))
                {
                    _command = (IAmmeterCommand)Activator.CreateInstance(_commandTypes[$"{controlCode}:{ functionCode}"]);
                }
                else
                {
                    throw new ApplicationException($"{controlCode}:{ functionCode}没有定义");
                }
                _command.DecodeInnerData(br);
            }

        }

        public byte[] Encode()
        {
            using MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(_headFlag);
            bw.Write(_headFlag);
            bw.Write(_headFlag);
            bw.Write(_headFlag);
            bw.Write(_beginFlag);
            bw.Write(MeterAddress.ConvertToByteArray().ReverseArray());
            bw.Write(_beginFlag);
            bw.Write((byte)_command.ControlCode());

            byte[] innerData = _command.EncodeInnerData();
            if (NeedFunctionCode(_command.ControlCode()))
            {
                bw.Write((byte)(innerData.Length + _functionCodeLength));
                bw.Write(((UInt32)_command.FunctionCode() + 0X33333333));
            }
            else
            {
                bw.Write((byte)(innerData.Length));
            }
            bw.Write(innerData);
            bw.Write((byte)0);
            bw.Write(_endFlag);
            byte[] ret = ms.ToArray();

            byte cs = CheckSum(ret, 4, ret.Length - 6);
            ret[^2] = cs;
            return ret;
        }

        internal void FromRedisCommand(RedisCommand command)
        {
            MeterAddress = command.DeviceId;
            (_command as IFromRedisCommand)?.FromRedisCommand(command);
        }

        internal void ToRedisCommand(ref RedisCommand redisCommand)
        {
            redisCommand.DeviceId = MeterAddress;
            (_command as IToRedisCommand)?.ToRedisCommand(redisCommand);
        }

        public bool NeedMatchCommand => this.GetType().Name.EndsWith("Ack");
        public bool MatchCommand(AmmeterCommand command)
        {
            if (command == null || this.GetType().Name != command.GetType().Name + "Ack")
            {
                return false;
            }

            return command.MeterAddress == MeterAddress || command.MeterAddress == AmmeterCommand.CommonAMmeterAddress;
        }


        #region static
        private static bool NeedFunctionCode(AmmeterControlCode controlCode)
        {
            bool need = false;
            switch (controlCode)
            {
                case AmmeterControlCode.Read:
                case AmmeterControlCode.ReadAck:
                case AmmeterControlCode.ReadError:
                case AmmeterControlCode.ReadSubsequence:
                case AmmeterControlCode.ReadSubsequenceAck:
                case AmmeterControlCode.ReadSubsequenceError:
                case AmmeterControlCode.ReadAddress:
                case AmmeterControlCode.ReadAddressAck:
                    need = true;
                    break;
                case AmmeterControlCode.Write:
                case AmmeterControlCode.WriteAck:
                case AmmeterControlCode.WriteError:
                case AmmeterControlCode.WriteAddress:
                case AmmeterControlCode.WriteAddressAck:
                case AmmeterControlCode.BroadCaseCheckTime:
                case AmmeterControlCode.SetOnOff:
                case AmmeterControlCode.SetOnOffAck:
                default:
                    break;
            }
            return need;
        }
        static byte CheckSum(byte[] data, int beginPos, int length)
        {
            byte checkSum = 0;
            for (int i = beginPos; i < beginPos + length; i++)
            {
                checkSum += data[i];
            }
            return checkSum;
        }

        static Dictionary<string, Type> _commandTypes;
        static AmmeterCommand()
        {
            Type currentType = null;
            _commandTypes = new Dictionary<string, Type>();
            CommonFunction.SearchAllTypeInAssembly((item) =>
            {
                try
                {
                    currentType = item;
                    AmmeterCommandAttribute attribute = (AmmeterCommandAttribute)item.GetCustomAttribute(typeof(AmmeterCommandAttribute));
                    if (attribute != null)
                    {
                        _commandTypes.Add($"{attribute.ControlCode}:{ attribute.FunctionCode}", item);
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"{currentType.Name} function code repeat", e);
                }
            });
        }
        #endregion static
    }

}
