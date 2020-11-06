using FrameworkCore.Metadata.ProductDefine;
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
        public Guid DeviceNameplatesId { get; set; }
        public Device DeviceNameplates { get; set; }

        public Guid DevicePropertiesId { get; set; }
        public Device DeviceProperties { get; set; }

        public Guid ProductFieldId { get; set; }
        public ProductField ProductField { get; set; }
        #endregion 关系

    }
}
