using DataHelpers.Services;

namespace DataHelpers.ServiceRealizations.Cache.Redis;

public class RedisCache(IRedisService cache) : ICacheService
{
    protected readonly IRedisService _cache = cache;

    public async Task Save<T>(string key, T value) =>
        await _cache.Save(key, value);

    public async Task Delete(string key) =>
        await _cache.Delete(key);

    public async Task<T?> Get<T>(string key) =>
        await _cache.Get<T?>(key);

    public async ValueTask<List<T>?> GetMany<T>(string key) => 
        await _cache.Get<List<T>?>(key);
}
