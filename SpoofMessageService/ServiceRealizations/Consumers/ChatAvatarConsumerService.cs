using AdditionalHelpers.Services;
using CommonObjects.Results;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using CommunicationLibrary.Services;
using SpoofMessageService.Services;

namespace SpoofMessageService.ServiceRealizations.Consumers;

public class ChatAvatarConsumerService(
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

    protected override string BaseQueueName => "message.chatAvatar";

    protected override string Exchange => "settings-service";


    public override async Task Initialize()
    {
        await SuccessCreated();
    }


    private async Task SuccessCreated()
    {
        await ConsumeFromQueueAsync<CreateChatAvatar>("success.created", "chatAvatar.success.created", async (createChatAvatar) =>
        {
            await _injectionService.Invoke<IChatAvatarService, Task<Result>>(async (chatService) =>
            {
                return await chatService.Create(createChatAvatar);
            });
        });
    }
}
