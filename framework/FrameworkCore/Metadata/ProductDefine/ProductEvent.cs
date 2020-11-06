using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FrameworkCore.Metadata.ProductDefine
{
    public class ProductEvent
    {
        public Guid ProductEventId { get; set; }

        public String ProductEventName { get; set; }

        public List<ProductField> ProductEventProperties { get; set; }

        #region 关系
        public Product Product { get; set; }
        #endregion 关系
    }
}