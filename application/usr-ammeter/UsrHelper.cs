using Ammeter;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace usr_ammeter
{
    public class UsrHelper
    {
        public const string UsrDevcieProductId = "5baa93cb-4fcb-46c5-b4e7-d7a395dd2f12";
        public const string AmmeterProductId = "dc34ae59-dccd-4846-8083-350e97e446df";
        public Random _random = new Random();

        public static Product CreateGatewayProduct()
        {
            return new Product()
            {
                ProductId = new Guid(UsrDevcieProductId),
                ProductName = "有人设备",
                IsGateway = true,
                IsIndependentOnline = true,
            };
        }

        public static Product CreateAmmeterProduct()
        {
            return new Product()
            {
                ProductId = new Guid(AmmeterProductId),
                ProductName = "智能电表",
                IsGateway = false,
                IsIndependentOnline = false,
                ProductBrands = new List<ProductField>(),
                ProductNameplates = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="变比",IsReadOnly=true,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=200, Min=1,  }},
                },
                ProductProperties = new List<ProductField>() {
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="电量",IsReadOnly=true,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=999999, Min=0,  }},
                    new ProductField(){ ProductFieldId=Guid.NewGuid(), ProductFieldName="功率",IsReadOnly=true,
                        MyDataType=new IntType(){ MyDataTypeId=Guid.NewGuid(), Max=2000, Min=0,  }},
                },
                ProductEvents = new List<ProductEvent>(),
                ProductFunctions = new List<ProductFunction>(),
            };
        }

        public static async Task<List<Device>> CreateUsrDevice(int count)
        {
            List<Device> devices = new List<Device>();
            for (int index = 1; index <= count; index++)
            {
                Guid guid = Guid.NewGuid();
                devices.Add(new Device()
                {
                    DeviceId = guid,
                    GatewayId = guid,
                    ParentId = guid,
                    ProductId = Guid.Parse(UsrDevcieProductId),
                    IsGateway = true,
                    IsIndependentOnline = true,
                    IsOnLine = false,
                    MacAddress = $"CED4{index:D4}0000",
                });
            }
            var ret = await DeviceService.AddDeviceAsync(devices);
            return ret ? devices : null;
        }

        public static async Task<List<Device>> CreateAmmeter(Guid gatewayId, string gatewayMacAddress, int count)
        {
            // Device deviceTemplate = DeviceService.GetDeviceTemplate(AmmeterProductId).Result;

            List<Device> devices = new List<Device>();
            for (int index = 1; index <= count; index++)
            {
                Device device = await DeviceService.GetDeviceTemplate(AmmeterProductId);
                device.DeviceId = Guid.NewGuid();
                device.DeviceNameplates[0].DataValue = "20";
                device.DeviceProperties.Where(p => p.DeviceFieldName == "功率").FirstOrDefault().DataValue = "120";
                device.DeviceProperties.Where(p => p.DeviceFieldName == "电量").FirstOrDefault().DataValue = "0";
                device.MacAddress = $"{ gatewayMacAddress.Substring(0, 8)}{index:D4}";
                device.GatewayId = gatewayId;
                device.IsGateway = false;
                device.IsIndependentOnline = false;
                device.IsOnLine = false;
                device.ParentId = gatewayId;


                //Device ammeter = new Device()
                //{
                //    DeviceId = Guid.NewGuid(),
                //    GatewayId = gatewayId,
                //    IsGateway = false,
                //    IsIndependentOnline = false,
                //    IsOnLine = true,
                //    ParentId = gatewayId,
                //    ProductId = Guid.Parse(AmmeterProductId),
                //    MacAddress = $"{ gatewayMacAddress.Substring(0, 8)}{index:D4}",
                //    DeviceNameplates = new List<DeviceField>(),
                //    DeviceProperties = new List<DeviceField>()
                //};

                //DeviceField nameplate = deviceTemplate.DeviceNameplates[0].Clone();
                //nameplate.DataValue = "20";
                //nameplate.DeviceFieldId = Guid.NewGuid();
                //nameplate.ProductFieldId = ammeter.DeviceId;
                //ammeter.DeviceNameplates.Add(nameplate);

                //DeviceField power = deviceTemplate.DeviceProperties
                //    .Where(p => p.DeviceFieldName == "功率").FirstOrDefault().Clone();
                //power.DataValue = "100";
                //power.DeviceFieldId = Guid.NewGuid();
                //power.ProductFieldId = ammeter.DeviceId;
                //ammeter.DeviceProperties.Add(power);

                //DeviceField energy = deviceTemplate.DeviceProperties
                //    .Where(p => p.DeviceFieldName == "电量").FirstOrDefault().Clone();
                //power.DataValue = "0";
                //power.DeviceFieldId = Guid.NewGuid();
                //power.ProductFieldId = ammeter.DeviceId;
                //ammeter.DeviceProperties.Add(energy);

                devices.Add(device);
            }

            var ret = await DeviceService.AddDeviceAsync(devices);
            return ret ? devices : null;
        }

        public static async Task<IEnumerable<Ammeter>> GetAmmetersAsync(string gatewayId)
        {
            IEnumerable<Device> devices = await DeviceService.SearchAsync(
                     d => d.ProductId.ToString() == AmmeterProductId
                     && (gatewayId == null || d.ParentId.ToString() == gatewayId));

            List<Ammeter> ammeters = new List<Ammeter>();
            foreach (var item in devices)
            {
                ammeters.Add(new Ammeter()
                {
                    AmmeterId = item.DeviceId.ToString(),
                    MacAddress = item.MacAddress,
                    Rate = int.Parse(item.DeviceNameplates.Find(d => d.DeviceFieldName == "变比").DataValue),
                    Energy = double.Parse(item.DeviceProperties.Find(d => d.DeviceFieldName == "电量").DataValue),
                    Power = double.Parse(item.DeviceProperties.Find(d => d.DeviceFieldName == "功率").DataValue),
                });
            }
            return ammeters;
        }
    }
    public class Ammeter
    {
        public string AmmeterId { get; set; }
        public string MacAddress { get; set; }
        public int Rate { get; set; }
        public double Energy { get; set; }
        public double Power { get; set; }
    }
}