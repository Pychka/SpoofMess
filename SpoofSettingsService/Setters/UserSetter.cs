using CommonObjects.Requests;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Setter
{
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
    }
}
