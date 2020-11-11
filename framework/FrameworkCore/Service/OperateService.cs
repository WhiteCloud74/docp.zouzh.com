using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.Service
{
    public class OperateService
    {
        readonly static Guid applicateServerId;
        static readonly ConcurrentDictionary<string, MyWaiter<RedisCommand>> _cache;
        static OperateService()
        {
            applicateServerId = Guid.NewGuid();
            _cache = new ConcurrentDictionary<string, MyWaiter<RedisCommand>>();

            RedisService.ApplicateServerSubscribeCommandAsync(applicateServerId, OnReceiveRedisCommand).Wait();
        }

        public static async Task<RedisCommand> GetPropertiesAsync(string deviceId, List<KeyValuePair<string, string>> properties)
        {
            RedisCommand command = new RedisCommand()
            {
                CommandId = Guid.NewGuid().ToString(),
                CommandType = CommandType.GetProperties,

                DeviceId = deviceId,
                GatewayId = await RedisService.GetGatewayIdAsync(deviceId),
                ApplicateServerId = applicateServerId.ToString(),

                Request = properties,
                Expand = new List<KeyValuePair<string, string>>(),
            };

            return await ProcessRedisCommandAsync(command);
        }

        public static async Task<RedisCommand> SetPropertiesAsync(string deviceId, List<KeyValuePair<string, string>> properties)
        {
            RedisCommand command = new RedisCommand()
            {
                CommandId = Guid.NewGuid().ToString(),
                CommandType = CommandType.SetProperties,

                DeviceId = deviceId,
                GatewayId = await RedisService.GetGatewayIdAsync(deviceId),
                ApplicateServerId = applicateServerId.ToString(),

                Request = properties,
                Expand = new List<KeyValuePair<string, string>>(),
            };

            return await ProcessRedisCommandAsync(command);
        }

        public static async Task<RedisCommand> CallFunctionAsync(string deviceId, string function, List<KeyValuePair<string, string>> inputs)
        {
            RedisCommand command = new RedisCommand()
            {
                CommandId = Guid.NewGuid().ToString(),
                CommandType = CommandType.Function,

                DeviceId = deviceId,
                GatewayId = await RedisService.GetGatewayIdAsync(deviceId),
                ApplicateServerId = applicateServerId.ToString(),

                Request = inputs,
                Expand = new List<KeyValuePair<string, string>>(),

                Function = function
            };

            return await ProcessRedisCommandAsync(command);
        }

        async static Task<RedisCommand> ProcessRedisCommandAsync(RedisCommand command)
        {
            RedisCommand result = null;
            var ret = await RedisService.ApplicateServerPublishCommandAsync(command);
            if (ret > 0)
            {
                var awaiter = new MyWaiter<RedisCommand>();
                await _cache.AddOrUpdate(command.CommandId, awaiter, (g, c) => c);
                result = await awaiter;
            }
            return result;
        }

        static void OnReceiveRedisCommand(RedisCommand command)
        {
            if (_cache.TryRemove(command.CommandId, out MyWaiter<RedisCommand> myWaiter))
            {
                myWaiter.SetResult(command);
            }
        }
    }
}