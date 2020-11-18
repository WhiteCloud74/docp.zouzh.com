using FrameworkCore.Metadata.Database;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Redis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FrameworkCore.Service
{
    public class DeviceService
    {
        public static async Task<IEnumerable<Device>> SearchAsync(Expression<Func<Device, bool>> expression)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            return await modelDbContext.Devices
                .Where(expression)
                .Include(d => d.DeviceNameplates)
                .Include(d => d.DeviceProperties)
                .ToListAsync();
        }

        public static async Task<Device> GetDeviceAsync(Guid deviceId)
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

        public static async Task<bool> AddDeviceAsync(IEnumerable<Device> devices)
        {
            try
            {
                using var modelDbContext = DbServiceProvider.ModelDbContext;
                modelDbContext.AddRange(devices);
                await modelDbContext.SaveChangesAsync();
                await RedisService.AddDeviceAsync(devices);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static async Task<bool> AddDeviceAsync(Device device)
        {
            try
            {
                using var modelDbContext = DbServiceProvider.ModelDbContext;
                modelDbContext.Add(device);
                await modelDbContext.SaveChangesAsync();
                await RedisService.AddDeviceAsync(device);
                return true;
            }
            catch (Exception)
            {
                return false;
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

        public static async Task<Device> GetDeviceTemplate(string productId)
        {
            Product product = await ProductSevice.GetProductAsync(Guid.Parse(productId));

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
