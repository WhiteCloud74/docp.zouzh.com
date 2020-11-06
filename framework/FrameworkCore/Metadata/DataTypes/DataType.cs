using FrameworkCore.Instrument;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.Metadata.DataTypes
{
    public abstract class DataType
    {
        public Guid DataTypeId { get; set; }

        /// <summary>
        /// 类型名称，仅供界面显示用
        /// </summary>
        public string TypeName { get { return this.GetType().Name; } }

        /// <summary>
        /// 校验数据是否有效
        /// </summary>
        /// <returns></returns>
        public abstract bool IsValid(string dataValue);

        /// <summary>
        /// 初始化，取得系统定义的数据类型
        /// </summary>
        /// <returns></returns>  
        public static Dictionary<string, Type> DataTypes { get; private set; }
        public static async Task InitAsync()
        {
            DataTypes = await CommonFunction.GetAllUnabstractTypeAndInheritFromBaseTypeAsync(typeof(DataType));
        }
    }
}