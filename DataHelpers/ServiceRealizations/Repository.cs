using AdditionalHelpers;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;

namespace DataHelpers.ServiceRealizations;

public class Repository(ICacheService cache, ILocalCacheService localCache) : IRepository
{
    private readonly ICacheService _cache = cache;
    private readonly ILocalCacheService _localCache = localCache;
    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> function, TimeSpan? expiration = null)
    {
        try
        {
            T? value = _localCache.Get<T>(key);
            if (value is not null)
                return value;

            value = await _cache.Get<T>(key);
            if(value is not null)
                return value;

            value = await function();
            if (value is not null)
                _ = Task.Run(async () => await _cache.Save(key, JsonService.Serialize(value), expiration));

            return value;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public async Task<T> ChangeState<T>(DbContext context, string key, T obj, EntityState state = EntityState.Added, TimeSpan? expiration = null) where T : class
    {
        try
        {
            context.Entry(obj).State = state;

            await context.SaveChangesAsync();
            await context.Entry(obj).ReloadAsync();

            if (state is EntityState.Deleted)
            {
                _ = Task.Run(() => _localCache.Delete(key));
                _ = Task.Run(async () => await _cache.Delete(key));
            }
            else if (state is EntityState.Added or EntityState.Modified)
            {
                _ = Task.Run(() => _localCache.Save(key, obj));
                _ = Task.Run(async () => await _cache.Save(key, JsonService.Serialize(obj), expiration));
                await context.Entry(obj).ReloadAsync();
            }
            return obj;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }

    public async Task<List<T>> ChangeStates<T>(DbContext context, (string key, T obj, EntityState state)[] entities, TimeSpan? expiration = null) where T : class
    {
        try
        {
            List<T> objects = [];
            for(int en = 0; en < entities.Length; en++)
                context.Entry(entities[en].obj).State = entities[en].state;

            await context.SaveChangesAsync();
            for (int en = 0; en < entities.Length; en++)
            {
                var (key, obj, state) = entities[en];
                if (state is EntityState.Deleted)
                {
                    _ = Task.Run(() => _localCache.Delete(key));
                    _ = Task.Run(async () => await _cache.Delete(key));
                }
                if (state is EntityState.Added or EntityState.Modified)
                {
                    _ = Task.Run(() => _localCache.Save(key, obj));
                    _ = Task.Run(async () => await _cache.Save(key, JsonService.Serialize(obj), expiration));
                    await context.Entry(obj).ReloadAsync();
                }
                objects.Add(obj);
            }

            return objects;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Ошибка при работе с БД", ex);
        }
    }
}
