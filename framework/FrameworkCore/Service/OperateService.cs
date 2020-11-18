using FrameworkCore.Instrument;
using FrameworkCore.Redis;
using StackExchange.Redis;
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
            RedisCommand command = await CreateRedisCommand(deviceId, properties);
            command.CommandType = CommandType.GetProperties;

            return await ProcessRedisCommandAsync(command);
        }

        public static async Task<RedisCommand> SetPropertiesAsync(string deviceId, List<KeyValuePair<string, string>> properties)
        {
            RedisCommand command = await CreateRedisCommand(deviceId, properties);
            command.CommandType = CommandType.SetProperties;

            return await ProcessRedisCommandAsync(command);
        }

        public static async Task<RedisCommand> CallFunctionAsync(string deviceId, string function, List<KeyValuePair<string, string>> inputs)
        {
            RedisCommand command = await CreateRedisCommand(deviceId, inputs);
            command.CommandType = CommandType.Function;
            command.Function = function;

            return await ProcessRedisCommandAsync(command);
        }

        static async Task<RedisCommand> CreateRedisCommand(string deviceId, List<KeyValuePair<string, string>> properties)
        {
            var redisValues = await RedisService.GetPropertiesAsync(deviceId, new string[] { "GatewayId", "MacAddress" });
            return new RedisCommand()
            {
                CommandId = Guid.NewGuid().ToString(),

                DeviceId = deviceId,
                GatewayId = redisValues[0],
                MacAddress = redisValues[1],

                ApplicateServerId = applicateServerId.ToString(),

                Request = properties,
                Expand = new List<KeyValuePair<string, string>>(),
            };
        }

        static async Task<RedisCommand> ProcessRedisCommandAsync(RedisCommand command)
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