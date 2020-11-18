using Ammeter;
using FrameworkCore.Metadata.DeviceDefine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace usr_ammeter
{
    public class UsrDeviceProtocolConvert
    {
        readonly Random random = new Random();
        readonly Dictionary<string, Ammeter> _ammeters = new Dictionary<string, Ammeter>();         //网关设备下的电表
        public async Task InitAsync()
        {
            var ret = await UsrHelper.GetAmmetersAsync(null);
            foreach (var ammeter in ret)
            {
                _ammeters.Add(ammeter.MacAddress, ammeter);
            }
        }
        internal AmmeterCommand CreateResponse(AmmeterCommand command)
        {
            //若没有此电表，视作新增的
            if (_ammeters.ContainsKey(command.MeterAddress))
            {
                _ammeters.Add(command.MeterAddress, new Ammeter()
                {
                    Energy = 0,
                    Power = 100,
                    MacAddress = command.MeterAddress
                });
            }
            AmmeterCommand ret = new AmmeterCommand() { MeterAddress = command.MeterAddress };
            switch (command._command.GetType().Name)
            {
                case nameof(GetAmmeterPower):
                    ret._command = new GetAmmeterPowerAck()
                    {
                        Power = random.Next(8, 13) / 10.0 * _ammeters[command.MeterAddress].Power
                    };
                    break;
                case nameof(GetAmmeterEnergy):
                    _ammeters[command.MeterAddress].Energy += random.Next(1, 10) / 10.0;
                    ret._command = new GetAmmeterEnergyAck()
                    {
                        Energy = _ammeters[command.MeterAddress].Energy
                    };
                    break;
                default:
                    throw new ApplicationException("啥命令啊！");
                    //break;
            }
            return ret;
        }
    }
}
