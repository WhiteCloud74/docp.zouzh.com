using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FrameworkCore.Metadata.ProductDefine
{
    public class ProductFunction
    {
        public Guid ProductFunctionId { get; set; }

        public String ProductFunctionName { get; set; }

        public List<ProductField> ProductFunctionInputs { get; set; }

        public List<ProductField> ProductFunctionOutputs { get; set; }

        #region 关系
        //[JsonIgnore]
        //public Guid ProductId { get; set; }
        //[JsonIgnore]
        public Product Product { get; set; }
        #endregion 关系
    }
}