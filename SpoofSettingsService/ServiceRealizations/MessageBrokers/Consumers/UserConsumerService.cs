using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using SpoofSettingsService.Services;
using SpoofSettingsService.Services.MessageBrokers;

namespace SpoofSettingsService.ServiceRealizations.MessageBrokers.Consumers;

public class UserConsumerService(
    RabbitMQSettings settings,
    ISerializer serializer,
    ILoggerService loggerService,
    IServiceScopeFactory serviceScopeFactory) : ConsumerService(
        settings,
        serializer,
        loggerService), IUserConsumerService
{
    protected override string BaseQueueName => "settings.user";
    protected override string Exchange => "entrance-service";

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected async Task ConfirmAdded() =>
        await ConsumeFromQueueAsync<CreateUser>(
            $"success.created",
            $"user.success.created",
            async (createUser) =>
                {
                    _loggerService.Info($"{createUser.UserId} was created");
                    await _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserService>().Create(createUser);
                });

    protected async Task ConfirmDeleted() =>
        await ConsumeFromQueueAsync<CreateUser>(
            $"success.deleted",
            $"user.success.deleted",
            async (createUser) =>
            {
                _loggerService.Info($"{createUser.UserId} was deleted");
                await _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserService>().Delete(createUser.UserId);
            });

    public override async Task Initialize()
    {
        await ConfirmAdded();
        await ConfirmDeleted();
    }
}