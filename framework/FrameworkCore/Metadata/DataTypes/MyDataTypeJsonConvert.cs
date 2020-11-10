using FrameworkCore.Instrument;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FrameworkCore.Metadata.DataTypes
{
    public class MyDataTypeJsonConvert : JsonConverter<MyDataType>
    {
        public static readonly MyDataTypeJsonConvert Instance;
        static readonly Dictionary<string, Type> _dataTypes;
        static MyDataTypeJsonConvert()
        {
            // _dataTypes = CommonFunction.GetAllUnabstractTypeAndInheritFromBaseTypeAsync(typeof(MyDataType)).Result;
            _dataTypes = new Dictionary<string, Type>
            {
                { "StringType", typeof(StringType) },
                { "IntType", typeof(IntType) }
            };
            Instance = new MyDataTypeJsonConvert();
        }
        private MyDataTypeJsonConvert()
        {
        }

        public override MyDataType ReadJson(JsonReader reader, Type objectType, MyDataType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            MyDataType target = null;
            if (jsonObject.TryGetValue("TypeName", StringComparison.OrdinalIgnoreCase, out JToken gender))
            {
                target = Activator.CreateInstance(_dataTypes[gender.ToString()]) as MyDataType;
            }
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, MyDataType value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
