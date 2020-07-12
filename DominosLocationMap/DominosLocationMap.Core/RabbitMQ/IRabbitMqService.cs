using RabbitMQ.Client;

namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IRabbitMqService
    {
        IConnection GetConnection();

        IModel GetModel(IConnection connection);
    }
}