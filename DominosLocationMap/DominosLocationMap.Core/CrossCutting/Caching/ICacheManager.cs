namespace DominosLocationMap.Core.CrossCutting.Caching
{
    public interface ICacheManager
    {
        T Get<T>(string key);

        void Add(string key, object data, int cacheTime = 60);

        bool IsAdd(string key);

        void Remove(string key);

        void RemoveByPattern(string pattern);

        void Clear();
    }
}