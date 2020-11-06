using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkCore.Database;
using FrameworkCore.Metadata.Database;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppService.Controllers
{
    /// <summary>
    /// 设备管理控制器，设备的注册注销、参数设置
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        // GET: api/<DeviceController>
        [HttpGet]
        public async Task<IEnumerable<Device>> GetAsync()
        {
            return await DeviceService.GetAllDeviceAsync();
        }

        // GET api/<DeviceController>/5
        [HttpGet("{DeviceId}")]
        public async Task<Device> GetAsync(string deviceId)
        {
            return await DeviceService.GetDeviceAsync(Guid.Parse(deviceId));
        }

        /// <summary>
        /// 添加新设备时，先向服务器获取设备资料模板，填写此模板后提交
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Device> GetTemplateAsync(string productId)
        {
            Product product = await ProductSevice.GetProductAsync(Guid.Parse(productId));

            return DeviceService.CreateDeviceTemplateFromProduct(product);

        }

        // POST api/<DeviceController>
        [HttpPost]
        public async Task<bool> AddAsync([FromBody] string value)
        {
            Device device = JsonConvert.DeserializeObject<Device>(value);
            return await DeviceService.AddDeviceAsync(device);
        }

        // PUT api/<DeviceController>/5
        [HttpPut("{DeviceId}")]
        public async Task<bool> UpdateAsync(string deviceId, [FromBody] string value)
        {
            Device device = JsonConvert.DeserializeObject<Device>(value);
            return await DeviceService.UpdateDeviceAsync(Guid.Parse(deviceId), device);
        }

        // DELETE api/<DeviceController>/5
        [HttpDelete("{DeviceId}")]
        public async Task<bool> DeleteAsync(string deviceId)
        {
            return await DeviceService.DeleteDeviceAsync(Guid.Parse(deviceId));
        }
    }
}
