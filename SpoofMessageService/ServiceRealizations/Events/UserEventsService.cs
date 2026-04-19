using CommonObjects.DTO;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class UserEventsService : IUserEventsService
{
    public event IUserEventsService.UserUpdatedHandler? UserUpdated;

    public void NotifyUpdate(UpdateUserInfo updateUser) =>
        UserUpdated?.Invoke(updateUser);
}