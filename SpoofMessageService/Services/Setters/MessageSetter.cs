using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Setters;

public static class MessageSetter
{
    public static void Set(this Message message, EditMessageRequest request)
    {
        message.Text = request.Text ?? message.Text;
        message.Attachments.Clear();
    }
    public static Message Set(this CreateMessageRequest request, Guid userId) =>
        new() { 
            ChatId = request.ChatId,
            UserId = userId,
            Text = request.Text 
        };

    public static MessageDTO Set(this Message message, List<CommonObjects.Requests.Attachments.Attachment> attachments) =>
        new(
            message.Id, 
            message.ChatId, 
            message.User.Login,
            message.User.Name, 
            message.Text, 
            message.SentAt,
            attachments
            );
}
