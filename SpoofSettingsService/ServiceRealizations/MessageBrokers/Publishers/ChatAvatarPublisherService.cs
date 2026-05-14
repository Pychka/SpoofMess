using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using SpoofSettingsService.Services.MessageBrokers;

namespace SpoofSettingsService.ServiceRealizations.MessageBrokers.Publishers;

public class ChatAvatarPublisherService(
        RabbitMQSettings settings,
        ILoggerService loggerService,
        ISerializer serializer
    ) : PublisherService(
            settings,
            loggerService,
            serializer
        ), IChatAvatarPublisherService
{
    protected override string Exchange => "settings-service";

    public async Task Publish(CreateChatAvatar chatAvatar)
    {
        await Publish("chatAvatar.success.created", chatAvatar);
    }
}