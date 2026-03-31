using AdditionalHelpers.Services;
using CommunicationLibrary;
using CommunicationLibrary.Communication;
using CommunicationLibrary.ServiceRealizations;
using SpoofEntranceService.Services.Publishers;

namespace SpoofEntranceService.ServiceRealizations.Publishers;

public class UserPublisherService(
        RabbitMQSettings settings,
        ILoggerService loggerService,
        ISerializer serializer
    ) : PublisherService(
        settings,
        loggerService,
        serializer
        ), IUserPublisherService
{
    protected override string Exchange => "entrance-service";

    public async Task Create(CreateUser createUser)
    {
        await Publish("user.success.created", createUser);
        Console.WriteLine($"{createUser.UserId} publish");
    }
}