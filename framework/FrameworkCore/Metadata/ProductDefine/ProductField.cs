using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public MyDataType MyDataType { get; set; }

        /// <summary>
        /// Field的值
        /// </summary>
        public string DataValue { get; set; }

        #region 关系
        //[JsonIgnore]
        //public Guid ProductBrandsId { get; set; }
        [JsonIgnore]
        public Product ProductBrands { get; set; }

        //[JsonIgnore]
        //public Guid ProductNameplatesId { get; set; }
        [JsonIgnore]
        public Product ProductNameplates { get; set; }

        //[JsonIgnore]
        //public Guid ProductPropertiesId { get; set; }
        [JsonIgnore]
        public Product ProductProperties { get; set; }

        //[JsonIgnore]
        //public Guid ProductFunctionInputsId { get; set; }
        [JsonIgnore]
        public ProductFunction ProductFunctionInputs { get; set; }

        //[JsonIgnore]
        //public Guid ProductFunctionOutputsId { get; set; }
        [JsonIgnore]
        public ProductFunction ProductFunctionOutputs { get; set; }

        //[JsonIgnore]
        //public Guid ProductEventId { get; set; }
        [JsonIgnore]
        public ProductEvent ProductEvent { get; set; }
        #endregion 关系

        //[NotMapped]
        //[JsonIgnore]
        //public List<DeviceField> DeviceFields { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ProductField f))
            {
                return false;
            }

            return ProductFieldId == f.ProductFieldId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}