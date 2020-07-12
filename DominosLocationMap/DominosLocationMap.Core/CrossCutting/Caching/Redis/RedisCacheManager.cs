using ServiceStack.Redis;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DominosLocationMap.Core.CrossCutting.Caching.Redis
{
    public class RedisCacheManager : ICacheManager
    {
        private readonly IRedisClient _redisClient;
        private readonly ConnectionMultiplexer _connection;

        public RedisCacheManager()
        {
            _redisClient = new RedisClient();
            _connection = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                AllowAdmin = true,
                EndPoints =
                {
                    _redisClient.Host,_redisClient.Port.ToString()
                },
                AbortOnConnectFail = false
            });
        }

        public T Get<T>(string key)
        {
            return (T)_redisClient.Get<T>(key);
        }

        public void Add(string key, object data, int cacheTime = 60)
        {
            if (data != null && _connection.IsConnected)
            {
                _redisClient.Add<object>(key, data, DateTime.Now.AddMinutes(cacheTime));
            }
        }

        public bool IsAdd(string key)
        {
            if (_connection.IsConnected)
                return _redisClient.ContainsKey(key);
            return false;
        }

        public void Remove(string key)
        {
            if (_connection.IsConnected)
            {
                _redisClient.Remove(key);
            }
        }

        public void RemoveByPattern(string pattern)
        {
            if (_connection.IsConnected)
            {
                var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var keysToRemove = _redisClient.GetAllKeys().Where(d => regex.IsMatch(d)).Select(d => d).ToList();
                foreach (var key in keysToRemove)
                {
                    Remove(key);
                }
            }
        }

        public void Clear()
        {
            if (_connection.IsConnected)
            {
                foreach (var item in _redisClient.GetAllKeys())
                {
                    Remove(item);
                }
            }
        }
    }
}