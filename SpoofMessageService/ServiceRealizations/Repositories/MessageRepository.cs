using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class MessageRepository(
        ICacheService cache,
        IDbContextFactory<SpoofMessageServiceContext> factory, 
        IProcessQueueTasksService processQueueTasks
    ) 
    : CachedSoftDeletableIdentifiedFactoryRepository<Message, Guid, SpoofMessageServiceContext>(
            cache,
            factory,
            processQueueTasks
        ), IMessageRepository
{
    public async Task<int> GetCount()
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return context.Messages.Where(x => !x.IsDeleted).Count();
    }

    public async Task Save(Message message)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        foreach (Attachment attachment in message.Attachments)
            context.Attach(attachment);
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
    }
    public async Task Update(Message message)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        foreach (Attachment attachment in message.Attachments)
            context.Attach(attachment);
        context.Messages.Update(message);
        await context.SaveChangesAsync();
    }

    public async Task<List<Message>> GetMessagesAfterDate(
            Guid chatId,
            DateTime after,
            int take = 50
        )
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.Messages.OrderBy(x => x.SentAt)
            .Include(x => x.User)
            .Include(x => x.Attachments)
            .ThenInclude(x => x.FileMetadata)
            .Where(m =>
                m.ChatId == chatId
                && m.SentAt >= after
            ).Take(take)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesBeforeDate(
            Guid chatId, 
            DateTime before,
            int take = 50
        )
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.Messages.OrderByDescending(x => x.SentAt)
            .Include(x => x.User)
            .Include(x => x.Attachments)
            .ThenInclude(x => x.FileMetadata)
            .Where(m =>
                m.ChatId == chatId
                && m.SentAt <= before
            ).Take(take)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessageSinceDate(
            Guid userId,
            DateTime after, 
            int take = 50
        )
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        return await context.Messages.OrderBy(x => x.SentAt)
            .Include(x => x.User)
            .Include(x => x.Attachments)
            .ThenInclude(x => x.FileMetadata)
            .Include(x => x.Chat)
            .ThenInclude(x => x.ChatUsers)
            .Where(x => 
                x.Chat.ChatUsers.Any(cu => cu.Key2 == userId)
                && x.SentAt >= after
            ).Take(take)
            .ToListAsync();
    }

    public async Task UploadAttachments(Message message)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        List<Guid> attachmentIds = [.. message.Attachments.Select(y => y.Id)];
        List<Attachment> loadedAttachments = await context.Attachments
            .Include(x => x.FileMetadata)
            .Where(x => x.MessageId == message.Id && attachmentIds.Contains(x.FileMetadataId))
            .ToListAsync();

        message.Attachments.Clear();
        foreach (Attachment attr in loadedAttachments)
        {
            message.Attachments.Add(attr);
        }
    }
}
