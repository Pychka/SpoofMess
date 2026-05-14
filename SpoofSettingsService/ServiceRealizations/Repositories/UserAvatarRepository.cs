using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services.Repositories;

namespace SpoofSettingsService.ServiceRealizations.Repositories;

public class UserAvatarRepository(
        ICacheService cache,
        IDbContextFactory<SpoofSettingsServiceContext> factory,
        IProcessQueueTasksService tasksService
    ) : CachedSoftDeletableIdentifiedFactoryRepository<UserAvatar, Guid, SpoofSettingsServiceContext>(
        cache, 
        factory,
        tasksService
    ), IUserAvatarRepository
{

    public override async Task<UserAvatar?> GetByIdAsync(Guid id)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await context.UserAvatars.Include(x => x.File).FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<UserAvatar?> GetActualUserAvatarById(Guid userId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await GetAsync(GetKey(userId), async () => 
            await context.UserAvatars.FirstOrDefaultAsync(x => 
                x.UserId == userId 
                && x.IsActive
            )
        );
    }

    public async Task<List<UserAvatar>?> GetUserAvatarsById(Guid userId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await context.UserAvatars.Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync();
    }

    public async Task<bool> TryDeleteAvatarByIds(Guid userId, Guid fileId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        UserAvatar? avatar = await context.UserAvatars.FirstOrDefaultAsync(x =>
            x.UserId == userId 
            && x.FileId == fileId
        );
        if (avatar is null)
            return false;

        await SoftDeleteAsync(avatar);
        return true;
    }

    private static string GetKey(Guid userId) =>
        $"{typeof(UserAvatar).Name.ToLower()}:{userId}";

    protected override string GetKey(UserAvatar entity) =>
        $"{typeof(UserAvatar).Name.ToLower()}:{entity.UserId}:{entity.FileId}";

}
