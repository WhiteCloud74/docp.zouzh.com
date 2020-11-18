using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace FrameworkCore.Metadata.Database
{
    internal static class DbServiceProvider
    {
        internal static ModelDbContext GetModelDbContext() { return new ModelDbContext(); }
        internal static ModelDbContext ModelDbContext { get { return Provider.GetService<ModelDbContext>(); } }

        static readonly ServiceProvider Provider;
        static readonly IServiceCollection container;
        static DbServiceProvider()
        {
            container = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            container.AddDbContextPool<ModelDbContext>(
                options => options.UseMySql(configuration.GetConnectionString("Mysql")), poolSize: 64);
            //container.AddDbContextPool<ModelDbContext>(
            //    options => options.UseInMemoryDatabase("Product"));
            Provider = container.BuildServiceProvider();
        }

        internal static bool Initialize()
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            bool s = modelDbContext.Database.EnsureDeleted();
            bool ret = modelDbContext.Database.EnsureCreated();
            return s && ret;
        }
    }
}
