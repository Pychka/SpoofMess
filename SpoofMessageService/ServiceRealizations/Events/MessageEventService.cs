using CommonObjects.DTO;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Events;

namespace SpoofMessageService.ServiceRealizations.Events;

public class MessageEventService : IMessageEventService
{
    public event IMessageEventService.MessageRecivedEventHandler? OnMessageRecived;

    public void ReciveMessage(MessageDTO message, Chat chat) =>
        OnMessageRecived?.Invoke(message, chat);
}