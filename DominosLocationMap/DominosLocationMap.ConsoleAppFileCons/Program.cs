﻿using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Business.Managers;
using DominosLocationMap.Business.Queues.RabbitMq;
using DominosLocationMap.Business.Queues.Redis;
using DominosLocationMap.Core.CrossCutting.Caching;
using DominosLocationMap.Core.CrossCutting.Caching.Redis;
using DominosLocationMap.Core.Entities.Options;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.DataAccess.Concrete.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DominosLocationMap.ConsoleAppFileCons
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("RabbitMq.FileOperationConsumerManager Program.cs Acıldı.");

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("project.json");

            Console.WriteLine("project.json okundu.");
            var configuration = builder.Build();

            var redisOptions = configuration.GetSection("RedisOptions").Get<RedisOptions>();
            var locationOptions = configuration.GetSection("LocationOptions").Get<LocationOptions>();

            var serviceProvider = new ServiceCollection()
               .AddSingleton<IConfiguration>(configuration)
               .AddScoped<IRabbitMqConfiguration, RabbitMqConfiguration>()
               .AddScoped<IRabbitMqService, RabbitMqService>()
               .AddScoped<IObjectDataConverter, ObjectDataConverterManager>()
               .AddScoped<IConsumerService, FileOperationConsumerManager>()
               .AddSingleton<ICacheManager, RedisCacheManager>()
               .AddScoped<ILocationInfoService, LocationInfoManager>()
                .AddScoped<ILocationInfoDal, EfLocationInfoDal>()
                .AddScoped<IPublisherService, PublisherManager>()
                .AddAutoMapper(typeof(Program))
               .AddDistributedRedisCache(option =>
               {
                   option.Configuration = redisOptions.Configuration;
                   option.InstanceName = redisOptions.InstanceName;
               })
               .BuildServiceProvider();

            var consumerService = serviceProvider.GetService<IConsumerService>();
            Console.WriteLine("FileOperationConsumerManager alındı.");
            Console.WriteLine($"FileOperationConsumerManager.Start() başladı: {DateTime.Now.ToShortTimeString()}");
            await consumerService.Start();
            Console.WriteLine($"FileOperationConsumerManager.Start() bitti:  {DateTime.Now.ToShortTimeString()}");
        }
    }
}