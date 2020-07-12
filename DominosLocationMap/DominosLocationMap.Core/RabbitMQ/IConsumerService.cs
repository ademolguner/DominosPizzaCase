using System;
using System.Threading.Tasks;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IConsumerService : IDisposable
    {
        Task Start();

        void Stop();
    }
}