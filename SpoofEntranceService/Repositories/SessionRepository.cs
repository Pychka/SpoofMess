using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofEntranceService.Models;

namespace SpoofEntranceService.Repositories;

public class SessionRepository(ICacheService cache, SpoofEntranceServiceDbContext context, ProcessQueueTasksService tasksService) : Repository<SessionInfo, Guid>(cache, context, tasksService)
{
    public async ValueTask<List<SessionInfo>?> GetSessionsByUserId(Guid userId) =>
        await GetManyAsync("user:sessions:" + userId,
            async () => await context.SessionInfos
                .Where(x => x.UserEntryId == userId
                    && !x.IsDeleted
                    && x.IsActive)
                .ToListAsync());

    public async Task SoftDelete(SessionInfo entity)
    {
        entity.IsActive = false;
        await SoftDeleteAsync(entity);
    }

    public async Task SoftDeleteSessionsByUserId(Guid userId, bool withCurrent = false, Guid? currentSessionId = null)
    {
        await context.SessionInfos
        .Where(x => x.
            UserEntryId == userId
            && !x.IsDeleted
            && x.IsActive
            && (withCurrent
            || x.Id != currentSessionId))
        .ExecuteUpdateAsync(si => si
            .SetProperty(prop => prop.IsActive, false)
            .SetProperty(prop => prop.IsDeleted, true));
    }
}