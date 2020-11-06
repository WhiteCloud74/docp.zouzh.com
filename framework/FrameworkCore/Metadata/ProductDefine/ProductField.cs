using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DataType = FrameworkCore.Metadata.DataTypes.DataType;

namespace FrameworkCore.Metadata.ProductDefine
{
    public class ProductField
    {
        public Guid ProductFieldId { get; set; }

        public string ProductFieldName { get; set; }

        /// <summary>
        /// 是否可以修改
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// Field的值类型
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Field的值
        /// </summary>
        public string DataValue { get; set; }

        #region 关系
        public Product ProductBrands { get; set; }
        public Product ProductNameplates { get; set; }
        public Product ProductProperties { get; set; }
        public ProductFunction ProductFunctionInputs { get; set; }
        public ProductFunction ProductFunctionOutputs { get; set; }
        public ProductEvent ProductEvent { get; set; }
        #endregion 关系

        [NotMapped]
        public List<DeviceField> DeviceFields { get; set; }

        public override bool Equals(object obj)
        {
            return ProductFieldId == ((ProductField)obj).ProductFieldId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}