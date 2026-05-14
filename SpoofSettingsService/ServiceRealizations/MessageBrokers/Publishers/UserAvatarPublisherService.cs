using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using SpoofSettingsService.Services.MessageBrokers;

namespace SpoofSettingsService.ServiceRealizations.MessageBrokers.Publishers;

public class UserAvatarPublisherService(
        RabbitMQSettings settings,
        ILoggerService loggerService,
        ISerializer serializer
    ) : PublisherService(
            settings,
            loggerService,
            serializer
        ), IUserAvatarPublisherService
{
    protected override string Exchange => "settings-service";

    public async Task Publish(CreateUserAvatar userAvatar)
    {
        await Publish("userAvatar.success.created", userAvatar);
    }
}