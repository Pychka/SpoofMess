using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class UserRepository(
    ICacheService cache,
    IDbContextFactory<SpoofMessageServiceContext> factory,
    IProcessQueueTasksService processQueueTasks
    ) : CachedSoftDeletableIdentifiedFactoryRepository<User, Guid, SpoofMessageServiceContext>(
        cache,
        factory,
        processQueueTasks
        ), IUserRepository
{
    public async Task<int> GetCountActiveUsers()
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return context.Users.Where(x => x.IsConnected).Count();
    }
    public async Task<bool> ExecuteUpdateAvatar(Guid userId, Guid fileId, string originalFileName)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        int count = await context.Users.Where(x => x.Id.Equals(userId)).ExecuteUpdateAsync(x =>
            x.SetProperty(p => p.AvatarId, fileId)
            .SetProperty(x => x.OriginalFileName, originalFileName)
        );
        User? user = await _cache.Get<User>(GetKey(userId));
        if (user is null)
            return count > 0;
        user.AvatarId = fileId;
        user.OriginalFileName = originalFileName;
        await _cache.Save(GetKey(userId), user);
        return count > 0;
    }

    public async Task<bool> ExecuteUpdateConnection(Guid userId, bool isConnected)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        int count = await context.Users.Where(x => x.Id.Equals(userId)).ExecuteUpdateAsync(x =>
            x.SetProperty(p => p.IsConnected, isConnected)
        );
        User? user = await _cache.Get<User>(GetKey(userId));
        if (user is null)
            return count > 0;
        user.IsConnected = isConnected;
        await _cache.Save(GetKey(userId), user);
        return count > 0;
    }
}
