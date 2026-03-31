using DataSaveHelpers.EntityTypesRealizations.Identified;

namespace DataSaveHelpers.Services.Repositories;

public interface IIdentifiedRepository<T, TKey> : IBaseRepository<T> where T : IdentifiedEntity<TKey>
{
    public Task<T?> GetByIdAsync(TKey id);

    public Task<bool> DeleteById(TKey id);
}
