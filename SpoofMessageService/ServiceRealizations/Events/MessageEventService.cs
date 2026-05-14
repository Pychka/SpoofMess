using CommonObjects.DTO;
using CommonObjects.Responses;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class MessageEventService : IMessageEventService
{
    public event IMessageEventService.MessageRecivedEventHandler? OnMessageRecived;
    public event IMessageEventService.MessageEditedEventHandler? OnMessageEdited;
    public event IMessageEventService.MessageDeletedEventHandler? OnMessageDeleted;

    public void NotifyDeleteMessage(Guid messageId, Chat chat) =>
        OnMessageDeleted?.Invoke(messageId, chat);

    public void NotifyEditMessage(EditMessageResponse message, Chat chat) =>
        OnMessageEdited?.Invoke(message, chat);

    public void NotifyReciveMessage(MessageDTO message, Chat chat) =>
        OnMessageRecived?.Invoke(message, chat);
}