using FrameworkCore.Metadata.ProductDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FrameworkCore.Metadata.DeviceDefine
{
    public class DeviceField
    {
        public Guid DeviceFieldId { get; set; }

        public string DeviceFieldName { get; set; }

        public string DataValue { get; set; }

        /// <summary>
        /// 表示该数据是产品的哪项数据
        /// </summary>

        #region 关系
        //[JsonIgnore]
        public Guid ProductFieldId { get; set; }
        //[JsonIgnore]
        //public ProductField ProductField { get; set; }

        //[JsonIgnore]
        //public Guid DeviceNameplatesId { get; set; }
        [JsonIgnore]
        public Device DeviceNameplates { get; set; }

        //[JsonIgnore]
        // public Guid DevicePropertiesId { get; set; }
        [JsonIgnore]
        public Device DeviceProperties { get; set; }
        #endregion 关系

    }
}
