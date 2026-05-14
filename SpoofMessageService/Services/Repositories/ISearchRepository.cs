using CommonObjects.DTO;

namespace SpoofMessageService.Services.Repositories;

public interface ISearchRepository
{
    public Task<List<SearchableEntity>> GetChats(string query, Guid userId);
    public Task<List<SearchableMessage>> GetMessages(string query, Guid userId);
}
