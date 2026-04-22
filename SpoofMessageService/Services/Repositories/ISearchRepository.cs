using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Repositories;

public interface ISearchRepository
{
    public Task<List<SearchableEntity>> GetChats(string query, Guid userId);
}
