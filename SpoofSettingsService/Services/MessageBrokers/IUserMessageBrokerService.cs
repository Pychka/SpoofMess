using CommunicationLibrary.Communication;

namespace SpoofSettingsService.Services.MessageBrokers;

public interface IUserMessageBrokerService
{
    public Task ConfirmCreate(CreateUser createUser);
    public Task ConfirmUpdate(UpdateUser updateUser);
    public Task ConfirmDelete(CreateUser createUser);
    public Task ErrorAdded(CreateUser createUser);
}
