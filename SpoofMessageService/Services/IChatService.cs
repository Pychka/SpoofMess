using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using CommunicationLibrary.Communication;

namespace SpoofMessageService.Services;

public interface IChatService
{
    public Task<Result> Create(CreateChat createUser);

    public Task<Result> Update(ChangeChatSettingsRequest request, Guid userId);

    public Task<Result> Delete();
}
