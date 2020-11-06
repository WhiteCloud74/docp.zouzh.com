using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FrameworkCore.Metadata.ProductDefine
{
    public class ProductFunction
    {
        public Guid ProductFunctionId { get; set; }

        public String ProductFunctionName { get; set; }

        public List<ProductField> ProductFunctionInputs { get; set; }

        public List<ProductField> ProductFunctionOutputs { get; set; }

        #region 关系
        public Product Product { get; set; }
        #endregion 关系
    }
}