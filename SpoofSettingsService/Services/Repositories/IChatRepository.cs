using DataSaveHelpers.Services.Repositories;
using SpoofSettingsService.Models;

namespace SpoofSettingsService.Services.Repositories;

public interface IChatRepository : ISoftDeletableIdentifiedRepository<Chat, Guid>
{
    public Task<Chat?> GetAsync(Guid id);
    public Task<Chat?> GetByUniqueName(string name);
    public Task Change(Chat newChat, Chat? oldChat);
}
