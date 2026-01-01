using Microsoft.EntityFrameworkCore;

namespace DataHelpers.Services;

public abstract class BaseRepository
{
    protected async virtual ValueTask<TEntity?> GetAsync<TEntity>(
        string key,
        Func<Task<TEntity>> function,
        TimeSpan? expiration = null) where TEntity : IIdentifiedEntity?
    { throw new NotImplementedException(); }

    protected async virtual ValueTask<TEntity> ChangeState<TEntity>(
        DbContext context,
        TEntity obj,
        EntityState state = EntityState.Added,
        TimeSpan? expiration = null) where TEntity : IIdentifiedEntity
    { throw new NotImplementedException(); }

    protected virtual ValueTask<List<TEntity>> ChangeStates<TEntity>(
        DbContext context,
        (TEntity obj, EntityState state)[] entities,
        TimeSpan? expiration = null) where TEntity : IIdentifiedEntity
    { throw new NotImplementedException(); }

    protected virtual ValueTask<List<T>?> GetMany<T>(
        string key,
        Func<Task<List<T>>> function,
        TimeSpan? expiration = null) where T : IIdentifiedEntity
    { throw new NotImplementedException(); }
}