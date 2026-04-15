using AdditionalHelpers.Services;
using DataSaveHelpers.Services;
using StackExchange.Redis;

namespace DataSaveHelpers.ServiceRealizations.Cache.Redis;

public class BaseRedisCache(IConnectionMultiplexer redis, ILoggerService loggerService, ISerializer serializer) : IRedisService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly ILoggerService _loggerService = loggerService;
    private readonly ISerializer _serializer = serializer;
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(10);

    public async Task Delete(string key)
    {
        try
        {
            await _database.StringDeleteAsync(key, When.Always);

#if DEBUG
            _loggerService.Debug($"Delete by {key} in redis value");
#endif
        }
        catch
        {
            throw;
        }
    }

    public async Task Save<T>(string key, T value)
    {
        try
        {
            string json = _serializer.Serialize(value);
            await _database.StringSetAsync(key, json, Expiration, When.Always);

#if DEBUG
            _loggerService.Debug($"Save by {key} to redis");
#endif
        }
        catch
        {
            throw;
        }
    }

    public async Task<T?> Get<T>(string key)
    {
        try
        {
            RedisValue? redisValue = await _database.StringGetAsync(key);
            if (string.IsNullOrEmpty(redisValue.Value))
                return default;

#if DEBUG
            _loggerService.Debug($"Get by {key} from redis");
#endif
            return _serializer.Deserialize<T>(redisValue.Value.ToString());
        }
        catch
        {
            throw;
        }
    }

    public async Task SaveRange<T>(Func<T, string> getKey, List<T> values)
    {
        try
        {
            T value;
            for (int i = 0; i < values.Count; i++)
            {
                value = values[i];
                await Save(getKey(value), value);
            }
        }
        catch
        {
            throw;
        }
    }

    public async Task MultiSave(KeyValuePair<RedisKey, RedisValue>[] valuePairs)
    {
        try
        {
            await _database.StringSetAsync(valuePairs);
        }
        catch
        {
            throw;
        }

    }
}
