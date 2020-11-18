using Microsoft.VisualStudio.TestTools.UnitTesting;
using FrameworkCore.Database;
using System;
using System.Collections.Generic;
using System.Text;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.Database;
using FrameworkCore.Service;

namespace FrameworkCore.Database.Tests
{
    [TestClass()]
    public class ProductSeviceTests
    {
        [TestInitialize]
        public void Initialize()
        {
            try
            {
                DatabaseService.InitDatabase();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod()]
        public void GetProductAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllProductAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateProductAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddProductAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteProductAsyncTest()
        {
            Assert.Fail();
        }
    }
}