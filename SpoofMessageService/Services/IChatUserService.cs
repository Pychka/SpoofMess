using CommonObjects.Results;
using CommunicationLibrary.Communication;
using SpoofMessageService.Models;
using SpoofMessageService.Models.Enums;

namespace SpoofMessageService.Services;

public interface IChatUserService
{
    public Task<Result<ChatUser>> GetMember(Guid chatId, Guid userId);

    public Task<Result<List<ChatUser>>> GetMembers(Guid chatId);

    public Task<Result<List<ChatUser>>> GetChats(Guid userId);

    public Task<Result> Update(UpdateChatUser updateChatUser);

    public Task<Result> Add(CreateChatUser createChatUser);

    public Task<Result<ChatUser>> GetAndCheckPermission(Guid chatId, Guid userId, Rules rule);

    public Task<Result> Delete(Guid chatId, Guid userId);
}
