using CommunicationLibrary.Communication;

namespace SpoofSettingsService.Services.MessageBrokers;

public interface IChatAvatarPublisherService
{
    public Task Publish(CreateChatAvatar createChatAvatar);
}
