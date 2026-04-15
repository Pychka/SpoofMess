using AdditionalHelpers.Services;
using DataSaveHelpers.Services;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace DataSaveHelpers.ServiceRealizations.Cache.Memory;

public class LocalCacheService(IMemoryCache cache, ILoggerService loggerService) : IMemoryCacheService, IDisposable
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILoggerService _loggerService = loggerService;

    public Task Clear()
    {
        _cache.Dispose();
        return Task.CompletedTask;
    }

    public Task Delete(string key)
    {
        _cache.Remove(key);
#if DEBUG
        _loggerService.Debug($"Delete by {key} in cache");
#endif
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<T?> Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
#if DEBUG
        _loggerService.Debug($"Get by {key} from cache");
#endif
        return Task.FromResult(value);
    }

    public Task MultiSave(KeyValuePair<RedisKey, RedisValue>[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            Save(value.Key!, value.Value);
        }
        return Task.CompletedTask;
    }

    public Task Save<T>(string key, T value)
    {
        _cache.Set(key, value);
#if DEBUG
        _loggerService.Debug($"Save by {key} to cache");
#endif
        return Task.CompletedTask;
    }

    public async Task SaveRange<T>(Func<T, string> getKey, List<T> values)
    {
        T value;
        for (int i = 0; i < values.Count; i++)
        {
            value = values[i];
            await Save(getKey(value), value);
        }
    }
}
