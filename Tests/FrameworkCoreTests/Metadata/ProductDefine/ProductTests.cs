using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrameworkCore.Metadata.ProductDefine;
using System;
using System.Collections.Generic;
using System.Text;
using FrameworkCore.Database;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FrameworkCore.Metadata.DataTypes;
using System.Threading.Tasks;
using FrameworkCore.Metadata.Database;
using FrameworkCore.Service;
using System.Threading;
using FrameworkCore.Instrument;
using AppClient;

namespace FrameworkCore.Metadata.ProductDefine.Tests
{
    [TestClass()]
    public class ProductTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                DbServiceProvider.Initialize();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //using var dataContext = new ProductDbContext();
            //dataContext.Database.EnsureDeleted();
        }

        [TestMethod()]
        public async Task AddOrUpdateToDatabaseTestAsync()
        {
            Product product = TestStub.CreateGatewayProduct();
           
            var ret = await ProductSevice.AddProductAsync(product);
            //using var dataContext = new ProductDbContext();
            //dataContext.Products.Add(product);
            //dataContext.SaveChanges();

            var ret2 = await ProductSevice.GetAllProductAsync();
        }

        [TestMethod()]
        public async void TestASync()
        {
            Console.WriteLine($"thread id: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"count: {await Dosome(2000)}");
            Console.WriteLine($"thread id: {Thread.CurrentThread.ManagedThreadId}");
        }

        public async Task<int> Dosome(int count)
        {
            await Task.Delay(count);
            return 2;
        }
    }
}