using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using CommunicationLibrary.Services;
using SpoofMessageService.Services;

namespace SpoofMessageService.ServiceRealizations.Consumers;

public class UserSSSConsumerService(
    RabbitMQSettings settings,
    ISerializer serializer,
    ILoggerService loggerService,
    IInjectionService injectionService
    ) : ConsumerService(
        settings,
        serializer,
        loggerService
        )
{
    protected readonly IInjectionService _injectionService = injectionService;
    protected override string Exchange => "settings-service";
    protected override string BaseQueueName => "message.user";

    public override async Task Initialize()
    {
        await ConfirmUpdated();
    }

    public async Task ConfirmUpdated()
    {
        await ConsumeFromQueueAsync<UpdateUser>(
            "success.updated",
            "user.success.updated",
            async (updateUser) =>
            {
                await _injectionService.Invoke<IUserService, Task>(
                    async (userEntryService) => await userEntryService.Update(updateUser));
                _loggerService.Info($"{updateUser.UserId} was updated");
            });
    }
}
