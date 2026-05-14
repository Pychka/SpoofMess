using CommunicationLibrary.Communication;

namespace SpoofSettingsService.Services.MessageBrokers;

public interface IUserAvatarPublisherService
{
    public Task Publish(CreateUserAvatar createUserAvatar);
}