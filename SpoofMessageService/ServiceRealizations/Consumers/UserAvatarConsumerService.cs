using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using CommunicationLibrary.Services;
using SpoofMessageService.Services;

namespace SpoofMessageService.ServiceRealizations.Consumers;

public class UserAvatarConsumerService(
    RabbitMQSettings settings,
    IInjectionService injectionService,
    ISerializer serializer,
    ILoggerService loggerService
    ) : ConsumerService(
        settings,
        serializer,
        loggerService
        )
{
    protected readonly IInjectionService _injectionService = injectionService;

    protected override string BaseQueueName => "message.userAvatar";

    protected override string Exchange => "settings-service";


    public override async Task Initialize()
    {
        await SuccessCreated();
    }


    private async Task SuccessCreated()
    {
        await ConsumeFromQueueAsync<CreateUserAvatar>("success.created", "userAvatar.success.created", async (createUserAvatar) =>
        {
            await _injectionService.Invoke<IUserAvatarService, Task<Result>>(async (userAvatarService) =>
            {
                return await userAvatarService.Create(createUserAvatar);
            });
        });
    }
}