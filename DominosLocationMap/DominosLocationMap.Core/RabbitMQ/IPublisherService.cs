using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IPublisherService
    {
        void Enqueue<T>(IEnumerable<T> queueDataModels, string queueName) where T : class, new();
    }
}
