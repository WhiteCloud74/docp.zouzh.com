using FrameworkCore.Redis;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        [HttpPut("InitDatabase")]
        public bool InitDatabase()
        {
            var ret1 = DatabaseService.InitDatabase();
            var ret2 = Task.Run(async () => await RedisService.InitDataAsync()).Result;
            return ret1 && ret2;
        }
    }
}
