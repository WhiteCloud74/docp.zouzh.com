using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppService.Controllers
{
    /// <summary>
    /// 设备管理控制器，设备的注册注销、参数设置
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        // GET: api/<DeviceController>
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Device>> GetAsync()
        {
            return await DeviceService.GetAllDeviceAsync();
        }

        // GET api/<DeviceController>/5
        [HttpGet("GetOne/{deviceId}")]
        public async Task<Device> GetAsync(string deviceId)
        {
            return await DeviceService.GetDeviceAsync(Guid.Parse(deviceId));
        }

        /// <summary>
        /// 添加新设备时，先向服务器获取设备资料模板，填写此模板后提交
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("GetTemplate/{productId}")]
        public async Task<Device> GetTemplateAsync(string productId)
        {
            return await DeviceService.GetDeviceTemplate(productId);
        }

        // POST api/<DeviceController>
        [HttpPost("AddOne")]
        public async Task<bool> AddAsync([FromBody] object value)
        {
            Device device = JsonConvert.DeserializeObject<Device>(value.ToString(), MyDataTypeJsonConvert.Instance);
            return await DeviceService.AddDeviceAsync(device);
        }

        // PUT api/<DeviceController>/5
        [HttpPut("Update/{deviceId}")]
        public async Task<bool> UpdateAsync(string deviceId, [FromBody] object value)
        {
            Device device = JsonConvert.DeserializeObject<Device>(value.ToString(), MyDataTypeJsonConvert.Instance);
            return await DeviceService.UpdateDeviceAsync(Guid.Parse(deviceId), device);
        }

        // DELETE api/<DeviceController>/5
        [HttpDelete("Delete/{deviceId}")]
        public async Task<bool> DeleteAsync(string deviceId)
        {
            return await DeviceService.DeleteDeviceAsync(Guid.Parse(deviceId));
        }
    }
}
