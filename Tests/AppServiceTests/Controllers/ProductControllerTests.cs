using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppService.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using FrameworkCore.Instrument;
using Newtonsoft.Json;
using FrameworkCore.Service;

namespace AppService.Controllers.Tests
{
    [TestClass()]
    public class ProductControllerTests
    {
        [TestMethod()]
        public void GetAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAsyncTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddTest()
        {
            try
            {
                var product = TestStub.CreateTestProduct();
                //var data = JsonConvert.SerializeObject(product);
                //var controller = new ProductController();
                //var ret = controller.AddAsync(data).Result;
                var result = ProductSevice.AddProductAsync(product).Result;
                //Assert.IsTrue(ret);

            }
            catch (Exception e)
            {

                throw;
            }
        }

        [TestMethod()]
        public void ModifyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }
    }
}