using CommonObjects.DTO;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class UserEventService : IUserEventService
{
    public event IUserEventService.UserUpdatedHandler? UserUpdated;

    public void NotifyUpdate(UpdateUserInfo updateUser, Guid userId) =>
        UserUpdated?.Invoke(updateUser, userId);
}