using FrameworkCore.Metadata.ProductDefine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.Metadata.DeviceDefine
{
    public class Device
    {
        /// <summary>
        /// 设备唯一标识码
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// 设备的网关
        /// </summary>
        public Guid GatewayId { get; set; }

        /// <summary>
        /// 设备通讯链路上直接上级节点
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// 设备的MacAddress
        /// </summary>
        public string MacAddress { get; set; }

        /// <summary>
        /// 设备是不是网关
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
        /// 设备是否在线
        /// IsGetway || IsIndependentOnline时,设备在线状态 = 网关在线状态 && IsOnLine
        /// 否则 设备在线状态 = 网关在线状态
        /// </summary>
        public bool IsOnLine { get; set; }

        public List<DeviceField> DeviceNameplates { get; set; }

        public List<DeviceField> DeviceProperties { get; set; }

        #region 关系
        ///// <summary>
        ///// 设备的产品定义
        ///// </summary>
        //[JsonIgnore]
        public Guid ProductId { get; set; }
        //[JsonIgnore]
        //public Product Product { get; set; }
        #endregion 关系
    }
}