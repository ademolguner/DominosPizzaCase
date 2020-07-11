using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IConsumerService : IDisposable
    {
        Task Start();
        void Stop();
    }
}
