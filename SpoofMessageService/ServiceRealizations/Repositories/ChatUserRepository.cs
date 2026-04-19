using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class ChatUserRepository(
    ICacheService cache,
    IDbContextFactory<SpoofMessageServiceContext> factory,
    IProcessQueueTasksService processQueueTasks) 
    : CachedSoftDeletableDoubleIdentifiedFactoryRepository<ChatUser, Guid, Guid, SpoofMessageServiceContext>(
        cache,
        factory,
        processQueueTasks), IChatUserRepository
{
    public async Task Delete(Guid chatId, Guid userId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        await context.ChatUsers.Where(x => x.Key1 == chatId && x.Key2 == userId).ExecuteDeleteAsync();
    }

    public new async Task<ChatUser?> GetByIdAsync(Guid chatId, Guid userId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.ChatUsers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => 
            x.Key1 == chatId 
            && x.Key2 == userId
        );
    }

    public async Task<List<ChatUser>> GetManyByChatId(Guid chatId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.ChatUsers.Where(x => x.Key1 == chatId).ToListAsync();
    }

    public async Task<List<ChatUser>> GetManyByUserId(Guid userId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.ChatUsers.Where(x => x.Key2 == userId).Include(x => x.Chat).ToListAsync();
    }
}
