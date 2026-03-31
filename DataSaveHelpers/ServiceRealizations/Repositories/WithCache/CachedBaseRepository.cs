using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.WithCache;

public abstract class CachedBaseRepository<T>(
        ICacheService cache, 
        DbContext context, 
        IProcessQueueTasksService processQueueTasks
    ) : CachedRepository<T> where T : class
{
    protected readonly IProcessQueueTasksService _processQueueTasks = processQueueTasks;
    protected readonly ICacheService _cache = cache;
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();
    public async Task AddAsync(T entity)
    {
        try
        {
            await _set.AddAsync(entity);
            await _context.SaveChangesAsync();
            SaveToCache(GetKey(entity), entity);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task DeleteAsync(T entity)
    {
        try
        {
            await _cache.Delete(GetKey(entity));
            _set.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            _set.Update(entity);
            await _context.SaveChangesAsync();
            SaveToCache(GetKey(entity), entity);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public async Task UpdateRangeAsync(List<T> entities)
    {
        try
        {
            _set.UpdateRange(entities);
            await _context.SaveChangesAsync();
            await _cache.SaveRange(GetKey, entities);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    protected async ValueTask<T?> GetFromDb(string key, Func<Task<T?>> function)
    {
        T? entity = await function();
        if (entity is not null)
            SaveToCache(key, entity);

        return entity;
    }
    protected override async ValueTask<T?> GetAsync(string key, Func<Task<T?>> function, TimeSpan? expiration = null)
    {
        try
        {
            T? entity = await _cache.Get<T>(key);

            entity ??= await GetFromDb(key, function);

            return entity;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("DataBase error", ex);
        }
    }

    protected abstract string GetKey(T entity);

    protected override void SaveToCache(string key, T entity) =>
        _processQueueTasks.AddTask(async () => await _cache.Save(key, entity));

    protected void SaveEntityToCache<TEntity>(string key, TEntity entity) =>
        _processQueueTasks.AddTask(async () => await _cache.Save(key, entity));

    protected override void SaveRangeToCache(List<T> entities) =>
        _processQueueTasks.AddTask(async () => await _cache.SaveRange(GetKey, entities));

}
