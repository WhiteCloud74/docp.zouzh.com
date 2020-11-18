
using FrameworkCore.MyLogger;
using GatewayService.SocketAdapter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FrameworkCore.Metadata.Database;

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
                //.ConfigureLogging((loggingBuilding) => {
                //    loggingBuilding.AddProvider(new MyLoggerProvider(new MyLoggerSettings(configuration)));
                //})
                .ConfigureServices((hostContext, services) =>
                {

                    //services.AddHostedService<Worker>();
                    services.AddHostedService<GatewayServiceStart>();
                    //services.AddHostedService<TcpClientDaemonManager>();
                    //services.AddDbContextPool<ModelDbContext>(
                    //    options => options.UseMySql(configuration.GetConnectionString("Mysql")), poolSize: 100);
                });

        }
    }
}



/*
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public interface IFooService { void DoThing(int number); }
public interface IBarService { void DoSomeRealWork(); }
public class BarService : IBarService
{
    private readonly IFooService _fooService;
    public BarService(IFooService fooService) { _fooService = fooService; }
    public void DoSomeRealWork() { for (int i = 0; i < 10; i++) { _fooService.DoThing(i); } }
}
public class FooService : IFooService
{
    private readonly ILogger<FooService> _logger;
    public FooService(ILoggerFactory loggerFactory) { _logger = loggerFactory.CreateLogger<FooService>(); }
    public void DoThing(int number) { _logger.LogInformation($"Doing the thing {number}"); }
}

public class Program
{
    public static void Main(string[] args)
    {    //setup our DI  
        var serviceProvider = new ServiceCollection()
            .AddLogging().AddSingleton<IFooService, FooService>()
            .AddSingleton<IBarService, BarService>()
            .BuildServiceProvider();
        //configure console logging    serviceProvider    .GetService<ILoggerFactory>()    .AddConsole(LogLevel.Debug);  
        var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
        logger.LogInformation("Starting application");
        //do the actual work here   
        var bar = serviceProvider.GetService<IBarService>();
        bar.DoSomeRealWork();
        logger.LogInformation("All done!");
    }
}
*/