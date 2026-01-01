using DataHelpers.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DataHelpers.ServiceRealizations.Cache.Memory;

public class LocalCacheService(IMemoryCache cache) : IMemoryCacheService
{
    private readonly IMemoryCache _cache = cache;

    public Task Clear()
    {
        throw new NotImplementedException();
    }

    public Task Delete(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<T?> Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task Save<T>(string key, T value)
    {
        _cache.Set(key, value);
        return Task.CompletedTask;
    }
}
