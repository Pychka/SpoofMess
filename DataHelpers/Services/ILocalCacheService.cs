namespace DataHelpers.Services;

public interface ILocalCacheService
{
    public void Delete(string key);

    public T? Get<T>(string key);

    public void Save<T>(string key, T value);
}
