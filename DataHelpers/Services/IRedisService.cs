namespace DataHelpers.Services;

public interface IRedisService
{
    public Task Save<T>(
        string key, T value);

    public Task Delete(
        string key);

    public Task<T?> Get<T>(string key);
}