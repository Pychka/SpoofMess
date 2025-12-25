using AdditionalHelpers;
using DataHelpers.Services;
using StackExchange.Redis;

namespace DataHelpers.ServiceRealizations;

public class CacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async ValueTask<bool> Delete(string key, When when = When.Always)
    {
        try
        {
            return await _database.StringDeleteAsync(key, when);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async ValueTask<T?> Get<T>(string key)
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

    public async ValueTask<bool> Save<T>(string key, T value, TimeSpan? expiration = null, When when = When.Always)
    {
        try
        {
            string json = JsonService.Serialize(value);
            return await _database.StringSetAsync(key, json, expiration, expiration is null, when);
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
