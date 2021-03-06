﻿using DominosLocationMap.Business.Abstract;
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
    public class FileOperationConsumerManager : IConsumerService
    {
        private SemaphoreSlim _semaphore;

        //olaylar
        public event EventHandler<LocationWriteDataQueue> MessageReceived;

        public event EventHandler<bool> MessageProcessed;

        private EventingBasicConsumer _consumer;
        private IModel _channel;
        private IConnection _connection;

        private readonly IRabbitMqService _rabbitMqServices;
        private readonly IObjectDataConverter _objectDataConverter;
        private readonly ILocationInfoService _locationInfoService;

        public FileOperationConsumerManager(IRabbitMqService rabbitMqServices, IObjectDataConverter objectDataConverter, ILocationInfoService locationInfoService)
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
                                        queue: RabbitMqConsts.RabbitMqConstsList.DominosLocationFileOptionQueue.ToString(),
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null
                                     );

                _channel.BasicQos(0, RabbitMqConsts.ParallelThreadsCount, false);
                _consumer = new EventingBasicConsumer(_channel);
                _consumer.Received += Consumer_Received;
                await Task.FromResult(
                                     _channel.BasicConsume(queue: RabbitMqConsts.RabbitMqConstsList.DominosLocationFileOptionQueue.ToString(),
                                     autoAck: false,
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
                LocationWriteDataQueue message = _objectDataConverter.JsonToObject<LocationWriteDataQueue>(Encoding.UTF8.GetString(ea.Body));
                MessageReceived?.Invoke(this, message);

                Task.Run(() =>
                {
                    try
                    {
                        var task = _locationInfoService.FileWritingOperation(message);
                        task.Wait();
                        var result = task.Result;
                        MessageProcessed?.Invoke(this, result);
                    }
                    catch (Exception ex)
                    {
                        //log
                        //throw new Exception(ex.InnerException.Message.ToString());
                    }
                    finally
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _semaphore.Release();
                    }
                });
            }
            catch (Exception ex)
            {
                //loglama
                //throw new Exception(ex.InnerException.Message.ToString());
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