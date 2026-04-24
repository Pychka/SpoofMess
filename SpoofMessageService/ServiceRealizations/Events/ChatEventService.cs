using CommonObjects.Requests.Changes;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class ChatEventService : IChatEventService
{
    public event IChatEventService.ChatUpdatedHandler? ChatUpdated;

    public void NotifyUpdate(ChangeChatSettingsRequest updateChat, Chat chat) =>
        ChatUpdated?.Invoke(updateChat, chat);
}