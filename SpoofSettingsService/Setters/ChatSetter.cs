using CommonObjects.Requests;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Setter;

public static class ChatSetter
{
    public static void Set(this Chat chat, ChangeChatSettingsRequest request)
    {
        chat.OwnerId = request.OwnerId ?? chat.OwnerId;
        chat.ChatTypeId = request.ChatTypeId ?? chat.ChatTypeId;
        chat.ChatName = request.ChatName ?? chat.ChatName;
        chat.IsPublic = request.IsPublic ?? chat.IsPublic;
        chat.UniqueName = request.UniqueName ?? chat.UniqueName;
    }
}
