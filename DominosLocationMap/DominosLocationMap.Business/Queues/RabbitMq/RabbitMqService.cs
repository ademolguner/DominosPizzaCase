using DominosLocationMap.Core.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DominosLocationMap.Business.Queues.RabbitMq
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IRabbitMqConfiguration _rabbitMQConfiguration;
        public RabbitMqService(IRabbitMqConfiguration rabbitMQConfiguration)
        {
            _rabbitMQConfiguration = rabbitMQConfiguration ?? throw new ArgumentNullException(nameof(rabbitMQConfiguration));
        }
        public IConnection GetConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _rabbitMQConfiguration.HostName,
                    UserName = _rabbitMQConfiguration.UserName,
                    Password = _rabbitMQConfiguration.Password
                    //VirtualHost=_rabbitMQConfiguration.VirtualHost
                    //Port = AmqpTcpEndpoint.UseDefaultPort// Convert.ToInt32(_rabbitMQConfiguration.Port)
                };
                  
                // Otomatik bağlantı kurtarmayı etkinleştirmek için,
                factory.AutomaticRecoveryEnabled = true;
                // Her 10 sn de bir tekrar bağlantı toparlanmaya çalışır 
                factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
                // sunucudan bağlantısı kesildikten sonra kuyruktaki mesaj tüketimini sürdürmez 
                // (TopologyRecoveryEnabled = false   olarak tanımlandığı için)
                factory.TopologyRecoveryEnabled = false;

                return factory.CreateConnection();
            }
            catch (BrokerUnreachableException)
            {
                // loglama işlemi yapabiliriz
                Thread.Sleep(5000);
                // farklı business ta yapılabilir, ancak biz tekrar bağlantı (connection) kurmayı deneyeceğiz
                return GetConnection();
            }
            catch(Exception ec)
            {
                return GetConnection();
            }
        }

        public IModel GetModel(IConnection connection)
        {
            return connection.CreateModel();
        }
    }
}
