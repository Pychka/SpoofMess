using DataSaveHelpers.EntityTypesRealizations.Identified;

namespace DataSaveHelpers.Services.Repositories;

public interface ISoftDeletableIdentifiedRepository<T, TKey> : IIdentifiedRepository<T, TKey>, ISoftDeletableRepository<T> where T : IdentifiedSoftDeletableEntity<TKey>
{
    public Task<bool> SoftDeleteAsync(TKey id);

    public Task<bool> SoftExecuteDelete(TKey id);
}
