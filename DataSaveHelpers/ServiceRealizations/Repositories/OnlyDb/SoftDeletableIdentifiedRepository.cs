using DataSaveHelpers.EntityTypesRealizations.Identified;
using DataSaveHelpers.Services.Repositories;
using Microsoft.EntityFrameworkCore;
namespace DataSaveHelpers.ServiceRealizations.Repositories.OnlyDb;

public class SoftDeletableIdentifiedRepository<T, TKey>(DbContext context) : IdentifiedRepository<T, TKey>(context), ISoftDeletableIdentifiedRepository<T, TKey> where T : IdentifiedSoftDeletableEntity<TKey>
{
    public async Task SoftDeleteAsync(T entity)
    {
        try
        {
            entity.IsDeleted = true;
            _set.Update(entity);
            await _context.SaveChangesAsync();
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

    public async Task<bool> SoftExecuteDelete(TKey id) =>
        await _set
            .Where(x => x.Id!.Equals(id))
            .ExecuteUpdateAsync(x =>
                x.SetProperty(
                    x => x.IsDeleted,
                    true)) > 0;
}