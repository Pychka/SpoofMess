using AdditionalHelpers;
using DataHelpers.Services;
using StackExchange.Redis;

namespace DataHelpers.ServiceRealizations.Cache.Redis;

public class BaseRedisCache(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _database = redis.GetDatabase();
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(10);

    public async Task Delete(string key)
    {
        try
        {
            await _database.StringDeleteAsync(key, When.Always);
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
