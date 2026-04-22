using DataSaveHelpers.Services.Repositories;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface IChatRepository : ISoftDeletableIdentifiedRepository<Chat, Guid>
{
    public Task<bool> ExecuteUpdateAvatar(Guid chatId, Guid fileId, string originalFileName);
}
