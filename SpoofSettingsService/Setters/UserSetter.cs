using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Setters;

public static class UserSetter
{
    public static void Set(this User user, ChangeUserSettingsRequest request)
    {
        user.Name = request.Name ?? user.Name;
        user.MonthsBeforeDelete = request.MonthsBeforeDelete ?? user.MonthsBeforeDelete;
        user.SearchMe = request.SearchMe ?? user.SearchMe;
        user.ShowMe = request.ShowMe ?? user.ShowMe;
        user.InviteMe = request.InviteMe ?? user.InviteMe;
        user.ForwardMessage = request.ForwardMessage ?? user.ForwardMessage;
    }

    public static UserDTO Set(this User user, byte[]? avatarToken, byte[]? avatarId, string? originalAvatarFileName) =>
        new(
            user.Name,
            user.Login,
            avatarId,
            avatarToken,
            originalAvatarFileName
        );
}
