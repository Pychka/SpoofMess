using CommonObjects.Requests.Changes;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Events;

public interface IChatEventService
{
    public event ChatUpdatedHandler? ChatUpdated;

    public delegate void ChatUpdatedHandler(ChangeChatSettingsRequest updateChat, Chat chat);

    public void NotifyUpdate(ChangeChatSettingsRequest updateChat, Chat chat);
}
