using StackExchange.Redis;

namespace DataHelpers.Services;

public interface ICacheService
{
    public ValueTask<bool> Save<T>(
        string key, T value,
        TimeSpan? expiration = null,
        When when = When.Always);

    public ValueTask<bool> Delete(
        string key,
        When when = When.Always);

    public ValueTask<T?> Get<T>(string key);
}
