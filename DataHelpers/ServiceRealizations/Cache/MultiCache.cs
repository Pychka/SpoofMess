using AdditionalHelpers;
using DataHelpers.Services;

namespace DataHelpers.ServiceRealizations.Cache;

public class MultiCache(IMemoryCacheService localCache, IRedisService cache, ILoggerService loggerService) : ICacheService
{
    protected readonly IRedisService _cache = cache;
    protected readonly IMemoryCacheService _localCache = localCache;
    private readonly ILoggerService _loggerService = loggerService;

    public async Task Save<T>(string key, T value)
    {
        await _localCache.Save(key, value);
        _loggerService.Trace($"Save by {key} to in-memory cache");
        await _cache.Save(key, value);
        _loggerService.Trace($"Save by {key} to redis");
    }

    public async Task Delete(string key)
    {
        await _localCache.Delete(key);
        _loggerService.Trace($"Delete by {key} to in-memory cache");
        await _cache.Delete(key);
        _loggerService.Trace($"Delete by {key} to redis");
    }

    public async Task<T?> Get<T>(string key)
    {
        T? entity = await _localCache.Get<T?>(key);
        if (entity is not null)
        {
            _loggerService.Trace($"Received by {key} from in-memory cache");
            return entity;
        }

        entity = await _cache.Get<T?>(key);
        if (entity is not null)
        {
            _loggerService.Trace($"Received by {key} from redis");
            await _localCache.Save(key, entity);
        }

        return entity;
    }
}
