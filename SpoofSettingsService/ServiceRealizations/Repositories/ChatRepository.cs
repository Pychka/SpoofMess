using DataSaveHelpers.ServiceRealizations.Repositories.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services.Repositories;

namespace SpoofSettingsService.ServiceRealizations.Repositories;

public class ChatRepository(ICacheService cache, SpoofSettingsServiceContext context, IProcessQueueTasksService tasksService) : CachedSoftDeletableIdentifiedRepository<Chat, Guid>(cache, context, tasksService), IChatRepository
{
    public async Task<Chat?> GetAsync(Guid id) =>
        await GetAsync(GetKey(id), async () => await context.Chats.Include(x => x.ChatAvatars).Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == id));

    public async Task<Chat?> GetByUniqueName(string name) =>
        await GetAsync(name, async () => await context.Chats.FirstOrDefaultAsync(x => x.UniqueName == name));


    public async Task Change(Chat newChat, Chat? oldChat)
    {
        using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (oldChat is null)
                await AddAsync(newChat);
            else
            {
                oldChat.IsDeleted = true;
                oldChat.LastModified = DateTime.UtcNow;
                oldChat.UniqueName = Guid.CreateVersion7().ToString();
                context.Chats.Update(oldChat);
                await context.AddAsync(newChat);
                await context.SaveChangesAsync();
                SaveToCache(GetKey(newChat), newChat);
                _processQueueTasks.AddTask(async () => await _cache.Delete(GetKey(oldChat)));
            }
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message, ex); //for now we throws all exceptions
        }
    }
}