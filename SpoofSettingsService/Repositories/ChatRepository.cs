using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Repositories;

public class ChatRepository(ICacheService cache, SpoofSettingsServiceContext context, ProcessQueueTasksService tasksService) : Repository<Chat, long>(cache, context, tasksService)
{
    public async Task<Chat?> GetByUniqueName(string name) =>
        await GetAsync(name, async () => await context.Chats.FirstOrDefaultAsync(x => x.UniqueName == name));


    protected override void ChangeEntity(Chat entity)
    {
        entity.LastModified = DateTime.UtcNow;
        base.ChangeEntity(entity);
    }

    public async Task Change(Chat newChat, Chat? oldChat)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
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
                SaveToCaches(GetKey(newChat), newChat);
                _processQueueTasks.AddTask(async () => await _cache.Delete(GetKey(oldChat)));

                await transaction.CommitAsync();
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message, ex); //for now we throws all exceptions
        }
    }
}