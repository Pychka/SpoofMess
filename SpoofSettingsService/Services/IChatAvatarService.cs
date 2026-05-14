using CommonObjects.Requests.Avatars;
using CommonObjects.Responses;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IChatAvatarService
{
    public Task<Result> SetAvatar(SetChatAvatarRequest request, Guid userId);

    public Task<Result<AvatarResponse>> GetAvatar(byte[] accessToken, Guid userId);

    public Task<Result<AvatarResponse>> GetActualAvatar(GetChatAvatarRequest request, Guid userId);

    public Task<Result<List<AvatarResponse>>> GetAvatars(GetChatAvatarRequest request, Guid userId);

    public Task<Result> RemoveAvatar(RemoveChatAvatarRequest request, Guid userId);
}
