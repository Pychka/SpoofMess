using DataSaveHelpers.EntityTypesRealizations.Identified;
using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;

public class CachedIdentifiedFactoryRepository<T, TKey, TDbContext>(
        ICacheService cache, 
        IDbContextFactory<TDbContext> factory,
        IProcessQueueTasksService processQueueTasks
    ) : CachedFactoryBaseRepository<T, TDbContext>(
            cache,
            factory,
            processQueueTasks
        ), IIdentifiedRepository<T, TKey> where T : IdentifiedEntity<TKey>
    where TDbContext : DbContext
{
    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        try
        {
            T? entity = await _cache.Get<T>(GetKey(id));
            await using DbContext context = await _factory.CreateDbContextAsync();
            entity ??= await context.Set<T>().FirstOrDefaultAsync(x => x.Id!.Equals(id));

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }
    protected override string GetKey(T entity) =>
        $"{entity.GetType().Name.ToLower()}:{entity.Id}";

    protected string GetEntityKey<TEntity, TEntityKey>(TEntity entity) where TEntity : IdentifiedEntity<TEntityKey> =>
        $"{entity.GetType().Name.ToLower()}:{entity.Id}";

    protected virtual string GetKey(TKey id) =>
        $"{typeof(T).Name.ToLower()}:{id}";

    public async Task<bool> DeleteById(TKey id)
    {
        try
        {
            await _cache.Delete(GetKey(id));
            await using DbContext context = await _factory.CreateDbContextAsync();
            return (
                await context.Set<T>()
                .Where(x => x.Id!.Equals(id)
                ).ExecuteDeleteAsync()) > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }
}
