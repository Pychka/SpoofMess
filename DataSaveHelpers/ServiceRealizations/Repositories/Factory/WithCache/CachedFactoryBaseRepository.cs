using DataSaveHelpers.Services;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;

public abstract class CachedFactoryBaseRepository<T, TDbContext>(
    ICacheService cache,
    IDbContextFactory<TDbContext> factory,
    IProcessQueueTasksService processQueueTasks
    ) : CachedRepository<T> where T : class
    where TDbContext : DbContext
{
    protected readonly IProcessQueueTasksService _processQueueTasks = processQueueTasks;
    protected readonly ICacheService _cache = cache;
    protected readonly IDbContextFactory<TDbContext> _factory = factory;

    public virtual async Task AddAsync(T entity)
    {
        try
        {
            await using DbContext context = await _factory.CreateDbContextAsync();
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            SaveToCache(GetKey(entity), entity);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            await using DbContext context = await _factory.CreateDbContextAsync();
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
            await _cache.Delete(GetKey(entity));
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            await using DbContext context = await _factory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
            await _cache.Delete(GetKey(entity));
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    public virtual async Task UpdateRangeAsync(List<T> entities)
    {
        try
        {
            await using DbContext context = await _factory.CreateDbContextAsync();
            context.Set<T>().UpdateRange(entities);
            await context.SaveChangesAsync();
            await _cache.SaveRange(GetKey, entities);
        }
        catch (Exception ex)
        {
            throw new Exception("DataBase error", ex);
        }
    }

    protected virtual async ValueTask<T?> GetFromDb(string key, Func<Task<T?>> function)
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
