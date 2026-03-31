using DataSaveHelpers.EntityTypesRealizations.Identified;
using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.WithCache;

public class CachedIdentifiedRepository<T, TKey>(
        ICacheService cache,
        DbContext context, 
        IProcessQueueTasksService processQueueTasks
    ) : CachedBaseRepository<T>(
        cache, 
        context, 
        processQueueTasks
    ), IIdentifiedRepository<T, TKey> where T : IdentifiedEntity<TKey>
{
    public async Task<T?> GetByIdAsync(TKey id)
    {
        try
        {
            T? entity = await _cache.Get<T>(GetKey(id));
            entity ??= await _set.FirstOrDefaultAsync(x => x.Id!.Equals(id));

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task<bool> DeleteById(TKey id)
    {
        try
        {
            await _cache.Delete(GetKey(id));
            return (
                await _set
                .Where(x => x.Id!.Equals(id)
                ).ExecuteDeleteAsync()) > 0;
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
}
