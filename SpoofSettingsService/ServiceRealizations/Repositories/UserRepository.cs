using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services.Repositories;

namespace SpoofSettingsService.ServiceRealizations.Repositories;

public class UserRepository(
        ICacheService cache, 
        IDbContextFactory<SpoofSettingsServiceContext> factory, 
        IProcessQueueTasksService tasksService
    ) : CachedSoftDeletableIdentifiedFactoryRepository<User, Guid, SpoofSettingsServiceContext>(
        cache,
        factory, 
        tasksService
    ), IUserRepository
{
    public async Task<User?> GetByLogin(string login)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await context.Users.Include(x => x.UserAvatars).FirstOrDefaultAsync(x => x.Login == login);
    }
}
