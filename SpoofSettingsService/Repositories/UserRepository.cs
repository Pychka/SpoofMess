using DataHelpers.ServiceRealizations;
using DataHelpers.Services;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Repositories;

public class UserRepository(ICacheService cache, SpoofSettingsServiceContext context, ProcessQueueTasksService tasksService) : Repository<User, Guid>(cache, context, tasksService)
{
}
