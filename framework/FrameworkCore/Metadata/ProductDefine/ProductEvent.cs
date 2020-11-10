using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FrameworkCore.Metadata.ProductDefine
{
    public class ProductEvent
    {
        public Guid ProductEventId { get; set; }

        public String ProductEventName { get; set; }

        public List<ProductField> ProductEventProperties { get; set; }

        #region 关系
        //[JsonIgnore]
        //public Guid ProductId { get; set; }
        //[JsonIgnore]
        public Product Product { get; set; }
        #endregion 关系
    }
}