using DataHelpers.Services;

namespace DataHelpers.ServiceRealizations.Cache;

public class MultiCache(IMemoryCacheService localCache, IRedisService cache) : ICacheService
{
    protected readonly IRedisService _cache = cache;
    protected readonly IMemoryCacheService _localCache = localCache;

    public async Task Save<T>(string key, T value)
    {
        await _localCache.Save(key, value);
        await _cache.Save(key, value);
    }

    public async Task Delete(string key)
    {
        await _localCache.Delete(key);
        await _cache.Delete(key);
    }

    public async Task<T?> Get<T>(string key)
    {
        T? entity = await _localCache.Get<T?>(key);
        if (entity is not null)
            return entity;

        entity = await _cache.Get<T?>(key);
        if (entity is not null)
            await _localCache.Save(key, entity);

        return entity;
    }

    public async ValueTask<List<T>?> GetMany<T>(string key)
    {
        List<T>? entities = await _localCache.Get<List<T>?>(key);
        entities ??= await _cache.Get<List<T>?>(key);

        return entities;
    }
}
