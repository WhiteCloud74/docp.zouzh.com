using FrameworkCore.MyLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace GatewaySimulator
{
    public class Program
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
                    object p = loggingBuilding.AddProvider(new MyLoggerProvider(new MyLoggerSettings(configuration)));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddHostedService<Worker>();
                    services.AddHostedService<GatewaySimulatorStart>();
                    //services.AddHostedService<TcpClientDaemonManager>();
                    //services.AddDbContextPool<ProductDbContext>(
                    //    options => options.UseMySql(configuration.GetConnectionString("Mysql")));
                });

        }
    }
}
