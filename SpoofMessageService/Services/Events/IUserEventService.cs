using CommonObjects.DTO;

namespace SpoofMessageService.Services.Events;

public interface IUserEventService
{
    public event UserUpdatedHandler? UserUpdated;

    public delegate void UserUpdatedHandler(UpdateUserInfo user, Guid userId);

    public void NotifyUpdate(UpdateUserInfo updateUser, Guid userId);
}
