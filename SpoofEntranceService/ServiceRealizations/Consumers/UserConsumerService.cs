using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using CommunicationLibrary.Services;
using SpoofEntranceService.Services;
using SpoofEntranceService.Services.Consumers;

namespace SpoofEntranceService.ServiceRealizations.Consumers;

public class UserConsumerService(
    RabbitMQSettings settings,
    ISerializer serializer,
    ILoggerService loggerService,
    IInjectionService injectionService
    ) : ConsumerService(
        settings,
        serializer,
        loggerService
        ), IUserConsumerService
{
    protected readonly IInjectionService _injectionService = injectionService;
    protected override string Exchange => "settings-service";
    protected override string BaseQueueName => "entrance.user";


    public override async Task Initialize()
    {
        await ConfirmAdded();
        await ErrorAdded();
        await ConfirmDelete();
    }


    private async Task ConfirmAdded()
    {
        await ConsumeFromQueueAsync<CreateUser>("success.created", "user.success.created", async (createUser) =>
        {
            _loggerService.Info($"{createUser.UserId} was created");
            await _injectionService.Invoke<IUserEntryService, Task>(
                async (userEntryService) =>
                    await userEntryService.Confirm(createUser.UserId)
                );
        });
    }


    private async Task ErrorAdded()
    {
        await ConsumeFromQueueAsync<CreateUser>("error.created", "user.error.created", async (createUser) =>
        {
            await _injectionService.Invoke<IUserEntryService, Task>(
                async (userEntryService) =>
                    await userEntryService.Error(createUser.UserId)
            );
        });
    }


    private async Task ConfirmDelete()
    {
        await ConsumeFromQueueAsync<CreateUser>("success.deleted", "user.success.deleted", async (deletedUser) =>
        {
            await _injectionService.Invoke<IUserEntryService, Task>(
                async (userEntryService) =>
                    await userEntryService.Delete(deletedUser.UserId)
                );
        });
    }
}