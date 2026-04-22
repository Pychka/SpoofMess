using CommonObjects.Results;
using CommunicationLibrary.Communication;

namespace SpoofMessageService.Services;

public interface IChatAvatarService
{
    public Task<Result> Create(CreateChatAvatar createChatAvatar);
}
