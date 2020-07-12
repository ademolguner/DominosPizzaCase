namespace DominosLocationMap.Core.RabbitMQ
{
    public interface IRabbitMqConfiguration
    {
        string HostName { get; }
        string UserName { get; }
        string Password { get; }
        string Port { get; }
        string VirtualHost { get; }
    }
}