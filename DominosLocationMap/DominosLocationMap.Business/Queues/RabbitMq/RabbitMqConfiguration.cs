using DominosLocationMap.Core.RabbitMQ;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Business.Queues.RabbitMq
{
    public class RabbitMqConfiguration : IRabbitMqConfiguration
    {
        public IConfiguration Configuration { get; }
        public RabbitMqConfiguration(IConfiguration configuration) => Configuration = configuration;
        public string HostName => Configuration.GetSection("RabbitMqOptions:HostName").Value;
        public string UserName => Configuration.GetSection("RabbitMqOptions:UserName").Value;
        public string Password => Configuration.GetSection("RabbitMqOptions:Password").Value;
        public string Port => Configuration.GetSection("RabbitMqOptions:Port").Value; 
        public string VirtualHost => Configuration.GetSection("RabbitMqOptions:VirtualHost").Value;
    }
}
