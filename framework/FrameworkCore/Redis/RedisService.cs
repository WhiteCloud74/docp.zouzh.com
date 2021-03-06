﻿using FrameworkCore.Instrument;
using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using FrameworkCore.Service;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.Redis
{
    public delegate void OnReceivedRedisCommand(RedisCommand command);
    public delegate Task OnReceivedRedisCommandAsync(RedisCommand command);

    public static class RedisService
    {
        public const string CommonApplicateServer = "42144c28-8537-45b6-9796-b313d08f0ad8";
        private static readonly ConnectionMultiplexer _redis;
        private static readonly int _product = 0;
        private static readonly int _device = 1;
        private static readonly int _topology = 2;

        static RedisService()
        {
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();
            string redisConnectString = configuration.GetConnectionString("Redis");

            _redis = ConnectionMultiplexer.Connect(redisConnectString);
        }

        public static async Task<bool> InitDataAsync()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                server.FlushDatabase(_product);
                server.FlushDatabase(_device);
                server.FlushDatabase(_topology);

                var products = await ProductSevice.GetAllProductAsync();
                foreach (var product in products)
                {
                    await AddProductAsync(product);
                }

                var devices = await DeviceService.GetAllDeviceAsync();
                foreach (var device in devices)
                {
                    await AddDeviceAsync(device);
                }

                return true;
            }
            catch
            {
                return false;
            }
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

        public static async Task GatewayServerSubscribeCommandAsync(string gatewayId, OnReceivedRedisCommandAsync action)
        {
            await _redis.GetSubscriber().SubscribeAsync(
                new RedisChannel(gatewayId, RedisChannel.PatternMode.Auto),
                new Action<RedisChannel, RedisValue>(async (c, v) =>
                {
                    RedisCommand command = JsonConvert.DeserializeObject<RedisCommand>(v.ToString());
                    await action(command);
                }));
        }

        public static async Task ApplicateServerSubscribeCommandAsync(Guid applicateServerID, OnReceivedRedisCommand action)
        {
            await _redis.GetSubscriber().SubscribeAsync(
                new RedisChannel(applicateServerID.ToString(), RedisChannel.PatternMode.Auto),
                new Action<RedisChannel, RedisValue>((c, v) =>
                {
                    RedisCommand command = JsonConvert.DeserializeObject<RedisCommand>(v.ToString());
                    action(command);
                }));
        }

        public static async Task UnsubscribeAsync(string channel)
        {
            await _redis.GetSubscriber().UnsubscribeAsync(
                new RedisChannel(channel,
                RedisChannel.PatternMode.Auto));
        }

        public static async Task UnsubscribeAllAsync()
        {
            await _redis.GetSubscriber().UnsubscribeAllAsync();
        }

        #endregion 发布订阅退订

        #region 数据操作
        public static async Task AddProductAsync(Product product)
        {
            await _redis.GetDatabase(_product).StringSetAsync(
                new RedisKey(product.ProductId.ToString()),
                new RedisValue(JsonConvert.SerializeObject(product)));
        }

        public static async Task AddDeviceAsync(IEnumerable<Device> devices)
        {
            foreach (var device in devices)
            {
                await AddDeviceAsync(device);
            }
        }

        public static async Task AddDeviceAsync(Device device)
        {
            await _redis.GetDatabase(_device).HashSetAsync(
                 new RedisKey(device.DeviceId.ToString()),
                 new HashEntry[] {
                        new HashEntry("DeviceId",device.DeviceId.ToString()),
                        new HashEntry("GatewayId",device.GatewayId.ToString()),
                        new HashEntry("IsGateway",device.IsGateway.ToString()),
                        new HashEntry("IsOnLine",device.IsOnLine.ToString()),
                        new HashEntry("MacAddress",device.MacAddress.ToString()),
                        new HashEntry("ParentId",device.ParentId.ToString()),
                        new HashEntry("ProductId",device.ProductId.ToString()),
                        new HashEntry("IsIndependentOnline",device.IsIndependentOnline.ToString())
                 });

            if (!device.IsGateway)
            {
                await _redis.GetDatabase(_topology).ListRightPushAsync(new RedisKey(device.GatewayId.ToString()),
                    new RedisValue(device.DeviceId.ToString()));
            }
        }

        public static async Task<string[]> GetPropertiesAsync(string deviceId, string[] properties)
        {
            List<RedisValue> redisValues = new List<RedisValue>();
            foreach (var item in properties)
            {
                redisValues.Add(new RedisValue(item));
            }
            var ret = await _redis.GetDatabase(_device).HashGetAsync(deviceId, redisValues.ToArray());
            var response = new List<string>();
            foreach (var item in ret)
            {
                response.Add(item.ToString());
            }
            return response.ToArray();
        }

        //public static async Task<string> GetGatewayIdAsync(string deviceId)
        //{
        //    return await _redis.GetDatabase(_device).HashGetAsync(deviceId, "GatewayId");
        //}

        public static async Task GatewayOnOffLineAsync(string gatewayId, bool isOnline)
        {
            await _redis.GetDatabase(_device).HashSetAsync(gatewayId, "IsOnLine", isOnline);
            if (isOnline)
            {
                foreach (var device in await _redis.GetDatabase(_topology).ListRangeAsync(gatewayId))
                {
                    var isIndependentOnline = await _redis.GetDatabase(_device).HashGetAsync(device.ToString(), "IsIndependentOnline");
                    if (!(bool)isIndependentOnline)
                    {
                        await _redis.GetDatabase(_device).HashSetAsync(device.ToString(), "IsOnline", true);
                    }
                }
            }
            else
            {
                foreach (var device in await _redis.GetDatabase(_topology).ListRangeAsync(gatewayId))
                {
                    await _redis.GetDatabase(_device).HashSetAsync(device.ToString(), "IsOnline", false);
                }
            }
        }
        #endregion 数据操作
    }
}