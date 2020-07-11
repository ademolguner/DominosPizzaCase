using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IRabbitMqService
    {
        IConnection GetConnection();
        IModel GetModel(IConnection connection);
    }
}
