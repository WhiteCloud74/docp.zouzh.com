using FrameworkCore.Metadata.Database;
using FrameworkCore.Metadata.ProductDefine;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameworkCore.Service
{
    public class ProductSevice
    {
        public async static Task<Product> GetProductAsync(Guid productId)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            return await modelDbContext.Products.FindAsync(productId);
        }

        public async static Task<IEnumerable<Product>> GetAllProductAsync()
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            return await modelDbContext.Products.ToListAsync();
        }

        public async static Task<int> AddProductAsync(Product product)
        {
            using var modelDbContext = DbServiceProvider.ModelDbContext;
            modelDbContext.Add(product);
            return await modelDbContext.SaveChangesAsync();
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