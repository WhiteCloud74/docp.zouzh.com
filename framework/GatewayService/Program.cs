using FrameworkCore.MyLogger;
using GatewayService.SocketAdapter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService
{
    public class Program1
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((loggingBuilding) => {
                    loggingBuilding.AddProvider(new MyLoggerProvider(new MyLoggerSettings(configuration)));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHostedService<Worker>();
                    services.AddHostedService<TcpSocketAdapterStart>();
                    //services.AddHostedService<TcpClientDaemonManager>();
                    //services.AddDbContextPool<ProductDbContext>(
                    //    options => options.UseMySql(configuration.GetConnectionString("Mysql")));
                });

        }
    }
}
