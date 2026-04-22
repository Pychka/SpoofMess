using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class ChatRepository(
    ICacheService cache,
    IDbContextFactory<SpoofMessageServiceContext> factory,
    IProcessQueueTasksService processQueueTasks)
    : CachedSoftDeletableIdentifiedFactoryRepository<Chat, Guid, SpoofMessageServiceContext>(
        cache,
        factory,
        processQueueTasks), IChatRepository
{
    public async Task<bool> ExecuteUpdateAvatar(Guid chatId, Guid fileId, string originalFileName)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        int count = await context.Chats.Where(x => x.Id.Equals(chatId)).ExecuteUpdateAsync(x =>
            x.SetProperty(p => p.AvatarId, fileId)
            .SetProperty(x => x.OriginalFileName, originalFileName)
        );
        Chat? chat = await _cache.Get<Chat>(GetKey(chatId));
        if (chat is null)
            return count > 0;
        chat.AvatarId = fileId;
        chat.OriginalFileName = originalFileName;
        await _cache.Save(GetKey(chatId), chat);
        return count > 0;
    }

}
