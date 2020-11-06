using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

//#nullable enable
namespace FrameworkCore.Redis
{
    public class RedisCommand
    {
        public string CommandId { get; set; }

        public CommandType CommandType { get; set; }

        public string DeviceId { get; set; }

        public string GatewayId { get; set; }

        public string GateWayServerId { get; set; }

        public string ApplicateServerId { get; set; } = Guid.Empty.ToString();

        public List<KeyValuePair<string, string>> Request { get; set; }

        public List<KeyValuePair<string, string>> Response { get; set; }  //EventReport只填充这项

        public List<KeyValuePair<string, string>> Expand { get; set; }    //可选扩展项，由于调试、优化等

        public string Function { get; internal set; }                     //Function专用
    }

    public enum CommandType
    {
        GetProperties,
        SetProperties,
        Function,
        EventReport
    }
}