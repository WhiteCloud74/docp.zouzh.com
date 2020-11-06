using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppService.Controllers
{
    /// <summary>
    /// 设备操作控制器，读取设置属性、调用设备操作
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OperateController : ControllerBase
    {
        [HttpGet]
        public async Task<List<KeyValuePair<string, string>>> GetPropertiesAsync(string deviceId, [FromBody] string value)
        {
            List<KeyValuePair<string, string>> properties = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value);
            var ret = await OperateService.GetPropertiesAsync(deviceId, properties);
            return ret.Response;
        }

        // POST api/<ProductController>
        [HttpPost]
        public async Task<List<KeyValuePair<string, string>>> SetPropertiesAsync(string deviceId, [FromBody] string value)
        {
            List<KeyValuePair<string, string>> properties = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value);
            var ret = await OperateService.SetPropertiesAsync(deviceId, properties);
            return ret.Response;
        }

        [HttpPost]
        public async Task<List<KeyValuePair<string, string>>> CallFunctionAsync(string deviceId, string function, [FromBody] string value)
        {
            List<KeyValuePair<string, string>> inputs = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value);
            var ret = await OperateService.CallFunctionAsync(deviceId, function, inputs);
            return ret.Response;
        }
    }
}