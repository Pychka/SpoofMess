namespace DataHelpers.Services;

public interface IMemoryCacheService : ICacheService
{
    public Task Clear();
}
