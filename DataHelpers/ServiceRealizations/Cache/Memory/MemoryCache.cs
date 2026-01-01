using DataHelpers.Services;

namespace DataHelpers.ServiceRealizations.Cache.Memory;

public class MemoryCache(IMemoryCacheService cache) : ICacheService
{
    protected readonly IMemoryCacheService _cache = cache;

    public async Task Save<T>(string key, T value) =>
        await _cache.Save(key, value);

    public async Task Delete(string key) =>
        await _cache.Delete(key);

    public async Task<T?> Get<T>(string key) =>
        await _cache.Get<T?>(key);

    public async ValueTask<List<T>?> GetMany<T>(string key) =>
        await _cache.Get<List<T>?>(key);
}