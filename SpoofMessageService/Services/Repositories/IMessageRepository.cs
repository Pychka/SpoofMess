using DataSaveHelpers.Services.Repositories;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface IMessageRepository : ISoftDeletableIdentifiedRepository<Message, Guid>
{
    public Task UploadAttachments(Message message);

    public Task<List<Message>> GetMessagesAfterDate(
            Guid chatId, 
            DateTime after,
            int take = 50
        );

    public Task<List<Message>> GetMessagesBeforeDate(
            Guid chatId, 
            DateTime before, 
            int take = 50
        );

    public Task<List<Message>> GetMessageSinceDate(
            Guid userId,
            DateTime after,
            int take = 50
        );
}
