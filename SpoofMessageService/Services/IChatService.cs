using CommonObjects.Requests.Changes;
using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services;

public interface IChatService
{
    public Task<Result<Chat>> Get(Guid chatId);
    public Task<Result> Create(CreateChat createUser);

    public Task<Result> Update(ChangeChatSettingsRequest request, Guid userId);

    public Task<Result> Delete();
}
