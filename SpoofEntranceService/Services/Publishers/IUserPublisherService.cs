using CommunicationLibrary.Communication;

namespace SpoofEntranceService.Services.Publishers;

public interface IUserPublisherService
{
    public Task Create(CreateUser createUser);
}
