using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.MyLogger
{
    public static class MyLoggerExtensions
    {
        //add 日志文件创建规则，分割规则，格式化规则，过滤规则 to appsettings.json
        public static ILoggerFactory AddFile(this ILoggerFactory factory, IConfiguration configuration)
        {
            return AddFile(factory, new MyLoggerSettings(configuration));
        }
        public static ILoggerFactory AddFile(this ILoggerFactory factory, MyLoggerSettings myLoggerSettings)
        {
            factory.AddProvider(new MyLoggerProvider(myLoggerSettings));
            return factory;
        }
    }
}
