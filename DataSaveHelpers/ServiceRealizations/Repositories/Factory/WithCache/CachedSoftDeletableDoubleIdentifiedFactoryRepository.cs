using DataSaveHelpers.EntityTypesRealizations.DoubleIdentified;
using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;

public class CachedSoftDeletableDoubleIdentifiedFactoryRepository<T, TKey1, TKey2, TDbContext>(
    ICacheService cache,
    IDbContextFactory<TDbContext> factory,
    IProcessQueueTasksService processQueueTasks
    ) : CachedDoubleIdentifiedFactoryRepository<T, TKey1, TKey2, TDbContext>(
        cache,
        factory,
        processQueueTasks
        ), ISoftDeletableDoubleIdentifiedRepository<T, TKey1, TKey2>
    where T : DoubleIdentifiedSoftDeletable<TKey1, TKey2>
    where TDbContext : DbContext
{
    public virtual async Task SoftDeleteAsync(T entity)
    {
        try
        {
            entity.IsDeleted = true;
            await using DbContext context = await _factory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
            await _cache.Save(GetKey(entity), entity);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public virtual async Task<bool> SoftDeleteAsync(TKey1 key1, TKey2 key2)
    {
        try
        {
            T? entity = await GetByIdAsync(key1, key2);
            if (entity is null) return false;

            await SoftDeleteAsync(entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }
}