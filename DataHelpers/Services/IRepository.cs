using Microsoft.EntityFrameworkCore;

namespace DataHelpers.Services;

public interface IRepository
{
    public Task<T?> GetAsync<T>(
        string key,
        Func<Task<T>> function,
        TimeSpan? expiration = null);

    public Task<T> ChangeState<T>(
        DbContext context,
        string key,
        T obj,
        EntityState state = EntityState.Added,
        TimeSpan? expiration = null)
        where T : class;

    public Task<List<T>> ChangeStates<T>(
        DbContext context,
        (string key, T obj, EntityState state)[] entities,
        TimeSpan? expiration = null)
        where T : class;
}