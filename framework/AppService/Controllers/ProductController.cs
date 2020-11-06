using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkCore.Database;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppService.Controllers
{
    /// <summary>
    /// 产品的定义、注册、注销等
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        // GET: api/<ProductController>
        [HttpGet]
        public async Task<IEnumerable<Product>> GetAsync()
        {
            return await ProductSevice.GetAllProductAsync();
        }

        // GET api/<ProductController>/5
        [HttpGet("{productId}")]
        public async Task<Product> GetAsync(string productId)
        {
            return await ProductSevice.GetProductAsync(Guid.Parse(productId));
        }

        // POST api/<ProductController>
        [HttpPost]
        public async Task<bool> AddAsync([FromBody] string value)
        {
            Product product = JsonConvert.DeserializeObject<Product>(value) as Product;
            return await ProductSevice.AddProductAsync(product) == 1;
        }

        // PUT api/<ProductController>/5
        [HttpPut("{productId}")]
        public async Task<bool> UpdateAsync(string productId, [FromBody] string value)
        {
            Product product = JsonConvert.DeserializeObject<Product>(value) as Product;
            return await ProductSevice.UpdateProductAsync(Guid.Parse(productId),product) == 1;
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{productId}")]
        public async Task<bool> DeleteAsync(string productId)
        {
            return await ProductSevice.DeleteProductAsync(Guid.Parse(productId)) == 1;
        }
    }
}
