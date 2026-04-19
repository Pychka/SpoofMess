using CommonObjects.DTO;
using CommonObjects.Results;
using CommunicationLibrary.Communication;

namespace SpoofMessageService.Services;

public interface IUserService
{
    public Task<Result> Create(CreateUser createUser);

    public Task<Result> Update(UpdateUser updateUser);

    public Task<Result> Delete(
            Guid userId
        );
    public Task<Result> ChangeConnectionState(
            Guid userId, 
            bool state
        );
}
