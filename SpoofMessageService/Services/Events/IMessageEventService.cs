using CommonObjects.DTO;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Events;

public interface IMessageEventService
{
    public event MessageRecivedEventHandler OnMessageRecived;

    public delegate void MessageRecivedEventHandler(MessageDTO message, Chat chat);

    public void ReciveMessage(MessageDTO message, Chat chat);
}
