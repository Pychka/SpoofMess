using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;

namespace DataHelpers.ServiceRealizations;

public class Repository<T, TKey>(ICacheService cache, DbContext context, ProcessQueueTasksService processQueueTasks) where T : IdentifiedEntity<TKey>
{
    protected readonly ICacheService _cache = cache;
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();
    protected readonly ProcessQueueTasksService _processQueueTasks = processQueueTasks;
    protected virtual TimeSpan? Expiration { get; set; } = TimeSpan.FromMinutes(10);

    public virtual async ValueTask<T?> GetByIdAsync(TKey id)
    {
        try
        {
            string key = GetKey(id);
            T? entity = await _cache.Get<T>(key);

            entity ??= await GetFromDb(key, async () => await _set.FirstOrDefaultAsync(x => EqualityComparer<TKey>.Default.Equals(x.Id, id)));

            return entity;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    protected virtual async ValueTask<T?> GetAsync(TKey id, Func<Task<T?>> function)
    {
        try
        {
            string key = GetKey(id);
            T? entity = await _cache.Get<T>(key);

            entity ??= await GetFromDb(key, function);

            return entity;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    protected virtual async ValueTask<T?> GetAsync(string key, Func<Task<T?>> function)
    {
        try
        {
            T? entity = await _cache.Get<T>(key);

            entity ??= await GetFromDb(key, function);

            return entity;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    protected virtual async ValueTask<List<T>?> GetManyAsync(string key, Func<Task<List<T>?>> function)
    {
        try
        {
            List<T>? entities = await _cache.Get<List<T>>(key);
            if(entities is not null)
                return entities;

            entities = await function();
            if (entities is not null)
                _processQueueTasks.AddTask(async () => await _cache.Save(key, entities));

            return entities;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public virtual async Task AddAsync(T entity)
    {
        try
        {
            ChangeEntity(entity);
            await _set.AddAsync(entity);
            await _context.SaveChangesAsync();
            string key = GetKey(entity);

            SaveToCaches(key, entity);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            ChangeEntity(entity);
            _set.Remove(entity);
            await _context.SaveChangesAsync();
            string key = GetKey(entity);

            _processQueueTasks.AddTask(async () => await _cache.Delete(key));
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            ChangeEntity(entity);
            _set.Update(entity);
            await _context.SaveChangesAsync();
            string key = GetKey(entity);

            SaveToCaches(key, entity);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public virtual async Task UpdateRangeAsync(List<T> entities)
    {
        try
        {
            T? entity = null;

            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];
                ChangeEntity(entity);
            }

            _set.UpdateRange(entities);
            await _context.SaveChangesAsync();

            string key = string.Empty;
            for (int i = 0; i < entities.Count; i++)
            {
                entity = entities[i];
                key = GetKey(entity);
                SaveToCaches(key, entity);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    protected virtual void ChangeEntity(T entity)
    {

    }

    protected async ValueTask<T?> GetFromDb(string key, Func<Task<T?>> function)
    {
        T? entity = await function();
        if (entity is not null)
            SaveToCaches(key, entity);

        return entity;
    }

    protected async Task<TResult> BeginTransaction<TResult>(Func<Task<TResult>> function)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            TResult result = await function();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task SoftDeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        ChangeEntity(entity);
        await UpdateAsync(entity);
    }

    protected virtual string GetKey(T entity) =>
        $"{entity.GetType().Name.ToLower()}:{entity.GetId}";

    protected virtual string GetKey(TKey id) =>
        $"{typeof(T).Name.ToLower()}:{id}";

    protected void SaveToCaches(string key, T entity) =>
        _processQueueTasks.AddTask(async () => await _cache.Save(key, entity));
}