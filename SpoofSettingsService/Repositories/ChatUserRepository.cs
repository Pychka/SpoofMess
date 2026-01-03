using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Repositories;

public class ChatUserRepository(ICacheService cache, SpoofSettingsServiceContext context, ProcessQueueTasksService tasksService) : Repository<ChatUser, Guid>(cache, context, tasksService)
{
    public async Task<bool> DeleteMemberById(Guid memberId, Guid chatId)
    {
        ChatUser? member = await GetAsync($"{typeof(ChatUser).Name.ToLower()}:{chatId}-{memberId}",
            async() => await context.ChatUsers.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == memberId));
        if (member is null)
            return false;

        await SoftDeleteAsync(member);
        return true;
    }
}
