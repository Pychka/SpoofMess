using AdditionalHelpers;
using DataHelpers.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DataHelpers.ServiceRealizations.Cache.Memory;

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
        _loggerService.Trace($"Delete by {key} in cache");
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
        _loggerService.Trace($"Get by {key} from cache");
        return Task.FromResult(value);
    }

    public Task Save<T>(string key, T value)
    {
        _cache.Set(key, value);
        _loggerService.Trace($"Save by {key} to cache");
        return Task.CompletedTask;
    }
}
