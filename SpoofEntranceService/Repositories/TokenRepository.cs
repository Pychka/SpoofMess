using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using SpoofEntranceService.Models;
using Microsoft.EntityFrameworkCore;

namespace SpoofEntranceService.Repositories;

public class TokenRepository(ICacheService cache, SpoofEntranceServiceDbContext context, ProcessQueueTasksService tasksService) : Repository<Token, string>(cache, context, tasksService)
{
    public async ValueTask Add(Token token) =>
        await AddAsync(token);

    public async ValueTask<Token?> GetByRefreshHash(string refreshHash) =>
        await GetAsync(GetKey(refreshHash),
            async () => await _set
                .Include(x => x.SessionInfo)
                .ThenInclude(x => x.UserEntry)
                .FirstOrDefaultAsync(x => refreshHash == x.Id));

    public async Task Replace(Token replaced, Token replacing)
    {
        replaced.IsDeleted = true;
        _context.Entry(replaced).State = EntityState.Modified;
        await _set.AddAsync(replacing);
        await _context.SaveChangesAsync();
    }
}
