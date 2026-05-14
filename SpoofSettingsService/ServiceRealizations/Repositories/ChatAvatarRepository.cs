using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services.Repositories;

namespace SpoofSettingsService.ServiceRealizations.Repositories;

public class ChatAvatarRepository(
    ICacheService cache,
    IDbContextFactory<SpoofSettingsServiceContext> factory,
    IProcessQueueTasksService tasksService
    ) : CachedSoftDeletableIdentifiedFactoryRepository<ChatAvatar, Guid, SpoofSettingsServiceContext>(
        cache,
        factory,
        tasksService
    ), IChatAvatarRepository
{
    public override async Task<ChatAvatar?> GetByIdAsync(Guid id)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await GetAsync(
                GetKey(id),
                async () => await context.ChatAvatars.Include(x => x.File).FirstOrDefaultAsync(x =>
                x.Id == id)
            );
    }

    public async Task<ChatAvatar?> GetActualChatAvatarById(Guid chatId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await GetAsync(
                GetKey(chatId),
                async () => await context.ChatAvatars.FirstOrDefaultAsync(x =>
                x.ChatId == chatId 
                && x.IsActive)
            );
    }

    public async Task<List<ChatAvatar>?> GetChatAvatarsById(Guid chatId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await context.ChatAvatars.Where(x =>
                x.ChatId == chatId 
                && !x.IsDeleted
            ).ToListAsync();
    }

    public async Task<bool> TryDeleteAvatarByIds(Guid chatId, Guid fileId)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        ChatAvatar? avatar = await context.ChatAvatars.FirstOrDefaultAsync(x => 
            x.ChatId == chatId 
            && x.FileId == fileId
        );
        if (avatar is null)
            return false;

        await SoftDeleteAsync(avatar);
        return true;
    }

    private static string GetKey(Guid chatId) =>
        $"{typeof(UserAvatar).Name.ToLower()}:{chatId}";
}
