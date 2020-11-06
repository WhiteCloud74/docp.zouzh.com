using FrameworkCore.Database;
using FrameworkCore.Metadata.DeviceDefine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace FrameworkCore.Metadata.ProductDefine
{
    /// <summary>
    /// 产品定义需要下列表
    /// 一、产品表 ProductBasic
    /// 二、功能表 ProductFunctions
    /// 三、事件表 ProductEvents
    /// 四、成员表 Field
    /// </summary>
    public class Product
    {
        /// <summary>
        /// 产品唯一标识码
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; } = "";

        /// <summary>
        /// 产品是不是网关
        /// </summary>
        public bool IsGateway { get; set; }

        /// <summary>
        /// IsGateway=true时，此属性没有意义
        /// 产品的在线状态是否独立，即是否与网关在线状态一致
        /// true： 有自己的在线状态
        /// false：与网关在线状态一致
        /// </summary>
        public bool IsIndependentOnline { get; set; }

        /// <summary>
        /// 与所有设备相关的信息，如产品厂商、联系电话等，一旦设定不能轻易修改
        /// Field.ParentGuid=ProductBasic.ProductBrandSetGuid
        /// </summary>
        public List<ProductField> ProductBrands { get; set; }

        /// <summary>
        /// 与某个设备相关的信息，如编号、出厂日期、额定功率等，一旦设定不能轻易修改
        /// Field.ParentGuid=ProductBasic.ProductNameplateSetGuid
        /// </summary>
        public List<ProductField> ProductNameplates { get; set; }

        /// <summary>
        /// 设备的属性数据，用以描述设备工作状态。
        /// 属性数据一般是不断变化的，如温湿度、电压、电流等
        /// 属性数据全部可读，部分可写
        /// Field.ParentGuid=ProductBasic.ProductPropertySetGuid
        /// </summary>
        public List<ProductField> ProductProperties { get; set; }

        /// <summary>
        /// 设备运行过程中，主动对外发布的数据，如各种报警
        /// </summary>
        public List<ProductEvent> ProductEvents { get; set; }

        /// <summary>
        /// 设备提供的操作，以供用户改变设备的状态，如重启、停止、暂停等
        /// </summary>
        public List<ProductFunction> ProductFunctions { get; set; }

        /// <summary>
        /// 该产品有哪些设备
        /// </summary>
        [NotMapped]
        public List<Device> Devices { get; set; }
    }
}