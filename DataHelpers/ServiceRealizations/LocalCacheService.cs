using DataHelpers.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DataHelpers.ServiceRealizations;

public class LocalCacheService(IMemoryCache cache) : ILocalCacheService
{
    private readonly IMemoryCache _cache = cache;
    public void Delete(string key)
    {
        _cache.Remove(key);
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Save<T>(string key, T value)
    {
        _cache.Set(key, value);
    }
}
