namespace DominosLocationMap.Business.Queues.Redis
{
    public class RedisOptions
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Configuration => $"{Host}:{Port}";
        public string InstanceName { get; set; }
    }
}