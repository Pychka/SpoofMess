using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Setters;

public static class ChatSetter
{
    public static void Set(this Chat chat, ChangeChatSettingsRequest request)
    {
        chat.OwnerId = request.OwnerId ?? chat.OwnerId;
        chat.ChatTypeId = request.ChatTypeId ?? chat.ChatTypeId;
        chat.Name = request.ChatName ?? chat.Name;
        chat.UniqueName = request.UniqueName ?? chat.UniqueName;
    }

    public static ChatDTO Set(this Chat chat, byte[]? avatarToken, byte[]? avatarId) =>
        new(
            chat.Id,
            chat.ChatTypeId,
            chat.UniqueName,
            chat.Name,
            avatarToken,
            avatarId,
            chat.ActualAvatar?.OriginalFileName,
            chat.CreatedAt,
            chat.OwnerId
            );
}