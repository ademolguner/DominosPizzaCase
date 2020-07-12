using System.Collections.Generic;
using System.Threading.Tasks;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IPublisherService
    {
        void Enqueue<T>(IEnumerable<T> queueDataModels, string queueName) where T : class, new();

        Task EnqueueAsync<T>(IEnumerable<T> queueDataModels, string queueName) where T : class, new();
    }
}