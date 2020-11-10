using FrameworkCore.Metadata.Database;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkCore.Service
{
    public class DeviceService
    {
        public async static Task<Device> GetDeviceAsync(Guid deviceId)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            return await modelDbContext.Devices
                .Include(d => d.DeviceNameplates)
                .Include(d => d.DeviceProperties)
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        }

        public static async Task<IEnumerable<Device>> GetAllDeviceAsync()
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            return await modelDbContext.Devices
                .Include(d => d.DeviceNameplates)
                .Include(d => d.DeviceProperties)
                .ToListAsync();
        }

        public static async Task<bool> AddDeviceAsync(Device device)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            modelDbContext.Add(device);
            try
            {
                var ret = await modelDbContext.SaveChangesAsync();
                return ret >= 1;

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public static async Task<bool> UpdateDeviceAsync(Guid deviceId, Device device)
        {
            device.DeviceId = deviceId;

            using var modelDbContext = DbServiceProvider.ModelDbContext;
            modelDbContext.Update(device);
            return await modelDbContext.SaveChangesAsync() == 1;
        }

        public static async Task<bool> DeleteDeviceAsync(Guid deviceId)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            var device = modelDbContext.Devices.FindAsync(deviceId);
            modelDbContext.Devices.Remove(device.Result);
            return await modelDbContext.SaveChangesAsync() == 1;
        }

        public static Device CreateDeviceTemplateFromProduct(Product product)
        {
            Device device = new Device()
            {
                DeviceId = Guid.NewGuid(),
                ProductId = product.ProductId,
                IsGateway = product.IsGateway,
                IsIndependentOnline = product.IsIndependentOnline,
                DeviceNameplates = new List<DeviceField>(),
                DeviceProperties = new List<DeviceField>(),
            };
            foreach (var item in product.ProductNameplates)
            {
                device.DeviceNameplates.Add(new DeviceField()
                {
                    DeviceFieldId = Guid.NewGuid(),
                    DeviceFieldName = item.ProductFieldName,
                    ProductFieldId = item.ProductFieldId,
                    DeviceNameplates = device
                });
            }
            foreach (var item in product.ProductProperties)
            {
                device.DeviceProperties.Add(new DeviceField()
                {
                    DeviceFieldId = Guid.NewGuid(),
                    DeviceFieldName = item.ProductFieldName,
                    ProductFieldId = item.ProductFieldId,
                    DeviceProperties = device
                });
            }

            return device;
        }
    }
}
