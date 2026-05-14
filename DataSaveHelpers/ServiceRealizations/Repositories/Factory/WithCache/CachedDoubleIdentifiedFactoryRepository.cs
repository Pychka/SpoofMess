using DataSaveHelpers.EntityTypesRealizations.DoubleIdentified;
using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;

public class CachedDoubleIdentifiedFactoryRepository<T, TKey1, TKey2, TDbContext>(
    ICacheService cache, 
    IDbContextFactory<TDbContext> factory, 
    IProcessQueueTasksService processQueueTasks
    ) : CachedFactoryBaseRepository<T, TDbContext>(
        cache, 
        factory, 
        processQueueTasks
    ), IDoubleIdentifiedRepository<T, TKey1, TKey2> 
    where T : DoubleIdentifiedEntity<TKey1, TKey2>
    where TDbContext : DbContext
{
    public virtual async Task<T?> GetByIdAsync(TKey1 key1, TKey2 key2)
    {
        try
        {
            T? entity = await _cache.Get<T>(GetKey(key1, key2));
            await using DbContext context = await _factory.CreateDbContextAsync();
            entity ??= await context.Set<T>().FirstOrDefaultAsync(x => 
                x.Key1!.Equals(key1)
                && x.Key2!.Equals(key2)
            );

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }
    protected override string GetKey(T entity) =>
        $"{entity.GetType().Name.ToLower()}:{entity.Key1}:{entity.Key2}";
    protected string GetKey(TKey1 key1, TKey2 key2) =>
        $"{typeof(T).Name.ToLower()}:{key1}:{key2}";
}
