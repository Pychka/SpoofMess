using CommonObjects.DTO;
using CommonObjects.Requests.Changes;
using CommonObjects.Responses;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Events;

public interface IChatEventService
{
    public event ChatUpdatedHandler? ChatUpdated;

    public event ChatAvatarUpdatedHandler? ChatAvatarUpdated;

    public event ChatCreatedHabdler? ChatCreated;

    public delegate void ChatUpdatedHandler(ChangeChatSettingsRequest updateChat, Chat chat);

    public delegate void ChatAvatarUpdatedHandler(ChatAvatarResponse chatAvatarResponse);

    public delegate void ChatCreatedHabdler(ChatUser chatUser);

    public void NotifyUpdate(ChangeChatSettingsRequest updateChat, Chat chat);

    public void NotifyUpdate(ChatAvatarResponse updateChat);

    public void NotifyCreate(ChatUser chatUser);
}
