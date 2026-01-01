using AdditionalHelpers;
using DataHelpers.Services;
using StackExchange.Redis;

namespace DataHelpers.ServiceRealizations.Cache.Redis;

public class BaseRedisCache(IConnectionMultiplexer redis, ILoggerService loggerService) : IRedisService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly ILoggerService _loggerService = loggerService;
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(10);

    public async Task Delete(string key)
    {
        try
        {
            await _database.StringDeleteAsync(key, When.Always);
            _loggerService.Trace($"Delete by {key} in redis value");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task Save<T>(string key, T value)
    {
        try
        {
            string json = JsonService.Serialize(value);
            await _database.StringSetAsync(key, json, Expiration, When.Always);
            _loggerService.Trace($"Save by {key} to redis");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<T?> Get<T>(string key)
    {
        try
        {
            RedisValue? redisValue = await _database.StringGetAsync(key);
            if (string.IsNullOrEmpty(redisValue.Value))
                return default;

            _loggerService.Trace($"Get by {key} from redis");
            return JsonService.Deserialize<T>(redisValue.Value.ToString());
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDataException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
