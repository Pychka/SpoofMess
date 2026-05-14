using CommonObjects.DTO;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Repositories;

namespace SpoofMessageService.ServiceRealizations.Repositories;

public class SearchRepository(ICacheService cache,
    IDbContextFactory<SpoofMessageServiceContext> factory,
    IProcessQueueTasksService processQueueTasks) : ISearchRepository
{
    private readonly ICacheService _cache = cache;
    private readonly IDbContextFactory<SpoofMessageServiceContext> _factory = factory;
    private readonly IProcessQueueTasksService _processQueueTasks = processQueueTasks;

    public async Task<List<SearchableEntity>> GetChats(string query, Guid userId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        query = $"%{query}%";
        return await context.Database.SqlQuery<SearchableEntity>(
            $@"SELECT  c.""Id"", 0 ""Type"", c.""AvatarId"", c.""OriginalFileName"", c.""Name"", c.""UniqueName"" FROM ""Chat"" c where c.""Name"" ILIKE {query} or c.""UniqueName"" ILIKE {query}
                UNION
                SELECT  u.""Id"", 1 ""Type"", u.""AvatarId"", u.""OriginalFileName"", u.""Name"", u.""Login"" AS ""UniqueName"" FROM ""User"" u where u.""Name"" ILIKE {query} or u.""Login"" ILIKE {query}")
        .ToListAsync();
    }

    public async Task<List<SearchableMessage>> GetMessages(string query, Guid userId)
    {
        await using SpoofMessageServiceContext context = await _factory.CreateDbContextAsync();
        query = $"%{query}%";
        return await context.Database.SqlQuery<SearchableMessage>(
            $@"with chats as (
                select cu.""ChatId"" from ""ChatUser"" cu where cu.""UserId"" = {userId}
                )
                SELECT m.""ChatId"", m.""Id"", m.""Text"", m.""SentAt"" from chats c
                join ""Message"" m on m.""ChatId"" = c.""ChatId"" and m.""Text"" ilike {query}
                ORDER by  m.""SentAt"" desc")
        .ToListAsync();
    }
}