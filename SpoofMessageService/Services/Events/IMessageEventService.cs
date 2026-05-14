using CommonObjects.DTO;
using CommonObjects.Responses;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Events;

public interface IMessageEventService
{
    public event MessageRecivedEventHandler OnMessageRecived;
    public event MessageEditedEventHandler OnMessageEdited;
    public event MessageDeletedEventHandler OnMessageDeleted;

    public delegate void MessageDeletedEventHandler(Guid messageId, Chat chat);

    public delegate void MessageRecivedEventHandler(MessageDTO message, Chat chat);

    public delegate void MessageEditedEventHandler(EditMessageResponse message, Chat chat);

    public void NotifyReciveMessage(MessageDTO message, Chat chat);

    public void NotifyEditMessage(EditMessageResponse message, Chat chat);
    public void NotifyDeleteMessage(Guid messageId, Chat chat);
}
