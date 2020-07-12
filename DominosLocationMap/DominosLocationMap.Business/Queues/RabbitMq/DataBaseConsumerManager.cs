using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.Entities.Consts;
using DominosLocationMap.Entities.Models.Queue;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DominosLocationMap.Business.Queues.RabbitMq
{
    public class DataBaseConsumerManager : IConsumerService
    {
        private SemaphoreSlim _semaphore;

        //olaylar
        public event EventHandler<LocationReadDataQueue> MessageReceived;

        public event EventHandler<LocationWriteDataQueue> MessageProcessed;

        private EventingBasicConsumer _consumer;
        private IModel _channel;
        private IConnection _connection;

        private readonly IRabbitMqService _rabbitMqServices;
        private readonly IObjectDataConverter _objectDataConverter;
        private readonly ILocationInfoService _locationInfoService;

        public DataBaseConsumerManager(IRabbitMqService rabbitMqServices, IObjectDataConverter objectDataConverter, ILocationInfoService locationInfoService)
        {
            _rabbitMqServices = rabbitMqServices;
            _objectDataConverter = objectDataConverter;
            _locationInfoService = locationInfoService ?? throw new ArgumentNullException(nameof(locationInfoService));
        }

        public async Task Start()
        {
            try
            {
                _semaphore = new SemaphoreSlim(RabbitMqConsts.ParallelThreadsCount);
                _connection = _rabbitMqServices.GetConnection();
                _channel = _rabbitMqServices.GetModel(_connection);
                _channel.QueueDeclare(
                                        queue: RabbitMqConsts.RabbitMqConstsList.DominosLocationDatabaseQueue.ToString(),
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null
                                     );

                _channel.BasicQos(0, RabbitMqConsts.ParallelThreadsCount, false);
                _consumer = new EventingBasicConsumer(_channel);
                _consumer.Received += Consumer_Received;
                await Task.FromResult(
                                     _channel.BasicConsume(queue: RabbitMqConsts.RabbitMqConstsList.DominosLocationDatabaseQueue.ToString(),
                                     autoAck: false,
                                     /* autoAck: bir mesajı aldıktan sonra bunu anladığına
                                        dair(acknowledgment) kuyruğa bildirimde bulunur ya da timeout gibi vakalar oluştuğunda
                                        mesajı geri çevirmek(Discard) veya yeniden kuyruğa aldırmak(Re-Queue) için dönüşler yapar*/
                                     consumer: _consumer));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message.ToString());
            }
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                _semaphore.Wait();
                LocationReadDataQueue message = _objectDataConverter.JsonToObject<LocationReadDataQueue>(Encoding.UTF8.GetString(ea.Body));
                MessageReceived?.Invoke(this, message);

                Task.Run(() =>
                {
                    try
                    {
                        var task = _locationInfoService.GetLocationsDistanceAsync(message);
                        task.Wait();
                        var result = task.Result;

                        var insertedTask = _locationInfoService.QueueDatabaseCreatedAfterSendFileProcess(result);
                        insertedTask.Wait();

                        MessageProcessed?.Invoke(this, result);
                    }
                    catch (Exception ex)
                    {
                        //throw new Exception(ex.InnerException.Message.ToString());
                    }
                    finally
                    {
                        // Teslimat Onayı
                        _channel.BasicAck(ea.DeliveryTag, false);
                        // akışı - thread'i serbest bırakıyoruz ek thread alabiliriz.
                        _semaphore.Release();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message.ToString());
            }
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _semaphore.Dispose();
        }
    }
}