using DataSaveHelpers.EntityTypesRealizations.Identified;
using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.WithCache;

public class CachedSoftDeletableIdentifiedRepository<T, TKey>(
    ICacheService cache, 
    DbContext context, 
    IProcessQueueTasksService processQueueTasks
    ) : CachedIdentifiedRepository<T, TKey>(
        cache,
        context,
        processQueueTasks
        ), ISoftDeletableIdentifiedRepository<T, TKey> where T : IdentifiedSoftDeletableEntity<TKey>
{
    public async Task SoftDeleteAsync(T entity)
    {
        try
        {
            entity.IsDeleted = true;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await _cache.Save(GetKey(entity), entity);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task<bool> SoftDeleteAsync(TKey id)
    {
        try
        {
            T? entity = await GetByIdAsync(id);
            if (entity is null) return false;

            await SoftDeleteAsync(entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task<bool> SoftExecuteDelete(TKey id)
    {
        bool result = await _set
            .Where(x => x.Id!.Equals(id))
            .ExecuteUpdateAsync(x =>
                x.SetProperty(
                    x => x.IsDeleted,
                    true)) > 0;
        if (result)
        {
            T? entity = await _cache.Get<T>(GetKey(id));
            if(entity is not null)
            {
                entity.IsDeleted = true;
                SaveToCache(GetKey(id), entity);
            }

            return true;
        }
        return false;
    }
}