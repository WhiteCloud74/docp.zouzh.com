using FrameworkCore.Instrument;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.Redis
{
    public delegate void OnReceivedRedisCommand(RedisCommand command);

    public static class RedisService
    {
        private static readonly ConnectionMultiplexer _redis;
        //private static readonly int _productDefine = 0;
        private static readonly int _device = 1;
        static RedisService()
        {
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();
            string redisConnectString = configuration["redis:connectString"];

            _redis = ConnectionMultiplexer.Connect(redisConnectString);
        }

        #region 发布订阅退订
        public static async Task<long> GatewayServerPublishCommandAsync(RedisCommand command)
        {
            Debug.Assert(command.GatewayId != null, "操作服务ID不能为空");

            return await _redis.GetSubscriber().PublishAsync(
                new RedisChannel(command.ApplicateServerId.ToString(), RedisChannel.PatternMode.Auto),
                new RedisValue(JsonConvert.SerializeObject(command)));
        }

        public static async Task<long> ApplicateServerPublishCommandAsync(RedisCommand command)
        {
            Debug.Assert(command.GatewayId != null, "网关ID不能为空");

            return await _redis.GetSubscriber().PublishAsync(
                new RedisChannel(command.GatewayId.ToString(), RedisChannel.PatternMode.Auto),
                new RedisValue(JsonConvert.SerializeObject(command)));
        }

        public static async Task GatewayServerSubscribeCommandAsync(Guid gatewayID, OnReceivedRedisCommand action)
        {
            await _redis.GetSubscriber().SubscribeAsync(
                new RedisChannel(gatewayID.ToString(), RedisChannel.PatternMode.Auto),
                new Action<RedisChannel, RedisValue>((c, v) => { ProcessCommand(v, action); }));
        }

        public static async Task ApplicateServerSubscribeCommandAsync(Guid applicateServerID, OnReceivedRedisCommand action)
        {
            await _redis.GetSubscriber().SubscribeAsync(
                new RedisChannel(applicateServerID.ToString(), RedisChannel.PatternMode.Auto),
                new Action<RedisChannel, RedisValue>((c, v) => { ProcessCommand(v, action); }));
        }

        public static async Task UnsubscribeAsync(Guid channel)
        {
            await _redis.GetSubscriber().UnsubscribeAsync(
                new RedisChannel(channel.ToString(),
                RedisChannel.PatternMode.Auto));
        }

        public static async Task UnsubscribeAllAsync()
        {
            await _redis.GetSubscriber().UnsubscribeAllAsync();
        }

        private static void ProcessCommand(RedisValue v, OnReceivedRedisCommand action)
        {
            RedisCommand command = JsonConvert.DeserializeObject<RedisCommand>(v.ToString());
            action(command);
        }
        #endregion 发布订阅退订

        #region 数据缓存
        public static async Task<string> GetGatewayId(string deviceId)
        {
            return await _redis.GetDatabase(_device).HashGetAsync(deviceId, new RedisValue("GatewayId"));
        }
        #endregion 数据缓存
    }
}