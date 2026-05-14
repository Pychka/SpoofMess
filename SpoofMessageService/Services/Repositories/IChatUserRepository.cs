using DataSaveHelpers.Services.Repositories;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface IChatUserRepository : IDoubleIdentifiedRepository<ChatUser, Guid, Guid>
{
    public Task Delete(Guid chatId, Guid userId);

    public Task<List<ChatUser>> GetManyByChatId(Guid chatId);

    public Task<List<ChatUser>> GetManyByUserId(Guid userId);

    public Task<ChatUser?> GetById(Guid chatId, Guid userId);
}
