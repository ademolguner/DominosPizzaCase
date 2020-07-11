using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.Entities.Consts;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Business.Queues.RabbitMq
{
    public class PublisherManager : IPublisherService
    {

        private readonly IRabbitMqService _rabbitMqServices;
        private readonly IObjectDataConverter _objectDataConverter;
        private IModel _channel;
        private IConnection _connection;

        public PublisherManager(IRabbitMqService rabbitMqServices, IObjectDataConverter objectDataConverter)
        {
            _rabbitMqServices = rabbitMqServices;
            _objectDataConverter = objectDataConverter;
        }

        public void Enqueue<T>(IEnumerable<T> queueDataModels, string queueName) where T : class, new()
        {
            try
            {
                _connection = _rabbitMqServices.GetConnection();
                using (_channel = _rabbitMqServices.GetModel(_connection))
                {
                    _channel.QueueDeclare(queue: queueName,
                                         durable: true,      // ile in-memory mi yoksa fiziksel olarak mı saklanacağı belirlenir.
                                         exclusive: false,   // yalnızca bir bağlantı tarafından kullanılır ve bu bağlantı kapandığında sıra silinir - özel olarak işaretlenirse silinmez
                                         autoDelete: false,  // en son bir abonelik iptal edildiğinde en az bir müşteriye sahip olan kuyruk silinir
                                         arguments: null);   // isteğe bağlı; eklentiler tarafından kullanılır ve TTL mesajı, kuyruk uzunluğu sınırı, vb. 

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Expiration = RabbitMqConsts.MessagesTTL.ToString();

                    foreach (var queueDataModel in queueDataModels)
                    {
                        var body = Encoding.UTF8.GetBytes(_objectDataConverter.ObjectToJson(queueDataModel));
                        _channel.BasicPublish(exchange: "",
                                             routingKey: queueName,
                                             mandatory: false,
                                             basicProperties: properties,
                                             body: body);
                    }
                }
            }
            catch (Exception ex)
            {
                //loglama yapılabilir
                throw new Exception(ex.InnerException.Message.ToString());
            }
        }
    }
}
