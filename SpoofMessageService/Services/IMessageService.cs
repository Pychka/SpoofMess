using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Responses;
using CommonObjects.Results;

namespace SpoofMessageService.Services;

public interface IMessageService
{
    [Obsolete("Need check permissions for message content")]
    public Task<Result<MessageDTO>> SendMessage(
        CreateMessageRequest request, 
        Guid userId
    );
    public Task<Result> DeleteMessage(
        Guid messageId, 
        Guid chatId,
        Guid userId
    );


    [Obsolete("Need check permissions for message content")]
    public Task<Result<EditMessageResponse>> EditMessage(
        EditMessageRequest request, 
        Guid userId
    );

    public Task<Result<List<MessageDTO>>> GetMessagesAfterDate(
        Guid chatId,
        Guid userId,
        DateTime date,
        int take = 50
    );

    public Task<Result<List<MessageDTO>>> GetMessagesBeforeDate(
        Guid chatId,
        Guid userId,
        DateTime date,
        int take = 50
    );
    public Task<Result<List<MessageDTO>>> GetSkippedMessages(
        Guid userId,
        DateTime after,
        int take = 50
    );
}
