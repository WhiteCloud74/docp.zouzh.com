using FrameworkCore.Metadata.Database;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkCore.Service
{
    public class ProductSevice
    {
        public async static Task<Product> GetProductAsync(Guid productId)
        {
            return await GetAllProduct().FirstOrDefaultAsync(f => f.ProductId == productId);
        }

        public async static Task<IEnumerable<Product>> GetAllProductAsync()
        {
            return await GetAllProduct().ToListAsync();
        }

        private static IIncludableQueryable<Product, MyDataType> GetAllProduct()
        {
            return DbServiceProvider.ModelDbContext.Products
                .Include(p => p.ProductBrands).ThenInclude(d => d.MyDataType)
                .Include(p => p.ProductNameplates).ThenInclude(d => d.MyDataType)
                .Include(p => p.ProductProperties).ThenInclude(d => d.MyDataType)
                .Include(p => p.ProductEvents).ThenInclude(e => e.ProductEventProperties).ThenInclude(d => d.MyDataType)
                .Include(p => p.ProductFunctions).ThenInclude(f => f.ProductFunctionInputs).ThenInclude(d => d.MyDataType)
                .Include(p => p.ProductFunctions).ThenInclude(f => f.ProductFunctionOutputs).ThenInclude(d => d.MyDataType);
        }

        public async static Task<bool> AddProductAsync(Product product)
        {
            try
            {
                using var modelDbContext = DbServiceProvider.ModelDbContext;
                modelDbContext.Add(product);
                await modelDbContext.SaveChangesAsync();
                await RedisService.AddProductAsync(product);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async static Task<int> UpdateProductAsync(Guid productId, Product product)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            //var CurrentProduct = modelDbContext.Products.FindAsync(productId);
            product.ProductId = productId;

            modelDbContext.Update(product);
            return await modelDbContext.SaveChangesAsync();
        }

        public async static Task<int> DeleteProductAsync(Guid productId)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            var product = modelDbContext.Products.FindAsync(productId);
            modelDbContext.Products.Remove(product.Result);
            return await modelDbContext.SaveChangesAsync();
        }
    }
}