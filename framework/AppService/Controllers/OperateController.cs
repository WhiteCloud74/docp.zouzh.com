﻿using System;
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
    [Route("[controller]")]
    [ApiController]
    public class OperateController : ControllerBase
    {
        [HttpPost("GetProperties")]
        public async Task<List<KeyValuePair<string, string>>> GetPropertiesAsync([FromBody] object value)
        {
            List<KeyValuePair<string, string>> properties
                = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value.ToString());
            string deviceId = properties[0].Value;
            properties.RemoveAt(0);
            var ret = await OperateService.GetPropertiesAsync(deviceId, properties);
            return ret.Response;
        }

        // POST api/<ProductController>
        [HttpPost("SetProperties")]
        public async Task<List<KeyValuePair<string, string>>> SetPropertiesAsync([FromBody] object value)
        {
            List<KeyValuePair<string, string>> properties = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value.ToString());
            string deviceId = properties[0].Value;
            properties.RemoveAt(0);
            var ret = await OperateService.SetPropertiesAsync(deviceId, properties);
            return ret.Response;
        }

        [HttpPost("CallFunction")]
        public async Task<List<KeyValuePair<string, string>>> CallFunctionAsync(string function, [FromBody] object value)
        {
            List<KeyValuePair<string, string>> inputs = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(value.ToString());
            string deviceId = inputs[0].Value;
            inputs.RemoveAt(0);
            var ret = await OperateService.CallFunctionAsync(deviceId, function, inputs);
            return ret.Response;
        }
    }
}