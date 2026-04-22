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
        return await context.Users
            .Where(x => !x.IsDeleted
                        && (string.Equals(x.Name, query, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.Login, query, StringComparison.CurrentCultureIgnoreCase)))
            .Select(x => new SearchableEntity(x.Id,
                                              x.Name,
                                              x.Login))
            .Union(context.Chats.Where(x =>
                        !x.IsDeleted
                        && (string.Equals(x.Name, query, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.UniqueName, query, StringComparison.CurrentCultureIgnoreCase)))
            .Select(x => new SearchableEntity(x.Id,
                                              x.Name ?? string.Empty,
                                              x.UniqueName))).ToListAsync();
    }
}
