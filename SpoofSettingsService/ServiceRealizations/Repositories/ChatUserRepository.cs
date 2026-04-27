using CommonObjects.DTO;
using DataSaveHelpers.ServiceRealizations.Repositories.Factory.WithCache;
using DataSaveHelpers.Services;
using Microsoft.EntityFrameworkCore;
using SpoofSettingsService.Models;
using SpoofSettingsService.Services.Repositories;

namespace SpoofSettingsService.ServiceRealizations.Repositories;

public class ChatUserRepository(
        ICacheService cache,
        IDbContextFactory<SpoofSettingsServiceContext> factory,
        IProcessQueueTasksService tasksService
    ) : CachedSoftDeletableDoubleIdentifiedFactoryRepository<ChatUser, Guid, Guid, SpoofSettingsServiceContext>(
            cache,
            factory,
            tasksService
        ), IChatUserRepository
{
    public async Task<ChatUser?> GetWithRules(Guid chatId, Guid userId)
    {
        return await GetAsync(
            GetKey(chatId, userId),
            async() =>
            {
                await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
                return await context.ChatUsers.
                    Include(x => x.ChatUserRules)
                    .Include(x => x.ChatUserChatRoles)
                    .FirstOrDefaultAsync(x =>
                        x.Key1 == chatId
                        && x.Key2 == userId
                    );
            });
    }

    public async Task<List<ChatUserDTO>> GetUserChatsAfterDate(Guid userId, DateTime after)
    {
        await using SpoofSettingsServiceContext context = await _factory.CreateDbContextAsync();
        return await context.Database.SqlQuery<ChatUserDTO>(
            $@"SELECT c.""Id"", c.""ChatTypeId"", c.""UniqueName"", c.""Name"", c.""CreatedAt"", ca.""FileId"", ca.""OriginalFileName"", ca.""Id"" AS ""AvatarId"", fm.""Metadata"",
               jsonb_agg(perm::permission_result) AS ""RulesJson""
               FROM ""ChatUser"" cu JOIN ""Chat"" c ON c.""Id"" = cu.""ChatId"" 
               LEFT JOIN LATERAL get_user_permission({userId}, cu.""ChatId"", null) perm ON true
               LEFT JOIN ""ChatAvatar"" ca ON c.""Id"" = ca.""ChatId""
               LEFT JOIN ""FileMetadata"" fm ON fm.""Id"" = ca.""FileId""
               WHERE cu.""UserId"" = {userId} AND cu.""JoinedAt"" > {after}
               GROUP BY c.""Id"", c.""ChatTypeId"", c.""UniqueName"", c.""Name"", ca.""FileId"", ca.""OriginalFileName"", ca.""Id"", fm.""Metadata"""
           ).ToListAsync();
    }
}