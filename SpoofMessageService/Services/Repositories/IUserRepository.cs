using DataSaveHelpers.Services.Repositories;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface IUserRepository : IIdentifiedRepository<User, Guid>
{
    public Task<bool> ExecuteUpdateConnection(Guid userId, bool isConnected);
    public Task<bool> ExecuteUpdateAvatar(Guid userId, Guid fileId, string originalFileName);
    public Task<int> GetCountActiveUsers();
}
