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
            return DatabaseService.InitDatabase() 
                && RedisService.InitDataAsync().Result;
        }
    }
}
