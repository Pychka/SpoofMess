using CommonObjects.Requests.Changes;
using CommonObjects.Responses;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class ChatEventService : IChatEventService
{
    public event IChatEventService.ChatUpdatedHandler? ChatUpdated;
    public event IChatEventService.ChatAvatarUpdatedHandler? ChatAvatarUpdated;
    public event IChatEventService.ChatCreatedHabdler? ChatCreated;

    public void NotifyCreate(ChatUser chatUser) =>
        ChatCreated?.Invoke(chatUser);

    public void NotifyUpdate(ChangeChatSettingsRequest updateChat, Chat chat) =>
        ChatUpdated?.Invoke(updateChat, chat);

    public void NotifyUpdate(ChatAvatarResponse updateChat) =>
        ChatAvatarUpdated?.Invoke(updateChat);

}