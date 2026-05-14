using CommonObjects.Requests.Avatars;
using CommonObjects.Responses;
using CommonObjects.Results;

namespace SpoofSettingsService.Services;

public interface IUserAvatarService
{
    public Task<Result<AvatarResponse>> GetAvatar(byte[] accessToken, Guid userId);

    public Task<Result<AvatarResponse>> GetActualAvatar(GetUserAvatarRequest request, Guid userId);

    public Task<Result> SetAvatar(SesUserAvatarRequest request, Guid userId);

    public Task<Result<List<AvatarResponse>>> GetAvatars(GetUserAvatarRequest request, Guid userId);

    public Task<Result> RemoveAvatar(RemoveUserAvatarRequest request, Guid userId);
}
