using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppService.Controllers
{
    /// <summary>
    /// 产品的定义、注册、注销等
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        // GET: api/<ProductController>
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Product>> GetAsync()
        {
            var ret = await ProductSevice.GetAllProductAsync();
            return ret;
        }

        // GET api/<ProductController>/5
        [HttpGet("GetOne/{productId}")]
        public async Task<Product> GetAsync(string productId)
        {
            return await ProductSevice.GetProductAsync(Guid.Parse(productId));
        }

        // POST api/<ProductController>
        [HttpPost("AddOne")]
        public async Task<bool> AddAsync([FromBody] object value)//()//
        {
            try
            {
                Product product = JsonConvert.DeserializeObject<Product>(value.ToString(), MyDataTypeJsonConvert.Instance) as Product;
                return await ProductSevice.AddProductAsync(product);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut("Update/{productId}")]
        public async Task<bool> UpdateAsync(string productId, [FromBody] object value)
        {
            Product product = JsonConvert.DeserializeObject<Product>(value.ToString(), MyDataTypeJsonConvert.Instance) as Product;
            return await ProductSevice.UpdateProductAsync(Guid.Parse(productId), product) == 1;
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("Delete/{productId}")]
        public async Task<bool> DeleteAsync(string productId)
        {
            return await ProductSevice.DeleteProductAsync(Guid.Parse(productId)) == 1;
        }
    }
}
