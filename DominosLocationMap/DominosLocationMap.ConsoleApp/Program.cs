﻿using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Business.Managers;
using DominosLocationMap.Business.Queues.RabbitMq;
using DominosLocationMap.Business.Queues.Redis;
using DominosLocationMap.Core.CrossCutting.Caching;
using DominosLocationMap.Core.CrossCutting.Caching.Redis;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.DataAccess.Concrete.EntityFramework;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DominosLocationMap.ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("RabbitMq.ConsumerConsole Program.cs Acıldı.");

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("project.json");

            Console.WriteLine("project.json okundu.");
            var configuration = builder.Build();

            var redisOptions = configuration.GetSection("RedisOptions").Get<RedisOptions>();


            var serviceProvider = new ServiceCollection()
               .AddSingleton<IConfiguration>(configuration)
               .AddSingleton<ILocationInfoService, LocationInfoManager>()
               .AddSingleton<ILocationInfoDal, EfLocationInfoDal>()
               .AddSingleton<IRabbitMqConfiguration, RabbitMqConfiguration>()
               .AddSingleton<IRabbitMqService, RabbitMqService>()
               .AddSingleton<IObjectDataConverter, ObjectDataConverterManager>()  
               .AddSingleton<IConsumerService, ConsumerManager>()
               .AddSingleton<ICacheManager, RedisCacheManager>()
               .AddDistributedRedisCache(option =>
               {
                   option.Configuration = redisOptions.Configuration;
                   option.InstanceName = redisOptions.InstanceName;
               })
               .BuildServiceProvider();
             


            var consumerService = serviceProvider.GetService<IConsumerService>();
            Console.WriteLine("consumerService alındı.");
            Console.WriteLine($"consumerService.Start() başladı: {DateTime.Now.ToShortTimeString()}");
            await consumerService.Start();
            Console.WriteLine($"consumerService.Start() bitti:  {DateTime.Now.ToShortTimeString()}");
        }
    }
}